using UnityEngine;
using System.Collections.Generic;

public class PromptManager : MonoBehaviour
{
    public static PromptManager Instance;

    [Header("Prefab")]
    public PromptInstance promptPrefab;

    [Header("Parents")]
    public RectTransform notificationParent;
    public RectTransform contextParent;

    [Header("Layout")]
    public float verticalSpacing = 40f;

    [Header("Default Positions")]
    public Vector2 notificationBasePos = new Vector2(0, -200);
    public Vector2 contextBasePos = new Vector2(0, 0);

    private readonly List<PromptInstance> notifications = new();
    private readonly List<PromptInstance> contexts = new();

    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    // NOTIFICATION (AUTO HIDE)
    public void Notify(string msg, float duration = 2f)
    {
        Debug.Log("Notify called: " + msg);
        SpawnPrompt(
            msg,
            null,
            PromptType.Notification,
            duration,
            notificationParent,
            notifications,
            notificationBasePos
        );
    }

    public void Notify(string msg, Vector2 customBasePos, float duration = 2f)
    {
        Debug.Log("Notify called: " + msg);
        SpawnPrompt(
            msg,
            null,
            PromptType.Notification,
            duration,
            notificationParent,
            notifications,
            customBasePos
        );
    }

    // CONTEXT (MANUAL HIDE)
    public void ShowContext(string msg, MonoBehaviour owner)
    {
        foreach (var p in contexts)
        {
            if (p != null && p.owner == owner)
                return; // 🔥 SUDAH ADA, JANGAN SPAWN LAGI
        }

        ShowContext(msg, owner, contextBasePos);
    }

    public void ShowContext(string msg, MonoBehaviour owner, Vector2 customBasePos)
    {
        SpawnPrompt(
            msg,
            owner,
            PromptType.Context,
            -1f,
            contextParent,
            contexts,
            customBasePos
        );
    }

    public void HideContext(MonoBehaviour owner)
    {
        for (int i = contexts.Count - 1; i >= 0; i--)
        {
            if (contexts[i] == null) continue;

            if (contexts[i].owner == owner)
                contexts[i].Hide();
        }
    }

    public void RefreshContext(MonoBehaviour owner, string newText)
    {
        foreach (var p in contexts)
        {
            if (p == null) continue;
            if (p.owner != owner) continue;

            p.SetText(newText);
            return;
        }

        // belum ada → spawn baru
        ShowContext(newText, owner);
    }

    // CORE SPAWN
    void SpawnPrompt(
        string msg,
        MonoBehaviour owner,
        PromptType type,
        float duration,
        RectTransform parent,
        List<PromptInstance> list,
        Vector2 basePos
    )
    {
        PromptInstance p = Instantiate(promptPrefab, parent);
        p.Setup(msg, owner, type, duration);

        p.onDestroyed = inst =>
        {
            list.Remove(inst);
            Reposition(list, basePos);
        };

        list.Add(p);
        Reposition(list, basePos);
    }

    // POSITIONING
    void Reposition(List<PromptInstance> list, Vector2 basePos)
    {
        for (int i = 0; i < list.Count; i++)
        {
            if (list[i] == null) continue;

            list[i].rect.anchoredPosition =
                basePos + Vector2.up * (i * verticalSpacing);
        }
    }

    // CLEANUP
    public void ClearAll()
    {
        foreach (var p in notifications)
            if (p) p.Hide();

        foreach (var p in contexts)
            if (p) p.Hide();

        notifications.Clear();
        contexts.Clear();
    }

    public void ClearAllContext()
    {
        for (int i = contexts.Count - 1; i >= 0; i--)
        {
            if (contexts[i] != null)
                contexts[i].Hide();
        }
        contexts.Clear();
    }

    public void ResetAll()
    {
        ClearAll();
    }
}