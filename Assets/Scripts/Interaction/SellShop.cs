using UnityEngine;

public class SellShop : MonoBehaviour, IInteractable
{
    [Header("UI Panels")]
    public GameObject SellPanel;   // Panel UI jual buah

    private bool isPlayerInside = false;
    private bool isOpen = false;

    public string PromptMessage => "[E] Sell Fruits";
    public InputType InputKey => InputType.E;

    // ===== TRIGGER MASUK =====
    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        isPlayerInside = true;

        if (!isOpen)
            PromptUI.Instance.Show(PromptMessage, this);
    }

    // ===== TRIGGER KELUAR =====
    void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        isPlayerInside = false;

        PromptUI.Instance.Hide(this);
    }

    void Update()
    {
        if (isOpen && Input.GetKeyDown(KeyCode.Escape))
            CloseShop();
    }
    
    // ===== INTERACT (dipanggil PlayerInteract) =====
    public void Interact()
    {
        if (!isPlayerInside || isOpen) return;
        isOpen = true;

        GlobalUIManager.Instance.HUDCanvas.SetActive(false);
        PromptUI.Instance.Hide(this);

        GlobalUIManager.Instance.OpenPanel(SellPanel);
    }

    // ===== TUTUP SHOP (ESC atau tombol X UI) =====
    public void CloseShop()
    {
        SaveLoadSystem.Instance.SaveGame();

        if (!isOpen) return;
        isOpen = false;

        GlobalUIManager.Instance.HUDCanvas.SetActive(true);
        GlobalUIManager.Instance.ClosePanel(SellPanel);

        if (isPlayerInside)  // tetap di area → prompt muncul lagi
            PromptUI.Instance.Show(PromptMessage, this);
    }
}