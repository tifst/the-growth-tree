using UnityEngine;
using System.Collections.Generic;

public class PlayerInteract : MonoBehaviour
{
    private readonly List<IInteractable> interactablesInRange = new();
    private readonly List<TriggerPickup> pickupsInRange = new();
    private readonly List<PlantPlot> plotsInRange = new();
    private PlantPlot activePlot;
    private IInteractable current;

    void Update()
    {
        // 1️⃣ PICKUP (aggregated)
        if (pickupsInRange.Count > 0 && !(current is QuestTrigger))
        {
            PromptManager.Instance.RefreshContext(this, "[R] Pick Fruit");

            if (Input.GetKeyDown(KeyCode.R))
            {
                var pickup = pickupsInRange[^1];
                pickupsInRange.Remove(pickup);
                pickup.Interact();
            }
        }
        else
        {
            PromptManager.Instance.HideContext(this);
        }

        // 2️⃣ PLOT (aggregated)
        if (activePlot != null &&
            (activePlot.growTree == null ||
            activePlot.growTree.GetActionOwner() == TreeActionOwner.Plot))
        {
            if (Input.GetKeyDown(KeyCode.E))
                activePlot.Interact();

            return;
        }

        // 3️⃣ OBJECT NORMAL
        if (current == null) return;

        if (Input.GetKeyDown(KeyCode.E) && current.InputKey == InputType.E)
            current.Interact();

        if (Input.GetKeyDown(KeyCode.R) && current.InputKey == InputType.R)
            current.Interact();
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent(out TriggerPickup pickup))
        {
            if (!pickupsInRange.Contains(pickup))
                pickupsInRange.Add(pickup);
            return;
        }

        if (other.TryGetComponent(out PlantPlot plot))
        {
            if (!plotsInRange.Contains(plot))
                plotsInRange.Add(plot);

            UpdateActivePlot();
            return;
        }

        if (other.TryGetComponent(out IInteractable interactable))
        {
            if (!interactablesInRange.Contains(interactable))
                interactablesInRange.Add(interactable);

            UpdateCurrent();
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.TryGetComponent(out TriggerPickup pickup))
        {
            pickupsInRange.Remove(pickup);
            return;
        }

        if (other.TryGetComponent(out PlantPlot plot))
        {
            plotsInRange.Remove(plot);
            PromptManager.Instance.HideContext(plot);
            UpdateActivePlot();
            return;
        }

        if (other.TryGetComponent(out IInteractable interactable))
        {
            interactablesInRange.Remove(interactable);
            PromptManager.Instance.HideContext(interactable as MonoBehaviour);
            UpdateCurrent();
        }
    }

    void UpdateCurrent()
    {
        current = interactablesInRange.Count > 0
            ? interactablesInRange[^1]
            : null;

        if (current != null && !string.IsNullOrEmpty(current.PromptMessage))
        {
            PromptManager.Instance.RefreshContext(
                current as MonoBehaviour,
                current.PromptMessage
            );
        }
    }

    void UpdateActivePlot()
    {
        foreach (var p in plotsInRange)
            PromptManager.Instance.HideContext(p);

        activePlot = plotsInRange.Count > 0
            ? plotsInRange[^1]   // atau nearest
            : null;

        if (activePlot != null && !string.IsNullOrEmpty(activePlot.PromptMessage))
        {
            PromptManager.Instance.RefreshContext(
                activePlot,
                activePlot.PromptMessage
            );
        }
    }

    public void ForceRefreshPlot(PlantPlot plot)
    {
        if (plot != activePlot) return;

        PromptManager.Instance.HideContext(plot);

        if (!string.IsNullOrEmpty(plot.PromptMessage))
            PromptManager.Instance.RefreshContext(plot, plot.PromptMessage);
    }
}