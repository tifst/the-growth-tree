using UnityEngine;
using TMPro;
using System.Collections;

public enum PromptType
{
    Notification,
    Context
}

public class PromptInstance : MonoBehaviour
{
    [Header("UI")]
    public TMP_Text text;
    public RectTransform rect;

    [HideInInspector] public MonoBehaviour owner;
    [HideInInspector] public PromptType type;

    public System.Action<PromptInstance> onDestroyed;

    Coroutine autoHideCR;

    // ================= SETUP =================
    public void Setup(
        string msg,
        MonoBehaviour owner,
        PromptType type,
        float duration
    )
    {
        this.owner = owner;
        this.type = type;

        text.text = msg;
        gameObject.SetActive(true);

        if (type == PromptType.Notification && duration > 0f)
        {
            autoHideCR = StartCoroutine(AutoHide(duration));
        }
    }

    public void SetText(string msg)
    {
        text.text = msg;
    }

    IEnumerator AutoHide(float duration)
    {
        yield return new WaitForSeconds(duration);
        Hide();
    }

    // ================= HIDE =================
    public void Hide()
    {
        if (autoHideCR != null)
            StopCoroutine(autoHideCR);

        onDestroyed?.Invoke(this);
        Destroy(gameObject);
    }
}