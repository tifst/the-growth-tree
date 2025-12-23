using UnityEngine;
using System.Collections.Generic;

public class NPCSpawner : MonoBehaviour
{
    [Header("NPC Prefabs (Random)")]
    public GameObject[] npcPrefabs;    // ← simpan banyak jenis NPC di sini

    [Header("Spawn Settings")]
    public int maxNPC = 15;
    public float spawnInterval = 3f;

    [Header("Spawn Points")]
    public Transform[] spawnPoints;

    private List<GameObject> spawnedNPCs = new List<GameObject>();
    private float timer;

    void Update()
    {
        timer += Time.deltaTime;

        if (timer >= spawnInterval && spawnedNPCs.Count < maxNPC)
        {
            SpawnNPC();
            timer = 0f;
        }

        // Bersihkan item null
        spawnedNPCs.RemoveAll(npc => npc == null);
    }

    void SpawnNPC()
    {
        if (spawnPoints.Length == 0 || npcPrefabs.Length == 0)
            return;

        // Pilih karakter random
        int randomNPC = Random.Range(0, npcPrefabs.Length);

        // Pilih spawn point random
        int randomSpawn = Random.Range(0, spawnPoints.Length);

        GameObject npc = Instantiate(
            npcPrefabs[randomNPC],
            spawnPoints[randomSpawn].position,
            spawnPoints[randomSpawn].rotation
        );

        spawnedNPCs.Add(npc);
    }
}
