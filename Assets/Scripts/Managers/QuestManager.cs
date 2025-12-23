using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class QuestManager : MonoBehaviour
{
    public static QuestManager Instance;

    [Header("Database Semua Quest di Game")]
    public List<QuestData> allQuests;

    private Dictionary<QuestData, int> progress = new();
    public QuestUIManager questUI;

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

    // MULAI QUEST
    public void StartQuest(QuestData q)
    {
        SaveLoadSystem.Instance.SaveGame();

        if (progress.ContainsKey(q))
        {
            Debug.Log("Quest sudah diambil, tidak bisa double");
            return;
        }

        progress[q] = 0;
        questUI.ShowQuest(q);

        Debug.Log("Quest Dimulai: " + q.questTitle);
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
        SaveLoadSystem.Instance.SaveGame();

        Debug.Log("🏆 Quest Completed: " + q.questTitle);

        completed.Add(q);
        questUI.MarkCompleted(q);

        // progress tetap ada → menunggu player tekan Claim
    }

    // Pemain menekan tombol CLAIM
    public void ClaimQuest(QuestData q)
    {
        SaveLoadSystem.Instance.SaveGame();

        Debug.Log("🎁 Quest Claimed: " + q.questTitle);

        GameManager.Instance.AddXP(q.rewardXP);
        GameManager.Instance.AddCoins(q.rewardCoins);
        GameManager.Instance.ModifyPollution(q.rewardPollutionReduction);

        questUI.MarkClaimed(q);

        progress.Remove(q);
    }

    public void FailQuest(QuestData q)
    {
        SaveLoadSystem.Instance.SaveGame();

        if (!progress.ContainsKey(q)) return;

        Debug.Log("❌ Quest Failed: " + q.questTitle);

        failed.Add(q);
        questUI.MarkFailed(q);

        progress.Remove(q);
    }

    public bool HasQuest(QuestData q)
    {
        return progress.ContainsKey(q);
    }
}