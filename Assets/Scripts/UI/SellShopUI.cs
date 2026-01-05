using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class SellShopUI : MonoBehaviour
{
    private readonly List<GameObject> slotPool = new List<GameObject>();

    [Header("UI References")]
    public TMP_Text coinUI;
    public GameObject SellPanel;
    public GameObject panelPrefab;
    public Transform panelParent;
    public QuestManager questManager;
    private SellShop sellShop;

    void Start()
    {
        SellPanel.SetActive(false);
        sellShop = FindFirstObjectByType<SellShop>();

        panelPrefab.SetActive(false);

        PreGenerateSlots();
    }

    void PreGenerateSlots()
    {
        int count = GameManager.Instance.allTrees.Count;

        for (int i = 0; i < count; i++)
        {
            GameObject panel = Instantiate(panelPrefab, panelParent);
            panel.SetActive(true);
            slotPool.Add(panel);
        }
    }

    void OnEnable()
    {
        RefreshUI();
    }

    void SetPanelAlpha(GameObject panel, float alpha)
    {
        Image bg = panel.GetComponent<Image>();
        if (!bg) return;

        Color c = bg.color;
        c.a = alpha;
        bg.color = c;
    }

    private void RefreshUI()
    {
        coinUI.text = $"Coins: {GameManager.Instance.coins}";

        for (int i = 0; i < slotPool.Count; i++)
        {
            GameObject panel = slotPool[i];
            TreeData tree = GameManager.Instance.allTrees[i];

            TMP_Text titleTxt = panel.transform.Find("TitleText")?.GetComponent<TMP_Text>();
            TMP_Text stockTxt = panel.transform.Find("StockText")?.GetComponent<TMP_Text>();
            TMP_Text priceTxt = panel.transform.Find("PriceText")?.GetComponent<TMP_Text>();
            Image iconImg = panel.transform.Find("Icon")?.GetComponent<Image>();

            Button sellOneBtn = panel.transform.Find("SellOneButton")?.GetComponent<Button>();
            Button sellAllBtn = panel.transform.Find("SellAllButton")?.GetComponent<Button>();
            TMP_Text sellAllTxt = sellAllBtn?.GetComponentInChildren<TMP_Text>();

            int stock = GameManager.Instance.GetFruitStock(tree.treeName);

            titleTxt.text = $"Sell {tree.treeName}";
            stockTxt.text = $"Stock: {stock}";
            priceTxt.text = $"Price: {tree.fruitSellPrice} Coins";
            iconImg.sprite = tree.fruitIcon;

            bool canSell = stock > 0;

            sellOneBtn.interactable = canSell;
            sellAllBtn.interactable = canSell;

            sellOneBtn.onClick.RemoveAllListeners();
            sellAllBtn.onClick.RemoveAllListeners();

            sellOneBtn.onClick.AddListener(() =>
            {
                SellItem(tree.treeName, 1, tree.fruitSellPrice);
                TutorialEvents.OnSellFruit?.Invoke();
                QuestManager.Instance.AddProgress(tree.treeName, QuestGoalType.SellFruit);
            });

            sellAllBtn.onClick.AddListener(() =>
            {
                SellItem(tree.treeName, stock, tree.fruitSellPrice);
                TutorialEvents.OnSellFruit?.Invoke();

                for (int j = 0; j < stock; j++)
                    QuestManager.Instance.AddProgress(tree.treeName, QuestGoalType.SellFruit);
            });

            sellAllTxt.text = $"Sell All ({stock})";

            // visual kalau kosong
            SetPanelAlpha(panel, canSell ? 1f : 0.5f);
        }
    }

    private void SellItem(string fruitName, int jumlah, int harga)
    {
        int stock = GameManager.Instance.GetFruitStock(fruitName);
        if (stock <= 0) return;

        int jumlahJual = Mathf.Min(stock, jumlah);
        int totalCoins = jumlahJual * harga;

        GameManager.Instance.ModifyFruitStock(fruitName, -jumlahJual);
        GameManager.Instance.AddCoins(totalCoins);
        PromptManager.Instance.Notify($"{jumlahJual} {fruitName} sold! You get {totalCoins} coins!");
        
        RefreshUI();
    }

    public void OnCloseButtonPress()
    {
        sellShop?.CloseShop();
    }
}