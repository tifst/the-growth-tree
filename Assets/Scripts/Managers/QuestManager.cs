using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System.Collections;

public class QuestManager : MonoBehaviour
{
    public static QuestManager Instance;
    public QuestUIManager questUI;

    [Header("Database Semua Quest di Game")]
    public List<QuestData> allQuests;
    public List<QuestQueueManager> questQueues;

    [Header("Quest Points")]
    public List<QuestPointController> questPoints;

    private Dictionary<QuestData, int> progress = new();
    private Dictionary<QuestData, int> lastFailedProgress = new();

    public List<QuestData> completed = new();
    public List<QuestData> failed = new();
    public bool IsCompleted(QuestData q) => completed.Contains(q);

    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    void Start()
    {
        foreach (var queue in questQueues)
        {
            queue.OnNPCSpawned += npc =>
            {
                GuideManager.Instance.AddTarget(npc.transform);
            };

            queue.OnNPCRemoved += () =>
            {
            };
        }
    }

    void Update()
    {
        int level = GameManager.Instance.level;

        foreach (var qp in questPoints)
        {
            qp.queue.TrySpawnNext(level);
        }
    }

    QuestQueueManager GetQueue(QuestDificulty diff)
    {
        return questQueues.Find(q => q.difficulty == diff);
    }

    // MULAI QUEST
    public void StartQuest(QuestData q)
    {
        if (progress.ContainsKey(q))
        {
            Debug.Log("Quest sudah diambil, tidak bisa double");
            return;
        }

        progress[q] = 0;
        questUI.ShowQuest(q, false);

        Debug.Log("Quest Dimulai: " + q.questTitle);
        SaveLoadSystem.Instance.SaveGame();
    }

    // TAMBAH PROGRESS
    public void AddProgress(string target, QuestGoalType goalType)
    {
        List<QuestData> toComplete = new();
        var keys = progress.Keys.ToList(); // aman dari modify

        foreach (var q in keys)
        {
            if (q.goalType != goalType) continue;
            if (q.targetName != "Any" && q.targetName != target) continue;

            progress[q]++;
            questUI.UpdateProgress(q, progress[q], q.requiredAmount);

            if (progress[q] >= q.requiredAmount)
                toComplete.Add(q);
        }

        foreach (var q in toComplete)
            CompleteQuest(q);
    }

    // PROGRESS COMPLETE → tombol claim aktif
    public void CompleteQuest(QuestData q)
    {
        PromptManager.Instance.Notify("Quest " + q.questID + ": " + q.questTitle + " Completed", 5f);

        completed.Add(q);
        questUI.MarkCompleted(q);
    }

    // Pemain menekan tombol CLAIM
    public void ClaimQuest(QuestData q)
    {
        Debug.Log("Quest Claimed: " + q.questTitle);

        GameManager.Instance.AddXP(q.rewardXP);
        GameManager.Instance.AddCoins(q.rewardCoins);

        questUI.MarkClaimed(q);

        progress.Remove(q);
        
        QuestQueueManager queue = GetQueue(q.difficulty);
        if (queue != null)
            queue.OnQuestFinished();

        SaveLoadSystem.Instance.SaveGame();
    }

    public void FailQuest(QuestData q)
    {
        if (!progress.ContainsKey(q)) return;

        Debug.Log("Quest Failed: " + q.questTitle);

        lastFailedProgress[q] = progress[q];
        failed.Add(q);
        questUI.MarkFailed(q);

        progress.Remove(q);
        
        QuestQueueManager queue = GetQueue(q.difficulty);
        if (queue != null)
            queue.OnQuestFinished();

        SaveLoadSystem.Instance.SaveGame();
    }

    public bool HasQuest(QuestData q)
    {
        return progress.ContainsKey(q);
    }

    public int GetProgress(QuestData q)
    {
        return progress.ContainsKey(q) ? progress[q] : 0;
    }

    public void ResetAll()
    {
        progress.Clear();
        completed.Clear();
        failed.Clear();

        questUI.ResetAll();

        foreach (var queue in questQueues)
            queue.ResetQueue();
    }

    public QuestSaveData ExportState()
    {
        QuestSaveData data = new QuestSaveData();

        // ===== QUEST STATE =====
        foreach (var q in allQuests)
        {
            int savedProgress = 0;

            if (HasQuest(q))
                savedProgress = progress[q];
            else if (failed.Contains(q))
                savedProgress = lastFailedProgress.ContainsKey(q)
                ? lastFailedProgress[q] : 0;

            data.quests.Add(new QuestSaveInfo
            {
                questID = q.questID,
                progress = savedProgress,
                active = HasQuest(q) || failed.Contains(q),
                startTime = questUI.GetStartTime(q),
                remainingTime = questUI.GetRemainingTime(q),
                finishTime = questUI.GetFinishedTime(q),
                completed = completed.Contains(q),
                failed = failed.Contains(q),
                claimed = questUI.IsClaimed(q), // 🔥 JANGAN INFER
            });
        }

        // ===== QUEUE STATE (🔥 INI FIX UTAMA) =====
        foreach (var queue in questQueues)
        {
            data.queues.Add(new QuestQueueSaveInfo
            {
                difficulty = queue.difficulty,
                currentIndex = queue.CurrentIndex,
                activeQuestID = queue.ActiveQuestID,
                hasActiveNPC = queue.HasActiveNPC,
                returnPointIndex = queue.GetActiveReturnPointIndex()
            });
        }

        return data;
    }

    public void ImportState(QuestSaveData data)
    {
        progress.Clear();
        completed.Clear();
        failed.Clear();

        // ====== PASS 1: SPAWN SEMUA QUEST TANPA URUTAN ======
        foreach (var qs in data.quests)
        {
            QuestData q = allQuests.Find(x => x.questID == qs.questID);
            if (q == null) continue;

            if (!qs.active && !qs.completed && !qs.failed)
                continue;

            questUI.ShowQuest(q, true);
        }

        // ====== PASS 2: RESTORE STATE ======
        foreach (var qs in data.quests)
        {
            QuestData q = allQuests.Find(x => x.questID == qs.questID);
            if (q == null) continue;

            // START TIME
            if (qs.active)
            {
                progress[q] = qs.progress;
                questUI.RestoreStartTime(q, qs.startTime);
                questUI.RestoreRemainingTime(q, qs.remainingTime);
                questUI.UpdateProgress(q, qs.progress, q.requiredAmount);
            }

            // COMPLETED
            if (qs.completed)
            {
                completed.Add(q);
                questUI.MarkCompleted(q);

                if (qs.claimed)
                    questUI.MarkClaimed(q);
            }

            // FAILED
            if (qs.failed)
            {
                failed.Add(q);
                questUI.MarkFailed(q);

                questUI.UpdateProgress(q, qs.progress, q.requiredAmount);
            }

            // FINISHED TIME (🔥 INI PENTING)
            if (qs.completed || qs.failed)
            {
                questUI.RestoreFinishedTime(q, qs.finishTime);
            }
        }

        // ====== PASS 3: REORDER SEKALI ======
        questUI.ForceReorder();

        // ====== RESTORE QUEUE ======
        foreach (var q in data.queues)
        {
            QuestQueueManager queue = GetQueue(q.difficulty);
            if (queue != null)
            {
                queue.RestoreFromSave(
                    q.currentIndex,
                    q.activeQuestID,
                    q.hasActiveNPC,
                    q.returnPointIndex
                );
            }
        }
    }
}