using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;

public class PlantingUI : MonoBehaviour
{
    [Header("Panel Utama")]
    public GameObject PlantingPanel;       // PlantingPanel
    public GameObject HUDPanel;    // HUDPanel

    [Header("Prefab Tombol Pohon")]
    public GameObject buttonPrefab;
    public Transform buttonParent;

    private PlantPlot currentPlot;
    private readonly List<Button> generatedButtons = new();

    void Start()
    {
        PlantingPanel.SetActive(false);
        HUDPanel.SetActive(true);
        GeneratePlantButtons();
    }

    void Update()
    {
        if (PlantingPanel.activeSelf && Input.GetKeyDown(KeyCode.Escape))
            CloseMenu();
    }

    private void GeneratePlantButtons()
    {
        if (GameManager.Instance == null || GameManager.Instance.allTrees == null)
        {
            Debug.LogWarning("⚠️ GameManager belum siap atau allTrees kosong!");
            return;
        }

        foreach (Transform child in buttonParent)
            child.gameObject.SetActive(false);

        generatedButtons.Clear();

        foreach (TreeData tree in GameManager.Instance.allTrees)
        {
            GameObject newBtnObj = Instantiate(buttonPrefab, buttonParent);
            newBtnObj.SetActive(true);
            Button btn = newBtnObj.GetComponent<Button>();
            generatedButtons.Add(btn);

            TMP_Text nameTxt = newBtnObj.transform.Find("NameText")?.GetComponent<TMP_Text>();
            Image iconImg = newBtnObj.transform.Find("Icon")?.GetComponent<Image>();
            TMP_Text stockTxt = newBtnObj.transform.Find("StockText")?.GetComponent<TMP_Text>();

            if (nameTxt) nameTxt.text = tree.treeName;
            if (iconImg) iconImg.sprite = tree.seedIcon;
            if (stockTxt) stockTxt.text = $"Seeds: {GameManager.Instance.GetSeedStock(tree.treeName)}";

            btn.onClick.AddListener(() => OnClickPlant(tree));
        }
    }

    public void OpenPlantingMenu(PlantPlot plot)
    {
        currentPlot = plot;

        HUDPanel.SetActive(false);   // hide HUD only
        GlobalUIManager.Instance.OpenPanel(PlantingPanel);

        UpdateButtons();
    }

    private void UpdateButtons()
    {
        for (int i = 0; i < generatedButtons.Count; i++)
        {
            TreeData tree = GameManager.Instance.allTrees[i];
            int stock = GameManager.Instance.GetSeedStock(tree.treeName);

            TMP_Text stockTxt = generatedButtons[i].transform.Find("StockText")?.GetComponent<TMP_Text>();
            if (stockTxt) stockTxt.text = $"Seeds: {stock}";

            generatedButtons[i].interactable = stock > 0;
        }
    }

    private void OnClickPlant(TreeData treeData)
    {
        if (currentPlot == null || treeData == null) return;
        currentPlot.SelectTreeType(treeData);
        string plantedName = treeData.treeName;
        PromptManager.Instance.Notify($"{plantedName} Seed planted!");
        currentPlot.PlantTree();
        
        CloseMenu();
    }

    public void CloseMenu()
    {
        HUDPanel.SetActive(true);
        GlobalUIManager.Instance.ClosePanel(PlantingPanel);
        
        currentPlot = null;
    }
}
