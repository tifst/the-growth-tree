using UnityEngine;

public class WaterSource : MonoBehaviour, IInteractable
{
    [Header("Refill Settings")]
    private bool isPlayerInside = false;
    private WaterSpray playerSpray;

    public string PromptMessage => "[R] Refill Water";
    public InputType InputKey => InputType.R;

    // TRIGGER MASUK
    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        playerSpray = other.GetComponentInChildren<WaterSpray>();
        isPlayerInside = true;

        // tampilkan prompt hanya jika belum penuh
        if (GameManager.Instance.currentWater < GameManager.Instance.maxWater)
            PromptUI.Instance.Show(PromptMessage, this);
    }

    // TRIGGER KELUAR
    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        // hentikan refill jika sedang refill
        if (playerSpray != null)
            playerSpray.StopRefill();

        isPlayerInside = false;
        playerSpray = null;

        // hide prompt hanya jika pemiliknya adalah well ini
        PromptUI.Instance.Hide(this);
    }

    // INTERACT DIPANGGIL PlayerInteract
    public void Interact()
    {
        if (!isPlayerInside) return;
        if (playerSpray == null) return;

        // kalau air penuh, jangan refill
        if (GameManager.Instance.currentWater >= GameManager.Instance.maxWater)
        {
            PromptUI.Instance.Hide(this);
            return;
        }

        // mulai refill
        playerSpray.StartRefill();

        // sembunyikan prompt saat refill mulai
        PromptUI.Instance.Hide(this);
    }
}