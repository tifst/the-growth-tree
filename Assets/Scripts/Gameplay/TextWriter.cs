using UnityEngine;
using TMPro;
using System.Collections;

public class TextWriter : MonoBehaviour
{
    private TMP_Text textUI;
    private Coroutine typingCR;

    void Awake()
    {
        textUI = GetComponent<TMP_Text>();
    }

    public void Write(string message, float speed = 0.1f)
    {
        if (typingCR != null)
            StopCoroutine(typingCR);

        typingCR = StartCoroutine(TypeRoutine(message, speed));
    }

    private IEnumerator TypeRoutine(string message, float speed)
    {
        textUI.text = "";

        foreach (char c in message)
        {
            textUI.text += c;
            yield return new WaitForSeconds(speed);
        }
    }
}