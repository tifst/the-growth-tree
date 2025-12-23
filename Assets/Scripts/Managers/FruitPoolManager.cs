using UnityEngine;
using System.Collections.Generic;

public class FruitPoolManager : MonoBehaviour
{
    public static FruitPoolManager Instance;

    [Header("Daftar Semua Jenis Pohon")]
    [SerializeField] private List<TreeData> allTreeData;
    [SerializeField] private int poolSize = 10;

    private Dictionary<string, Queue<GameObject>> poolDict = new();

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        // Buat pool untuk setiap jenis pohon
        foreach (var tree in allTreeData)
        {
            if (tree == null || tree.fruitPrefab == null)
            {
                Debug.LogWarning($"⚠️ {tree?.treeName} tidak punya prefab buah, dilewati.");
                continue;
            }

            Queue<GameObject> queue = new();

            for (int i = 0; i < poolSize; i++)
            {
                GameObject fruit = Instantiate(tree.fruitPrefab);
                fruit.name = tree.treeName + "_Fruit";
                fruit.SetActive(false);

                // Pastikan ada TriggerPickup dan set nama buah
                TriggerPickup pickup = fruit.GetComponent<TriggerPickup>();
                if (pickup == null) pickup = fruit.AddComponent<TriggerPickup>();
                pickup.sourceTreeData = tree;

                queue.Enqueue(fruit);
            }

            poolDict[tree.treeName] = queue;
        }
    }

    public GameObject GetFruit(string treeName, Vector3 pos, Quaternion rot, Transform parent = null)
    {
        if (!poolDict.ContainsKey(treeName))
        {
            Debug.LogWarning($"❌ Pool untuk {treeName} tidak ditemukan!");
            return null;
        }

        Queue<GameObject> q = poolDict[treeName];
        GameObject fruit = q.Dequeue();
        fruit.transform.SetPositionAndRotation(pos, rot);
        fruit.transform.SetParent(parent);
        fruit.SetActive(true);
        q.Enqueue(fruit);
        return fruit;
    }

    public void ReturnFruit(GameObject fruit)
    {
        fruit.SetActive(false);
    }
}