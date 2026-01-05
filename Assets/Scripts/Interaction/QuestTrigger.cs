using UnityEngine;

public class QuestTrigger : MonoBehaviour, IInteractable
{
    [Header("Quest")]
    public QuestData questToGive;
    public Transform npcPoint;

    private bool isPlayerInside;

    public string PromptMessage => "[R] Talk";
    public InputType InputKey => InputType.R;

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        isPlayerInside = true;
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        isPlayerInside = false;
        PopupQuest.Instance.Hide();
    }

    public void Interact()
    {
        if (!isPlayerInside) return;

        // ❗ JANGAN start quest / invoke tutorial DI SINI
        GuideManager.Instance.RemoveTarget(transform);

        PromptManager.Instance.RefreshContext(
            this,
            QuestManager.Instance.HasQuest(questToGive)
                ? "Quest already taken, complete it first!"
                : "[Enter] Confirm quest"
        );

        // tampilkan popup
        PopupQuest.Instance.Show(questToGive, npcPoint, this);
    }
}