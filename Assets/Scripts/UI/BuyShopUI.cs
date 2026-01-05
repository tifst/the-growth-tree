using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System.Collections;

public class BuyShopUI : MonoBehaviour
{
    [Header("UI References")]
    public TMP_Text coinUI;
    public GameObject BuyPanel;
    public GameObject panelPrefab;
    public Transform panelParent;

    private BuyShop buyShop;

    // cache slot panel yang sudah dibuat
    private readonly List<GameObject> slotPool = new List<GameObject>();

    void Start()
    {
        BuyPanel.SetActive(false);
        buyShop = FindFirstObjectByType<BuyShop>();

        panelPrefab.SetActive(false); // pastikan template hide

        // generate panel HANYA SEKALI
        PreGenerateSlots();
    }

    void OnEnable()
    {
        if (GameManager.Instance == null) return;

        coinUI.text = $"Coins: {GameManager.Instance.coins}";
        UpdateShopPanels(); // cuma update isi, tidak instantiate
    }

    // 🔥 BUAT SLOT HANYA SEKALI
    private void PreGenerateSlots()
    {
        int totalTreeTypes = GameManager.Instance.allTrees.Count;

        for (int i = 0; i < totalTreeTypes; i++)
        {
            GameObject panel = Instantiate(panelPrefab, panelParent);
            panel.SetActive(true);
            slotPool.Add(panel);
        }
    }

    // 🔥 UPDATE UI TANPA INSTANTIATE
    private void UpdateShopPanels()
    {
        for (int i = 0; i < slotPool.Count; i++)
        {
            GameObject panel = slotPool[i];
            TreeData tree = GameManager.Instance.allTrees[i];

            // ambil komponen dalam panel
            TMP_Text titleTxt = panel.transform.Find("TitleText")?.GetComponent<TMP_Text>();
            TMP_Text descTxt = panel.transform.Find("DescriptionText")?.GetComponent<TMP_Text>();
            TMP_Text costTxt = panel.transform.Find("CostText")?.GetComponent<TMP_Text>();
            Image iconImg = panel.transform.Find("Icon")?.GetComponent<Image>();
            Button buyBtn = panel.GetComponentInChildren<Button>();

            if (titleTxt) titleTxt.text = $"{tree.treeName} Seed";
            if (descTxt) descTxt.text = $"Grow your own {tree.treeName} tree!";
            if (costTxt) costTxt.text = $"Buy: {tree.seedBuyPrice} Coins";
            if (iconImg) iconImg.sprite = tree.seedIcon;

            buyBtn.onClick.RemoveAllListeners();
            buyBtn.onClick.AddListener(() => PurchaseSeed(tree));

            bool enoughCoin = GameManager.Instance.coins >= tree.seedBuyPrice;
            bool enoughLevel = GameManager.Instance.level >= tree.requiredLevel;

            if (!enoughLevel)
            {
                buyBtn.interactable = false;
                descTxt.text = $"Unlocked at level {tree.requiredLevel}";
                SetPanelLocked(panel, 0.8f);
            }
            else if (!enoughCoin)
            {
                buyBtn.interactable = false;
                descTxt.text = $"Coin is not enough";
                SetPanelLocked(panel, 0.8f);
            }
            else
            {
                buyBtn.interactable = true;
                SetPanelLocked(panel, 1f);
            }
        }
    }

    private void PurchaseSeed(TreeData tree)
    {
        TutorialEvents.OnBuySeed?.Invoke();

        if (GameManager.Instance.level < tree.requiredLevel)
        {
            PromptManager.Instance.Notify(
                $"Unlocked at level {tree.requiredLevel}"
            );
            return;
        }

        if (GameManager.Instance.coins < tree.seedBuyPrice)
        {
            PromptManager.Instance.Notify(
                $"Coin is not enought! You need {tree.seedBuyPrice} Coins"
            );
            return;
        }

        GameManager.Instance.AddCoins(-tree.seedBuyPrice);
        GameManager.Instance.ModifySeedStock(tree.treeName, 1);

        coinUI.text = $"Coins: {GameManager.Instance.coins}";
        PromptManager.Instance.Notify($"1 {tree.treeName} seed is purchased!");

        QuestManager.Instance.AddProgress(tree.treeName, QuestGoalType.BuySeed);

        UpdateShopPanels();
    }

    void SetPanelLocked(GameObject panel, float alpha)
    {
        Image bg = panel.GetComponent<Image>();
        if (bg != null)
        {
            Color c = bg.color;
            c.a = alpha;
            bg.color = c;
        }

        // semua text di dalam panel ikut redup
        TMP_Text[] texts = panel.GetComponentsInChildren<TMP_Text>(true);
        foreach (var t in texts)
        {
            Color tc = t.color;
            tc.a = alpha;
            t.color = tc;
        }

        // icon juga
        Image[] images = panel.GetComponentsInChildren<Image>(true);
        foreach (var img in images)
        {
            if (img == bg) continue;
            Color ic = img.color;
            ic.a = alpha;
            img.color = ic;
        }
    }

    public void OnCloseButtonPress()
    {
        buyShop?.CloseShop();
    }
}