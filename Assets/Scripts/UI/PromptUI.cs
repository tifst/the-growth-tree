using UnityEngine;
using TMPro;
using System.Collections;

public class PromptUI : MonoBehaviour
{
    public static PromptUI Instance;

    public GameObject promptRoot;
    public TMP_Text promptText;
    public RectTransform promptRect;
    public Vector2 anchoredPos = new Vector2(0, -300);

    private MonoBehaviour currentOwner = null;

    void Awake()
    {
        Instance = this;
    } 

    public void ShowPickupMessage(string msg, float duration = 2f)
    {
        StartCoroutine(ShowMessageRoutine(msg, duration));
    }

    private IEnumerator ShowMessageRoutine(string msg, float duration)
    {
        Show(msg, null);
        yield return new WaitForSeconds(duration);
        Hide(null);
    }

    public void Show(string msg, MonoBehaviour owner)
    {
        currentOwner = owner;
        promptRect.anchoredPosition = anchoredPos;
        promptText.text = msg;
        promptRoot.SetActive(true);
    }

    public void Show(string msg, MonoBehaviour owner, Vector2 anchoredPos)
    {
        currentOwner = owner;
        promptText.text = msg;

        promptRect.anchoredPosition = anchoredPos;
        promptRoot.SetActive(true);
    }

    public void Hide(MonoBehaviour owner)
    {
        if (owner == currentOwner || owner == null)
        {
            promptRoot.SetActive(false);
            currentOwner = null;
        }
    }
}
