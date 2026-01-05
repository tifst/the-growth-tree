using UnityEngine;

public class SellShop : MonoBehaviour, IInteractable
{
    [Header("UI Panels")]
    public GameObject SellPanel;

    private bool isPlayerInside = false;
    private bool isOpen = false;

    public string PromptMessage => isOpen ? "" : "[E] Sell Fruits";
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
            CloseShop();
    }

    public void Interact()
    {
        if (!isPlayerInside || isOpen) return;

        isOpen = true;
        PromptManager.Instance.HideContext(this);

        GlobalUIManager.Instance.HUDCanvas.SetActive(false);
        GlobalUIManager.Instance.OpenPanel(SellPanel);
    }

    public void CloseShop()
    {
        if (!isOpen) return;

        isOpen = false;
        GlobalUIManager.Instance.HUDCanvas.SetActive(true);
        GlobalUIManager.Instance.ClosePanel(SellPanel);

        if (isPlayerInside)
            PromptManager.Instance.RefreshContext(this, PromptMessage);
    }
}