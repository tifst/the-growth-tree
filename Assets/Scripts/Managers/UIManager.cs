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
    public TMP_Text xpText;
    public TMP_Text waterText;
    public TMP_Text bagCapacityText;
    public string BagCapacityString => bagCapacityText != null ? bagCapacityText.text : "";

    [Header("Bars")]
    public GradientBarController xpBar;
    public GradientBarController pollutionBar;
    public GradientBarController waterBar;
    public GradientBarController bagBar;   // 🆕 BAG BAR

    [Header("Inventory UI")]
    public Transform itemContainer;
    public GameObject itemSlotPrefab;

    private List<ItemSlot> allSlots = new List<ItemSlot>();

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        GameManager.Instance.uiManager = this;

        GenerateAllSlots();
        UpdateAllUI();
    }

    // ================= INVENTORY SLOT =================
    void GenerateAllSlots()
    {
        allSlots.Clear();

        foreach (var tree in GameManager.Instance.allTrees)
        {
            if (tree == null) continue;

            CreateSlot(tree.treeName, true);   // Seed
            CreateSlot(tree.treeName, false);  // Fruit
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

    // ================= UPDATE ALL =================
    public void UpdateAllUI()
    {
        var gm = GameManager.Instance;

        UpdateLevel(gm.level);
        UpdateCoins(gm.coins);
        UpdatePollution(gm.pollution, gm.maxPollution);
        UpdateWater(gm.currentWater, gm.maxWater);
        UpdateXP(gm.xp, gm.prevXp, gm.nextXp);

        UpdateInventoryUI();
    }

    public void UpdateInventoryUI()
    {
        foreach (var slot in allSlots)
            slot.UpdateSlotUI();

        UpdateBagCapacity();
    }

    // ================= BAG =================
    public void UpdateBagCapacity()
    {
        int total = GameManager.Instance.GetTotalItems();
        int max = InventorySystem.Instance.maxBagCapacity;

        bagCapacityText.text = $"{total}/{max}";

        float progress = Mathf.Clamp01((float)total / max);
        bagBar?.UpdateBar(progress);
    }

    // ================= INDIVIDUAL =================
    public void UpdatePollution(float v, float max)
    {
        pollutionText.text = $"{v:F0}%";
        pollutionBar?.UpdateBar(v / max);
    }

    public void UpdateWater(float c, float max)
    {
        waterText.text = $"{c:F0}/{max:F0}";
        waterBar?.UpdateBar(c / max);
    }

    public void UpdateXP(int total, int prev, int next)
    {
        int denom = Mathf.Max(1, next - prev);
        float prog = Mathf.Clamp01((float)(total - prev) / denom);

        xpText.text = $"{total}/{next}";
        xpBar?.UpdateBar(prog);
    }

    public void UpdateLevel(int lvl)
    {
        levelText.text = $"{lvl}";
    }

    public void UpdateCoins(int c)
    {
        coinsText.text = $"{c}";
    }
}