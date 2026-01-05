using UnityEngine;
using System.Collections.Generic;

public class NPCSpawner : MonoBehaviour
{
    [Header("NPC Prefabs")]
    public GameObject[] npcPrefabs;

    [Header("Spawn Points")]
    public Transform[] spawnPoints;

    [Header("Population Settings")]
    public int maxNpcClean = 20;
    public int maxNpcDirty = 4;
    public float spawnInterval = 2.5f;

    private List<GameObject> activeNPCs = new();
    private Queue<GameObject> npcPool = new();

    private Transform player;
    private float timer;

    void Start()
    {
        // Cari player sekali saja
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
            player = playerObj.transform;
        else
            Debug.LogWarning("[NPCSpawner] Player tidak ditemukan (tag: Player)");
    }

    void Update()
    {
        if (Time.timeScale == 0f) return;

        timer += Time.deltaTime;

        int targetPopulation = GetTargetPopulation();

        // ================= SPAWN =================
        if (timer >= spawnInterval && activeNPCs.Count < targetPopulation)
        {
            SpawnNPC();
            timer = 0f;
        }

        // ================= DESPAWN (JIKA KELEBIHAN) =================
        while (activeNPCs.Count > targetPopulation)
        {
            GameObject npc = GetBestNPCToDespawn();
            if (npc == null) break;

            DespawnNPC(npc);
        }

        // Bersihkan referensi null (jaga-jaga)
        activeNPCs.RemoveAll(npc => npc == null);
    }

    // ================= POPULATION =================
    int GetTargetPopulation()
    {
        if (GameManager.Instance == null) return maxNpcClean;

        float pollutionPercent =
            GameManager.Instance.pollution / GameManager.Instance.maxPollution;

        return Mathf.RoundToInt(
            Mathf.Lerp(maxNpcClean, maxNpcDirty, pollutionPercent)
        );
    }

    // ================= SPAWN =================
    void SpawnNPC()
    {
        if (spawnPoints.Length == 0 || npcPrefabs.Length == 0)
            return;

        GameObject npc;

        // Ambil dari pool jika ada
        if (npcPool.Count > 0)
        {
            npc = npcPool.Dequeue();
            npc.SetActive(true);
        }
        else
        {
            npc = Instantiate(
                npcPrefabs[Random.Range(0, npcPrefabs.Length)]
            );
        }

        // Posisi spawn
        Transform sp = spawnPoints[Random.Range(0, spawnPoints.Length)];
        npc.transform.SetPositionAndRotation(sp.position, sp.rotation);

        activeNPCs.Add(npc);
    }

    // ================= DESPAWN =================
    void DespawnNPC(GameObject npc)
    {
        if (npc == null) return;

        activeNPCs.Remove(npc);

        npc.SetActive(false);
        npcPool.Enqueue(npc);
    }

    // ================= PRIORITY DESPAWN =================
    GameObject GetBestNPCToDespawn()
    {
        GameObject best = null;
        float bestScore = float.MinValue;

        foreach (var npc in activeNPCs)
        {
            if (npc == null) continue;

            float score = 0f;

            // 1️⃣ Jarak dari player
            if (player != null)
            {
                float dist = Vector3.Distance(
                    npc.transform.position,
                    player.position
                );
                score += dist;
            }

            // 2️⃣ Visibility (PALING PENTING)
            Renderer r = npc.GetComponentInChildren<Renderer>();
            if (r != null && !r.isVisible)
                score += 1000f;

            // 3️⃣ NPC yang sedang sleep diprioritaskan
            NPCSleepController sleep = npc.GetComponent<NPCSleepController>();
            if (sleep != null && sleep.IsSleeping)
                score += 500f;

            if (score > bestScore)
            {
                bestScore = score;
                best = npc;
            }
        }

        return best;
    }
}