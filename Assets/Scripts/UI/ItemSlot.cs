using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class ItemSlot : MonoBehaviour
{
    [Header("Item Data")]
    public string itemID;
    public bool isSeed;

    [Header("UI")]
    public Image icon;
    public TextMeshProUGUI quantityText;
    public Image selectionBorder; // 🆕 border outline

    public int currentQuantity;
    private TreeData data;

    void Start()
    {
        FindTreeData();
        AssignIcon();
        UpdateSlotUI();

        // sembunyikan border awal
        if (selectionBorder != null)
            selectionBorder.enabled = false;
    }

    //   AMBIL TREEDATA DARI GAMEMANAGER
    void FindTreeData()
    {
        foreach (var t in GameManager.Instance.allTrees)
        {
            if (t != null && t.treeName == itemID)
            {
                data = t;
                return;
            }
        }

        Debug.LogWarning($"TreeData '{itemID}' tidak ditemukan di GameManager!");
    }

    //   SET ICON
    void AssignIcon()
    {
        if (data == null) return;

        icon.sprite = isSeed ? data.seedIcon : data.fruitIcon;
    }

    //   UPDATE SLOT UI
    public void UpdateSlotUI()
    {
        if (data == null) return;

        currentQuantity = isSeed
            ? GameManager.Instance.GetSeedStock(itemID)
            : GameManager.Instance.GetFruitStock(itemID);

        // jika 0 → hide slot
        if (currentQuantity <= 0)
        {
            gameObject.SetActive(false);
            return;
        }

        // muncul kembali
        if (!gameObject.activeSelf)
            gameObject.SetActive(true);

        // quantity
        quantityText.text = currentQuantity.ToString();
        icon.enabled = true;

        // reset border
        if (selectionBorder != null)
            selectionBorder.enabled = false;
    }

    //   KLIK SLOT
    public void OnSlotClicked()
    {
        if (currentQuantity <= 0)
        {
            InventorySystem.Instance.DeselectItem();
            return;
        }

        InventorySystem.Instance.SelectItem(this);
        InventorySystem.Instance.HighlightSlot(this);

        if (selectionBorder != null)
            selectionBorder.enabled = true;
    }

    //   DROP ITEM
    public void DropItem(bool dropAll)
    {
        if (currentQuantity <= 0) return;

        int amount = dropAll ? currentQuantity : 1;

        if (isSeed)
            GameManager.Instance.ModifySeedStock(itemID, -amount);
        else
            GameManager.Instance.ModifyFruitStock(itemID, -amount);

        // Jika dropOne → tetap selected
        // Jika dropAll → deselect karena slot hilang

        if (dropAll)
        {
            InventorySystem.Instance.DeselectItem();
        }

        UIManager.Instance.UpdateInventoryUI();
    }
}
