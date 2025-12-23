using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections.Generic;

public class QuestUIManager : MonoBehaviour
{
    [Header("UI References")]
    public GameObject questPanel;
    public GameObject panelPrefab;
    public Transform panelParent;

    private Dictionary<QuestData, QuestItem> slots = new();
    private Dictionary<QuestData, float> startTime = new();

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.L))
            OpenPanel();

        if (Input.GetKeyDown(KeyCode.Escape))
            ClosePanel();

        foreach (var kvp in slots)
        {
            QuestData q = kvp.Key;
            QuestItem item = kvp.Value;

            if (QuestManager.Instance.IsCompleted(q) || item.isClaimed) continue;
            if (!q.hasTimer) continue;

            float elapsed = Time.time - startTime[q];
            float remain = q.duration - elapsed;

            if (remain <= 0)
            {
                item.MarkFailed();
                QuestManager.Instance.FailQuest(q);
                continue;
            }

            int m = Mathf.FloorToInt(remain / 60);
            int s = Mathf.FloorToInt(remain % 60);

            item.durationText.text = $"{m:00}:{s:00}";
        }
    }

    public void OpenPanel()
    {
        GlobalUIManager.Instance.OpenPanel(questPanel);
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

    public void ShowQuest(QuestData q)
    {
        if (slots.ContainsKey(q))
            return;

        GameObject obj = Instantiate(panelPrefab, panelParent);
        obj.SetActive(true);

        QuestItem item = obj.GetComponent<QuestItem>();
        item.Setup(q);

        slots.Add(q, item);
        startTime[q] = Time.time;

        item.transform.SetSiblingIndex(0);
    }

    private int GetCompletedCount()
    {
        int count = 0;
        foreach (var kvp in slots)
        {
            QuestData q = kvp.Key;
            // completed tapi bukan failed
            if (QuestManager.Instance.IsCompleted(q))
                count++;
        }
        return count;
    }
    private int GetActiveCount()
    {
        int count = 0;
        foreach (var kvp in slots)
        {
            QuestData q = kvp.Key;
            if (!QuestManager.Instance.IsCompleted(q))
                count++;
        }
        return count;
    }

    public void MarkClaimed(QuestData q)
    {
        if (!slots.ContainsKey(q)) return;
        slots[q].MarkClaimed();
    }

    public void MarkCompleted(QuestData q)
    {
        if (!slots.ContainsKey(q)) return;

        float duration = Time.time - startTime[q];
        QuestItem item = slots[q];

        item.MarkCompleted(duration);
        int activeCount = GetActiveCount();

        // Completed quest pertama masuk di index = activeCount
        item.transform.SetSiblingIndex(activeCount);
    }

    public void UpdateProgress(QuestData q, int now, int max)
    {
        if (!slots.ContainsKey(q)) return;
        slots[q].UpdateProgress(now, max);
    }

    public void MoveQuestActiveBelowNew(QuestData q)
    {
        if (!slots.ContainsKey(q)) return;
        slots[q].transform.SetSiblingIndex(1);  
    }

    public void MarkFailed(QuestData q)
    {
        if (!slots.ContainsKey(q)) return;

        QuestItem item = slots[q];
        item.MarkFailed();

        int activeCount = GetActiveCount();
        int completedCount = GetCompletedCount();

        // Failed quest masuk setelah active + completed
        int failedIndex = activeCount + completedCount;

        item.transform.SetSiblingIndex(failedIndex);
    }

    public void MoveQuestToTop(QuestData q)
    {
        if (!slots.ContainsKey(q)) return;
        slots[q].transform.SetSiblingIndex(0);
    }

    public void MoveQuestToBottom(QuestData q)
    {
        if (!slots.ContainsKey(q)) return;
        slots[q].transform.SetSiblingIndex(panelParent.childCount - 1);
    }
}