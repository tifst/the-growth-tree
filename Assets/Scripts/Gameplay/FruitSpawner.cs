using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class FruitSpawner : MonoBehaviour
{
    [Header("Assign ONLY the parent of fruit spawn points")]
    [SerializeField] private Transform spawnPointParent;

    private TreeData treeData;
    private GrowTree growTree;

    private List<Transform> spawnPoints = new List<Transform>();
    private Dictionary<Transform, GameObject> fruitAtPoint = new Dictionary<Transform, GameObject>();

    private bool isSpawning = false;
    private float spawnTimer = 0f;

    void Awake()
    {
        growTree = GetComponent<GrowTree>();
        treeData = growTree?.treeData;

        spawnPoints.Clear();

        if (spawnPointParent != null)
        {
            foreach (Transform t in spawnPointParent)
                spawnPoints.Add(t);
        }
        else
        {
            Debug.LogError($"❌ {name} tidak punya SpawnPointParent! Assign di inspector.");
        }
    }

    void Update()
    {
        if (growTree == null || treeData == null) return;
        if (!growTree.IsFullyGrown) return;
        if (growTree.CurrentHealthPercent <= 0f) return;
        if (growTree.CurrentHealthPercent < growTree.treeData.waterNeedThreshold) return;

        float spawnSpeedMultiplier = 1f;

        if (growTree.CurrentHealthPercent < growTree.treeData.waterNeedThreshold)
        {
            spawnSpeedMultiplier = 0.5f; // 2x lebih lama
        }

        spawnTimer += Time.deltaTime * spawnSpeedMultiplier;

        if (spawnTimer >= treeData.fruitSpawnInterval)
        {
            spawnTimer = 0f;
            TrySpawnFruit();
        }

        if (!isSpawning)
            StartCoroutine(CheckDroppedFruits());
    }

    private void TrySpawnFruit()
    {
        int activeFruits = 0;
        foreach (var kvp in fruitAtPoint)
        {
            if (kvp.Value != null && kvp.Value.activeInHierarchy)
                activeFruits++;
        }

        if (activeFruits >= treeData.maxFruitCount)
            return;

        List<Transform> available = new List<Transform>();
        foreach (Transform p in spawnPoints)
        {
            if (p == null) continue;

            if (!fruitAtPoint.ContainsKey(p) ||
                fruitAtPoint[p] == null ||
                !fruitAtPoint[p].activeInHierarchy)
            {
                available.Add(p);
            }
        }

        if (available.Count == 0) return;

        Transform point = available[Random.Range(0, available.Count)];

        GameObject fruit = FruitPoolManager.Instance.GetFruit(
            treeData.treeName,
            point.position,
            Quaternion.identity,
            transform
        );

        if (fruit != null)
        {
            fruitAtPoint[point] = fruit;
            Debug.Log($"🍎 Spawn buah pada: {point.name}");
        }
    }

    IEnumerator CheckDroppedFruits()
    {
        isSpawning = true;

        while (true)
        {
            yield return new WaitForSeconds(2f);

            if (growTree == null || treeData == null)
                yield break;

            List<Transform> toClear = new List<Transform>();

            foreach (var kvp in fruitAtPoint)
            {
                if (kvp.Key == null) continue;

                GameObject fruit = kvp.Value;

                if (fruit == null || !fruit.activeInHierarchy)
                {
                    toClear.Add(kvp.Key);
                    continue;
                }

                float dist = Vector3.Distance(kvp.Key.position, fruit.transform.position);
                if (dist > 2f)
                    toClear.Add(kvp.Key);
            }

            foreach (Transform p in toClear)
                fruitAtPoint[p] = null;
        }
    }

    //  REQUIRED BY TreePhysic → jangan hapus!
    public List<GameObject> GetActiveFruits()
    {
        List<GameObject> list = new List<GameObject>();

        foreach (var kvp in fruitAtPoint)
        {
            if (kvp.Value != null && kvp.Value.activeInHierarchy)
                list.Add(kvp.Value);
        }

        return list;
    }

    void OnDrawGizmos()
    {
        if (spawnPointParent == null) return;

        Gizmos.color = Color.yellow;

        foreach (Transform t in spawnPointParent)
            Gizmos.DrawSphere(t.position, 0.05f);
    }
}