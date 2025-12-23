using UnityEngine;

public class BuyShop : MonoBehaviour, IInteractable
{
    [Header("UI Panels")]
    public GameObject BuyPanel;   // Panel Shop di HUD Canvas

    private bool isPlayerInside = false;
    private bool isOpen = false;

    public string PromptMessage => "[E] Buy Seeds";
    public InputType InputKey => InputType.E;

    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        isPlayerInside = true;
        PromptUI.Instance.Show(PromptMessage, this);
    }

    void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        isPlayerInside = false;
        PromptUI.Instance.Hide(this);
    }

    void Update()
    {
        if (isOpen && Input.GetKeyDown(KeyCode.Escape))
        {
            CloseShop();
        }
    }

    public void Interact()
    {
        if (!isPlayerInside || isOpen) return;
        isOpen = true;

        // Tampilkan panel shop
        GlobalUIManager.Instance.HUDCanvas.SetActive(false);
        PromptUI.Instance.Hide(this);

        GlobalUIManager.Instance.OpenPanel(BuyPanel);
    }

    public void CloseShop()
    {
        SaveLoadSystem.Instance.SaveGame();

        if (!isOpen) return;
        isOpen = false;

        GlobalUIManager.Instance.HUDCanvas.SetActive(true);
        GlobalUIManager.Instance.ClosePanel(BuyPanel);

        // Kalau player masih di area, tampilkan prompt lagi
        if (isPlayerInside)
            PromptUI.Instance.Show(PromptMessage, this);
    }
}