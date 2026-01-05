using UnityEngine;

public class WaterSource : MonoBehaviour, IInteractable
{
    private bool isPlayerInside;
    private WaterSpray playerSpray;
    public InputType InputKey => InputType.R;
    public string PromptMessage
    {
        get
        {
            if (!isPlayerInside || playerSpray == null)
                return "";

            if (playerSpray.IsRefilling)
                return "Refilling...";

            if (GameManager.Instance.currentWater >= GameManager.Instance.maxWater)
                return "Water is full";

            return "[R] Refill Water";
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        playerSpray = other.GetComponentInChildren<WaterSpray>(true);
        if (playerSpray != null)
            playerSpray.SetSource(this);   // 🔥 TAMBAHKAN

        isPlayerInside = true;
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        if (playerSpray != null)
        {
            playerSpray.StopRefill();
            playerSpray.ClearSource();     // 🔥 TAMBAHKAN
        }

        isPlayerInside = false;
        playerSpray = null;
    }

    public void Interact()
    {
        if (!isPlayerInside) return;
        if (playerSpray == null) return;

        TutorialEvents.OnRefill?.Invoke();

        if (GameManager.Instance.currentWater >= GameManager.Instance.maxWater)
            return;

        playerSpray.StartRefill();
    }
}