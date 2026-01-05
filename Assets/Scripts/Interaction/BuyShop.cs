using UnityEngine;

public class BuyShop : MonoBehaviour, IInteractable
{
    [Header("UI Panels")]
    public GameObject BuyPanel;   // Panel Shop di HUD Canvas

    private bool isPlayerInside = false;
    private bool isOpen = false;

    public string PromptMessage => isOpen ? "" : "[E] Buy Seeds";
    public InputType InputKey => InputType.E;

    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        isPlayerInside = true;
    }

    void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        isPlayerInside = false;
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
        PromptManager.Instance.HideContext(this);

        // Tampilkan panel shop
        GlobalUIManager.Instance.HUDCanvas.SetActive(false);
        GlobalUIManager.Instance.OpenPanel(BuyPanel);
    }

    public void CloseShop()
    {
        if (!isOpen) return;

        isOpen = false;
        GlobalUIManager.Instance.HUDCanvas.SetActive(true);
        GlobalUIManager.Instance.ClosePanel(BuyPanel);

        if (isPlayerInside)
            PromptManager.Instance.RefreshContext(this, PromptMessage);
    }
}