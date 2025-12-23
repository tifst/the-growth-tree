using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class SellShopUI : MonoBehaviour
{
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
    }

    void OnEnable()
    {
        RefreshUI();
    }

    private void RefreshUI()
    {
        if (GameManager.Instance == null) return;

        coinUI.text = $"Coins: {GameManager.Instance.coins}";

        foreach (Transform child in panelParent)
            child.gameObject.SetActive(false);

        foreach (var tree in GameManager.Instance.allTrees)
        {
            GameObject panel = Instantiate(panelPrefab, panelParent);
            panel.SetActive(true);
            
            TMP_Text titleTxt = panel.transform.Find("TitleText")?.GetComponent<TMP_Text>();
            TMP_Text stockTxt = panel.transform.Find("StockText")?.GetComponent<TMP_Text>();
            TMP_Text priceTxt = panel.transform.Find("PriceText")?.GetComponent<TMP_Text>();
            Image iconImg = panel.transform.Find("Icon")?.GetComponent<Image>();

            Button sellOneBtn = panel.transform.Find("SellOneButton")?.GetComponent<Button>();
            Button sellAllBtn = panel.transform.Find("SellAllButton")?.GetComponent<Button>();
            TMP_Text sellAllTxt = sellAllBtn?.GetComponentInChildren<TMP_Text>();

            string fruitName = tree.treeName;
            int stock = GameManager.Instance.GetFruitStock(fruitName);
            int sellPrice = tree.fruitSellPrice;

            if (titleTxt) titleTxt.text = $"Sell {fruitName}";
            if (stockTxt) stockTxt.text = $"Stock: {stock}";
            if (priceTxt) priceTxt.text = $"Price: {sellPrice} Coins";
            if (iconImg) iconImg.sprite = tree.fruitIcon;

            bool bisaJual = stock > 0;

            if (sellOneBtn)
            {
                sellOneBtn.interactable = bisaJual;
                sellOneBtn.onClick.RemoveAllListeners();
                sellOneBtn.onClick.AddListener(() =>
                {
                    SellItem(fruitName, 1, sellPrice);

                    // Progress quest 1x
                    QuestManager.Instance.AddProgress(fruitName, QuestGoalType.SellFruit);
                });
            }

            if (sellAllBtn)
            {
                sellAllBtn.interactable = bisaJual;
                sellAllBtn.onClick.RemoveAllListeners();
                sellAllBtn.onClick.AddListener(() =>
                {
                    SellItem(fruitName, stock, sellPrice);

                    // Progress quest sebanyak jumlah yang dijual
                    for (int i = 0; i < stock; i++)
                    {
                        QuestManager.Instance.AddProgress(fruitName, QuestGoalType.SellFruit);
                    }
                });

                if (sellAllTxt) sellAllTxt.text = $"Sell All ({stock})";
            }
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
        StartCoroutine(ShowSellMessage($"Terjual {jumlahJual} {fruitName} seharga {totalCoins} Koin!"));
        
        RefreshUI();
    }

    public void OnCloseButtonPress()
    {
        sellShop?.CloseShop();
    }

    private IEnumerator ShowSellMessage(string msg, float duration = 1f)
    {
        PromptUI.Instance.Show(msg, this);
        yield return new WaitForSeconds(duration);
        PromptUI.Instance.Hide(this);
    }
}