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
            if (iconImg) iconImg.sprite = tree.fruitIcon;

            buyBtn.onClick.RemoveAllListeners();
            buyBtn.onClick.AddListener(() => PurchaseSeed(tree));

            bool enoughCoin = GameManager.Instance.coins >= tree.seedBuyPrice;
            bool enoughLevel = GameManager.Instance.level >= tree.requiredLevel;

            buyBtn.interactable = enoughCoin && enoughLevel;

            if (!enoughLevel)
                descTxt.text = $"Terbuka Pada Level {tree.requiredLevel}";
        }
    }

    private void PurchaseSeed(TreeData tree)
    {
        if (GameManager.Instance.coins < tree.seedBuyPrice) return;
        if (GameManager.Instance.level < tree.requiredLevel) return;

        GameManager.Instance.AddCoins(-tree.seedBuyPrice);
        GameManager.Instance.ModifySeedStock(tree.treeName, 1);

        coinUI.text = $"Coins: {GameManager.Instance.coins}";
        StartCoroutine(ShowBuyMessage($"1 Bibit {tree.treeName} dibeli!"));

        UpdateShopPanels(); // update only
    }

    public void OnCloseButtonPress()
    {
        buyShop?.CloseShop();
    }
    
    private IEnumerator ShowBuyMessage(string msg, float duration = 1f)
    {
        PromptUI.Instance.Show(msg, this);
        yield return new WaitForSeconds(duration);
        PromptUI.Instance.Hide(this);
    }
}