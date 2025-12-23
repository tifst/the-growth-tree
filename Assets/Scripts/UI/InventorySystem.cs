using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class InventorySystem : MonoBehaviour
{
    public static InventorySystem Instance;

    [Header("UI Panels")]
    public GameObject inventoryPanel;
    public GameObject HUDCanvas;
    public GameObject dropPanel;

    [Header("Drop UI")]
    public TMP_Text selectedItemText;
    public Button dropOneButton;
    public Button dropAllButton;

    [Header("Bag Settings")]
    public int maxBagCapacity = 20;

    private bool isOpen = false;
    private ItemSlot selectedSlot = null;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    void Start()
    {
        inventoryPanel.SetActive(false);
        dropPanel.SetActive(false);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.I))
            OpenInventory();

        if (isOpen && Input.GetKeyDown(KeyCode.Escape))
            CloseInventory();
    }

    //  OPEN / CLOSE INVENTORY
    public void OpenInventory()
    {
        if (isOpen) return;
        isOpen = true;

        GlobalUIManager.Instance.OpenPanel(inventoryPanel);
        HUDCanvas.SetActive(false);

        UIManager.Instance.UpdateInventoryUI();
        UIManager.Instance.UpdateBagCapacity();

        DeselectItem();
    }

    public void CloseInventory()
    {
        if (!isOpen) return;
        isOpen = false;

        GlobalUIManager.Instance.ClosePanel(inventoryPanel);
        HUDCanvas.SetActive(true);

        DeselectItem();
    }

    public void CloseByButton() => CloseInventory();

    //  SELECT ITEM
    public void SelectItem(ItemSlot slot)
    {
        selectedSlot = slot;
        dropPanel.SetActive(true);

        // Nama lengkap item
        string suffix = slot.isSeed ? "Seed" : "Fruit";
        selectedItemText.text = $"{slot.itemID} {suffix}";

        // Reset listener
        dropOneButton.onClick.RemoveAllListeners();
        dropAllButton.onClick.RemoveAllListeners();
        TMP_Text dropAllTxt = dropAllButton?.GetComponentInChildren<TMP_Text>();

        // DROP ONE — tetap selected & update UI
        dropOneButton.onClick.AddListener(() =>
        {
            slot.DropItem(false);

            // Update jumlah di tombol Drop All
            int qty = slot.currentQuantity;
            if (dropAllTxt) dropAllTxt.text = $"Drop All ({qty})";

            // Update tulisan item (misal quantity berubah)
            selectedItemText.text = $"{slot.itemID} {suffix}";

            // Slot tetap selected → JANGAN deselect
            UIManager.Instance.UpdateInventoryUI();

            // Border tetap
            HighlightSlot(slot);
        });

        // DROP ALL — langsung deselect
        dropAllButton.onClick.AddListener(() =>
        {
            slot.DropItem(true);

            DeselectItem();
            UIManager.Instance.UpdateInventoryUI();
        });

        HighlightSlot(slot);

        UIManager.Instance.UpdateBagCapacity();
    }

    //  DESELECT ITEM
    public void DeselectItem()
    {
        selectedSlot = null;
        dropPanel.SetActive(false);

        // hilangkan semua highlight
        foreach (var slot in FindObjectsByType<ItemSlot>(FindObjectsSortMode.None))
        {
            if (slot.selectionBorder != null)
                slot.selectionBorder.enabled = false;
        }
    }

    //  HIGHLIGHT SLOT TERPILIH
    public void HighlightSlot(ItemSlot chosen)
    {
        foreach (var slot in FindObjectsByType<ItemSlot>(FindObjectsSortMode.None))
        {
            if (slot.selectionBorder == null) continue;

            slot.selectionBorder.enabled = (slot == chosen);
        }
    }

    //  BAG CHECK
    public bool CanAddItem(int total)
    {
        return total < maxBagCapacity;
    }
}