using UnityEngine;
using System.Collections;

public class QuestTrigger : MonoBehaviour, IInteractable
{
    [Header("Quest")]
    public QuestData questToGive;
    public Transform npcHead;

    private bool isPlayerInside = false;

    public string PromptMessage => "[R] Take Quest";
    public InputType InputKey => InputType.R;

    // ========== TRIGGER MASUK ==========
    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        isPlayerInside = true;

        PromptUI.Instance.Show(PromptMessage, this);
    }

    // ========== TRIGGER KELUAR ==========
    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        isPlayerInside = false;

        PromptUI.Instance.Hide(this);
        PopupQuest.Instance.Hide();
    }

    private IEnumerator ShowQuestMessage(string msg, float duration = 2f)
    {
        PromptUI.Instance.Show(msg, this);
        yield return new WaitForSeconds(duration);
        PromptUI.Instance.Hide(this);
    }

    public void Interact()
    {
        if (!isPlayerInside) return;

        PromptUI.Instance.Hide(this);

        // Popup quest muncul
        PopupQuest.Instance.Show(questToGive, npcHead);

        if (!QuestManager.Instance.HasQuest(questToGive))
        {
            QuestManager.Instance.StartQuest(questToGive);
            StartCoroutine(ShowQuestMessage("New quest taken: " + questToGive.questTitle));
        }
        else
        {
            StartCoroutine(ShowQuestMessage("Quest already taken, complete it first!"));
        }
    }
}