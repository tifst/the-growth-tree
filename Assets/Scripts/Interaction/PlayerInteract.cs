using UnityEngine;

public class PlayerInteract : MonoBehaviour
{
    private IInteractable currentInteractable;

    void Update()
    {
        if (currentInteractable == null) return;

        InputType key = currentInteractable.InputKey;

        if (key == InputType.E && Input.GetKeyDown(KeyCode.E))
            currentInteractable.Interact();

        if (key == InputType.Q && Input.GetKeyDown(KeyCode.Q))
            currentInteractable.Interact();

        if (key == InputType.R && Input.GetKeyDown(KeyCode.R))
            currentInteractable.Interact();
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent(out IInteractable interactable))
        {
            currentInteractable = interactable;
            PromptUI.Instance.Show(interactable.PromptMessage, this);
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.TryGetComponent(out IInteractable interactable))
        {
            if (currentInteractable == interactable)
            {
                currentInteractable = null;
                PromptUI.Instance.Hide(this);
            }
        }
    }
}