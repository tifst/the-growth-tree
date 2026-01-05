using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections.Generic;

enum QuestUIState
{
    CompletedUnclaimed,
    Active,
    Finished
}

public class QuestUIManager : MonoBehaviour
{
    public static QuestUIManager Instance;

    [Header("UI References")]
    public GameObject questPanel;
    public GameObject panelPrefab;
    public Transform panelParent;

    private Dictionary<QuestData, QuestItem> slots = new();
    private Dictionary<QuestData, float> startTime = new();
    private Dictionary<QuestData, float> remainingTime = new();
    private Dictionary<QuestData, float> finishedTime = new();

    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.L))
        {
            if (PauseMenuManager.Instance.IsPaused) return;
            OpenPanel();
        }

        if (Input.GetKeyDown(KeyCode.Escape))
            ClosePanel();

        foreach (var kvp in slots)
        {
            QuestData q = kvp.Key;
            QuestItem item = kvp.Value;

            if (item.isCompleted || item.isClaimed || item.isFailed) continue;
            if (!q.hasTimer) continue;
            if (!startTime.ContainsKey(q)) continue;

            if (!remainingTime.ContainsKey(q)) continue;

            remainingTime[q] -= Time.deltaTime;

            if (remainingTime[q] <= 0f)
            {
                remainingTime[q] = 0f;

                if (QuestManager.Instance.HasQuest(q))
                {
                    item.MarkFailed();
                    QuestManager.Instance.FailQuest(q);
                }
                continue;
            }

            int m = Mathf.FloorToInt(remainingTime[q] / 60);
            int s = Mathf.FloorToInt(remainingTime[q] % 60);
            item.durationText.text = $"{m:00}:{s:00}";
        }
    }

    QuestUIState GetState(QuestData q)
    {
        QuestItem item = slots[q];

        if (item.isFailed || item.isClaimed)
            return QuestUIState.Finished;

        if (QuestManager.Instance.IsCompleted(q))
            return QuestUIState.CompletedUnclaimed;

        return QuestUIState.Active;
    }

    void ReorderAll()
    {
        List<QuestData> completedUnclaimed = new();
        List<QuestData> activeTimed = new();
        List<QuestData> activeNormal = new();
        List<QuestData> finished = new();

        // === GROUPING ===
        foreach (var q in slots.Keys)
        {
            switch (GetState(q))
            {
                case QuestUIState.CompletedUnclaimed:
                    completedUnclaimed.Add(q);
                    break;

                case QuestUIState.Active:
                    if (q.hasTimer && !slots[q].isFailed)
                        activeTimed.Add(q);
                    else
                        activeNormal.Add(q);
                    break;

                case QuestUIState.Finished:
                    finished.Add(q);
                    break;
            }
        }

        // === SORTING ===

        // Timed quest → sisa waktu paling sedikit di atas
        activeTimed.Sort((a, b) =>
            GetRemainingTime(a).CompareTo(GetRemainingTime(b))
        );

        // Active normal → yang paling baru di atas
        activeNormal.Sort((a, b) =>
        {
            float ta = startTime.ContainsKey(a) ? startTime[a] : 0f;
            float tb = startTime.ContainsKey(b) ? startTime[b] : 0f;
            return tb.CompareTo(ta);
        });

        // Finished → yang paling lama di bawah
        finished.Sort((a, b) =>
        {
            float ta = finishedTime.ContainsKey(a) ? finishedTime[a] : 0f;
            float tb = finishedTime.ContainsKey(b) ? finishedTime[b] : 0f;
            return tb.CompareTo(ta);
        });

        // === APPLY ORDER ===
        int index = 0;

        foreach (var q in completedUnclaimed)
            slots[q].transform.SetSiblingIndex(index++);

        foreach (var q in activeTimed)
            slots[q].transform.SetSiblingIndex(index++);

        foreach (var q in activeNormal)
            slots[q].transform.SetSiblingIndex(index++);

        foreach (var q in finished)
            slots[q].transform.SetSiblingIndex(index++);
    }

    public float GetRemainingTime(QuestData q)
    {
        return remainingTime.ContainsKey(q) ? remainingTime[q] : -1f;
    }

    public void RestoreRemainingTime(QuestData q, float time)
    {
        if (time > 0)
            remainingTime[q] = time;
    }

    public void OpenPanel()
    {
        GlobalUIManager.Instance.OpenPanel(questPanel);
        TutorialEvents.OnOpenQuest?.Invoke();
    }

    public void ClosePanel()
    {
        GlobalUIManager.Instance.ClosePanel(questPanel);
    }

    void Start()
    {
        questPanel.SetActive(false);
        panelPrefab.SetActive(false);
    }

    public float GetStartTime(QuestData q)
    {
        return startTime.ContainsKey(q) ? startTime[q] : -1f;
    }

    public void RestoreStartTime(QuestData q, float savedTime)
    {
        if (savedTime > 0)
            startTime[q] = savedTime;
    }

    public void ShowQuest(QuestData q, bool isRestore)
    {
        if (slots.ContainsKey(q)) return;

        GameObject obj = Instantiate(panelPrefab, panelParent);
        obj.SetActive(true);

        QuestItem item = obj.GetComponent<QuestItem>();
        item.Setup(q, isRestore);

        slots.Add(q, item);

        if (!remainingTime.ContainsKey(q))
            remainingTime[q] = q.duration; // 🔥 START FULL TIME

        if (!startTime.ContainsKey(q) && !isRestore)
            startTime[q] = Time.time;

        if (!isRestore)
            ReorderAll(); 
    }

    public void MarkCompleted(QuestData q)
    {
        if (!slots.ContainsKey(q)) return;

        slots[q].MarkCompleted();

        if (!finishedTime.ContainsKey(q))
            finishedTime[q] = Time.time;

        ReorderAll();
    }

    public void MarkFailed(QuestData q)
    {
        if (!slots.ContainsKey(q)) return;

        remainingTime.Remove(q);
        slots[q].MarkFailed();

        if (!finishedTime.ContainsKey(q))
            finishedTime[q] = Time.time;

        ReorderAll();
    }

    public void MarkClaimed(QuestData q)
    {
        if (!slots.ContainsKey(q)) return;

        slots[q].MarkClaimed();
        ReorderAll();

        if (TutorialManager.Instance.currentStep == TutorialStep.ClaimQuest)
        {
            TutorialEvents.OnClaimQuest?.Invoke(q.questID);
        }
    }

    public void UpdateProgress(QuestData q, int now, int max)
    {
        if (!slots.ContainsKey(q)) return;
        slots[q].UpdateProgress(now, max);
    }

    public bool IsClaimed(QuestData q)
    {
        if (!slots.ContainsKey(q)) return false;
        return slots[q].isClaimed;
    }

    public float GetFinishedTime(QuestData q)
    {
        return finishedTime.ContainsKey(q) ? finishedTime[q] : -1f;
    }

    public void RestoreFinishedTime(QuestData q, float time)
    {
        if (time > 0)
            finishedTime[q] = time;
    }

    public void ForceReorder()
    {
        ReorderAll();
    }

    public void ResetAll()
    {
        // destroy semua UI quest
        foreach (var item in slots.Values)
        {
            if (item != null)
                Destroy(item.gameObject);
        }

        slots.Clear();
        startTime.Clear();
        remainingTime.Clear();
        finishedTime.Clear();

        questPanel.SetActive(false);
    }
}