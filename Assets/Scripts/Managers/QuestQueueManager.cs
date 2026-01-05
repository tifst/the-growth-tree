using UnityEngine;
using System.Collections.Generic;
using System;

public class QuestQueueManager : MonoBehaviour
{
    public Action<NPCQuestController> OnNPCSpawned;
    public Action OnNPCRemoved;

    public int CurrentIndex => currentIndex;
    public bool HasActiveNPC => activeNPC != null;
    public string ActiveQuestID => activeNPC != null ? activeNPC.CurrentQuestID : "";

    [Header("Difficulty")]
    public QuestDificulty difficulty;

    [Header("Quest Order")]
    public List<QuestData> questOrder;

    [Header("Spawn Points (index-based)")]
    public List<Transform> spawnPoints;

    [Header("Quest Location")]
    public Transform questPoint;

    [Header("Reuse NPC")]
    public bool reuseSameNPC = true;
    private bool restoredFromSave = false;

    private int currentIndex;
    private NPCQuestController activeNPC;

    // ================= SPAWN =================
    public void TrySpawnNext(int playerLevel)
    {
        if (restoredFromSave) return; // 🔥 PENTING
        if (!CanSpawnByLevel(playerLevel)) return;
        if (HasActiveNPC) return;
        if (currentIndex >= questOrder.Count) return;

        if (currentIndex >= spawnPoints.Count)
        {
            Debug.LogError("[QuestQueue] Spawn point count < quest count!");
            return;
        }

        QuestData q = questOrder[currentIndex];
        Transform spawn = spawnPoints[currentIndex];

        if (spawn == null)
        {
            Debug.LogError($"[QuestQueue] Spawn point null at index {currentIndex}");
            return;
        }

        GameObject npc = Instantiate(
            q.npcPrefab,
            spawn.position,
            spawn.rotation
        );

        activeNPC = npc.GetComponent<NPCQuestController>();
        activeNPC.Setup(q, questPoint, spawn, this, false);

        OnNPCSpawned?.Invoke(activeNPC);
        currentIndex++;
    }

    // ================= FLOW =================
    public void OnQuestFinished()
    {
        if (activeNPC == null) return;

        if (currentIndex >= questOrder.Count)
        {
            activeNPC.LeaveAndDestroy();
            return;
        }

        QuestData next = questOrder[currentIndex];

        if (CanReuseNPC(next))
        {
            // 🔁 NPC sama, lanjut quest
            activeNPC.AssignNextQuest(next);
            currentIndex++;
        }
        else
        {
            // 🚪 NPC lama pergi, NPC baru akan spawn
            activeNPC.LeaveAndDestroy();
        }
    }

    public void OnNPCGone()
    {
        activeNPC = null;
        OnNPCRemoved?.Invoke();
    }

    bool CanSpawnByLevel(int level)
    {
        return difficulty switch
        {
            QuestDificulty.Easy => level >= 1,
            QuestDificulty.Medium => level >= 3,
            QuestDificulty.Hard => level >= 5,
            _ => false
        };
    }

    // ================= SAVE LOAD =================
    public void RestoreFromSave(int index, string questID, bool hasNPC, int returnIndex)
    {
        restoredFromSave = true;

        if (activeNPC != null)
        {
            Destroy(activeNPC.gameObject);
            activeNPC = null;
        }

        currentIndex = index;

        if (!hasNPC || string.IsNullOrEmpty(questID))
        {
            restoredFromSave = false;
            return;
        }

        QuestData q = questOrder.Find(x => x.questID == questID);
        if (q == null) return;

        Transform returnPoint =
        (returnIndex >= 0 && returnIndex < spawnPoints.Count)
        ? spawnPoints[returnIndex]
        : questPoint; // fallback aman

        GameObject npc = Instantiate(
            q.npcPrefab,
            questPoint.position,
            questPoint.rotation
        );

        activeNPC = npc.GetComponent<NPCQuestController>();
        activeNPC.Setup(q, questPoint, returnPoint, this, true);

        restoredFromSave = false;
    }

    bool CanReuseNPC(QuestData next)
    {
        if (!reuseSameNPC) return false;
        if (activeNPC == null) return false;

        // 🔥 FLAG PALING PENTING
        if (next.forceNewNPC) return false;

        // prefab beda = NPC beda
        QuestData current = activeNPC.GetComponent<QuestTrigger>()?.questToGive;
        if (current == null) return false;

        return current.npcPrefab == next.npcPrefab;
    }

    public int GetActiveReturnPointIndex()
    {
        if (activeNPC == null) return -1;
        return spawnPoints.IndexOf(activeNPC.GetReturnPoint());
    }

    public void ResetQueue()
    {
        if (activeNPC != null)
            Destroy(activeNPC.gameObject);

        activeNPC = null;
        currentIndex = 0;
        restoredFromSave = false;
    }
}