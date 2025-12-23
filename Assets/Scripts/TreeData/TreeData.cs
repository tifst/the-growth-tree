using UnityEngine;

[CreateAssetMenu(fileName = "NewTreeData", menuName = "Farming/Tree Data", order = 0)]
public class TreeData : ScriptableObject
{
    [Header("Identitas")]
    public string treeName = "Pohon Baru";

    [Header("Ekonomi")]
    public int seedBuyPrice = 10;
    public int fruitSellPrice = 5;
    public int requiredLevel = 1;

    [Header("Reward Saat Tumbuh Penuh")]
    public int xpRewardPlant = 10;
    public int xpRewardHarvest = 2;
    public float pollutionReduction = 3f;

    [Header("Prefab & Komponen")]
    public GameObject treePrefab;
    public GameObject fruitPrefab;
    public Sprite fruitIcon;
    public Sprite seedIcon;

    [Header("Visual Pohon")]
    public Material healthyLeafMaterial;
    public Material dryLeafMaterial;
    public GameObject deadTrunkPrefab;

    [Header("Pertumbuhan")]
    public Vector3 maxScale = new Vector3(1f, 1f, 1f);
    public float timeNeededToGrow = 60f; // waktu penuh (detik)

    [Header("Kesehatan & Air")]
    public float maxHealth = 100f;
    public float startHealth = 50f;
    public float grownDecayRate = 1f;
    public float healthyDecayRate = 2f;
    public float criticalDecayRate = 5f;
    public float waterNeedThreshold = 0.4f;

    [Header("Buah")]
    public int maxFruitCount = 3;
    public float fruitSpawnInterval = 5f;
}