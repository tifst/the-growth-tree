using UnityEngine;
using System.Collections;

public class TriggerPickup : MonoBehaviour, IInteractable
{
    [HideInInspector] public TreeData sourceTreeData; // diisi oleh FruitPoolManager

    private bool isPlayerInside = false;

    public string PromptMessage => "[R] Pick Fruit";
    public InputType InputKey => InputType.R;

    // === TRIGGER ===
    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        isPlayerInside = true;

        // Tampilkan prompt
        PromptUI.Instance.Show(PromptMessage, this);
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        isPlayerInside = false;

        // Hilangkan prompt
        PromptUI.Instance.Hide(this);
    }

    // === INTERACT (dipanggil PlayerInteract) ===
    public void Interact()
    {
        if (!isPlayerInside) return;
        if (sourceTreeData == null) return;

        SaveLoadSystem.Instance.SaveGame();

        // 1. Tambah buah ke inventory
        GameManager.Instance.ModifyFruitStock(sourceTreeData.treeName, 1);

        // 2. Quest progress
        QuestManager.Instance.AddProgress(sourceTreeData.treeName, QuestGoalType.HarvestFruit);

        // 3. Hilangkan prompt dulu
        PromptUI.Instance.Hide(this);
        isPlayerInside = false;

        // 4. Tampilkan pesan pickup (coroutine berjalan di PromptUI yang aktif)
        PromptUI.Instance.ShowPickupMessage($"1 Buah {sourceTreeData.treeName} diambil!");

        // 5. Kembalikan ke pool (object langsung inactive)
        FruitPoolManager.Instance.ReturnFruit(gameObject);
    }

    private IEnumerator ShowPickupMessage(string msg, float duration = 2f)
    {
        PromptUI.Instance.Show(msg, this);
        yield return new WaitForSeconds(duration);
        PromptUI.Instance.Hide(this);
    }
}
