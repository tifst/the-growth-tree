using UnityEngine;
using System.Collections;

public class TriggerPickup : MonoBehaviour, IInteractable
{
    [HideInInspector] public TreeData sourceTreeData; // diisi oleh FruitPoolManager

    private bool isPlayerInside = false;

    public string PromptMessage => "";
    public InputType InputKey => InputType.R;

    // === TRIGGER ===
    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        isPlayerInside = true;
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        isPlayerInside = false;
    }

    // === INTERACT (dipanggil PlayerInteract) ===
    public void Interact()
    {
        if (!isPlayerInside) return;
        if (sourceTreeData == null) return;

        TutorialEvents.OnPickupFruit?.Invoke();

        GameManager.Instance.ModifyFruitStock(sourceTreeData.treeName, 1);
        GameManager.Instance.AddXP(sourceTreeData.xpRewardHarvest);
        QuestManager.Instance.AddProgress(sourceTreeData.treeName, QuestGoalType.HarvestFruit);

        PromptManager.Instance.Notify($"1 {sourceTreeData.treeName} is picked up!");
        FruitPoolManager.Instance.ReturnFruit(gameObject);
    }
}
