using UnityEngine;
using TMPro;
using System.Collections.Generic;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;

    [Header("UI Text")]
    public TMP_Text levelText;
    public TMP_Text coinsText;
    public TMP_Text pollutionText;
    public TMP_Text seedsText;
    public TMP_Text fruitsText;
    public TMP_Text xpText;
    public TMP_Text waterText;
    public TMP_Text bagCapacityText;

    [Header("Bars")]
    public GradientBarController xpBar;
    public GradientBarController pollutionBar;
    public GradientBarController waterBar;

    [Header("Inventory UI")]
    public Transform itemContainer;      // GridLayoutGroup parent
    public GameObject itemSlotPrefab;    // Slot prefab untuk benih & buah

    private List<ItemSlot> allSlots = new List<ItemSlot>();

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        // Pasang pointer balik
        GameManager.Instance.uiManager = this;

        // Generate semua slot inventori sekali saja
        GenerateAllSlots();

        // Update semua UI
        UpdateAllUI();
    }

    //  GENERATE SLOT INVENTORI (1x SAJA)
    void GenerateAllSlots()
    {
        allSlots.Clear();

        foreach (var tree in GameManager.Instance.allTrees)
        {
            if (tree == null) continue;

            // 1️⃣ Slot Benih
            CreateSlot(tree.treeName, true);

            // 2️⃣ Slot Buah
            CreateSlot(tree.treeName, false);
        }
    }

    void CreateSlot(string id, bool isSeed)
    {
        GameObject obj = Instantiate(itemSlotPrefab, itemContainer);
        ItemSlot slot = obj.GetComponent<ItemSlot>();

        slot.itemID = id;
        slot.isSeed = isSeed;

        allSlots.Add(slot);
    }

    //  UPDATE UI
    public void UpdateAllUI()
    {
        var gm = GameManager.Instance;

        UpdateLevel(gm.level);
        UpdateCoins(gm.coins);
        UpdatePollution(gm.pollution, gm.maxPollution);
        UpdateWater(gm.currentWater, gm.maxWater);
        UpdateXP(gm.xp, gm.prevXp, gm.nextXp);

        UpdateSeeds(gm.GetTotalSeeds());
        UpdateFruits(gm.GetTotalFruits());

        UpdateInventoryUI();
    }

    public void UpdateInventoryUI()
    {
        // Refresh semua slot
        foreach (var slot in allSlots)
            slot.UpdateSlotUI();

        UpdateBagCapacity();
    }

    public void UpdateBagCapacity()
    {
        int total = GameManager.Instance.GetTotalItems();
        int max = InventorySystem.Instance.maxBagCapacity;

        bagCapacityText.text = $"{total}/{max}";
    }

    //  INDIVIDUAL UPDATE FUNCTIONS
    public void UpdatePollution(float v, float max)
    {
        pollutionText.text = $"POLLUTION: {v:F0}%";
        pollutionBar?.UpdateBar(v / max, Color.red);
    }

    public void UpdateWater(float c, float max)
    {
        waterText.text = $"WATER: {c:F0}/{max:F0}";
        waterBar?.UpdateBar(c / max, Color.cyan);
    }

    public void UpdateXP(int total, int prev, int next)
    {
        float prog = Mathf.Clamp01((float)(total - prev) / (next - prev));
        xpText.text = $"{total}/{next}";
        xpBar?.UpdateBar(prog, Color.green);
    }

    public void UpdateLevel(int lvl)
    {
        levelText.text = $"LEVEL: {lvl}";
        PromptUI.Instance.ShowPickupMessage($"LEVEL UP! You are now Level {lvl}!", 3f);
    }
    public void UpdateCoins(int c) => coinsText.text = $"COINS: ${c}";
    public void UpdateSeeds(int c) => seedsText.text = $"SEEDS: {c}";
    public void UpdateFruits(int c) => fruitsText.text = $"FRUITS: {c}";
}
