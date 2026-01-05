using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class LoadingScreenUI : MonoBehaviour
{
    public static LoadingScreenUI Instance;

    [Header("Canvas Group")]
    public CanvasGroup canvasGroup;

    [Header("UI")]
    public Image progressFill;
    public TMP_Text loadingText;
    public TMP_Text percentText;

    [Header("Animation")]
    public float fadeSpeed = 4f;
    public float lerpSpeed = 6f;

    float targetProgress;
    float currentProgress;
    bool fadingIn;
    bool fadingOut;

    void Awake()
    {
        // 🔥 SINGLETON AMAN
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        // 🔥 ROOT OBJECT PERSISTENT
        DontDestroyOnLoad(transform.root.gameObject);

        // 🔥 SAFETY INIT
        if (canvasGroup != null)
        {
            canvasGroup.alpha = 0f;
            canvasGroup.blocksRaycasts = false;
            canvasGroup.interactable = false;
        }
    }

    void Update()
    {
        // Kalau object sudah tidak valid, hentikan Update
        if (canvasGroup == null) return;

        // Smooth progress
        currentProgress = Mathf.Lerp(
            currentProgress,
            targetProgress,
            Time.deltaTime * lerpSpeed
        );

        if (progressFill)
            progressFill.fillAmount = currentProgress;

        if (percentText)
            percentText.text = Mathf.RoundToInt(currentProgress * 100f) + "%";

        // Fade in
        if (fadingIn)
        {
            canvasGroup.alpha = Mathf.MoveTowards(
                canvasGroup.alpha, 1f, Time.deltaTime * fadeSpeed
            );

            if (canvasGroup.alpha >= 1f)
                fadingIn = false;
        }

        // Fade out
        if (fadingOut)
        {
            canvasGroup.alpha = Mathf.MoveTowards(
                canvasGroup.alpha, 0f, Time.deltaTime * fadeSpeed
            );

            if (canvasGroup.alpha <= 0f)
            {
                fadingOut = false;
                canvasGroup.blocksRaycasts = false;
            }
        }
    }

    // ================= API =================

    public void Show(string text)
    {
        if (canvasGroup == null) return;

        if (loadingText)
            loadingText.text = text;

        currentProgress = targetProgress = 0f;

        canvasGroup.blocksRaycasts = true;
        fadingOut = false;
        fadingIn = true;
    }

    public void Hide()
    {
        if (canvasGroup == null) return;

        fadingIn = false;
        fadingOut = true;
    }

    public void SetProgress(float value)
    {
        targetProgress = Mathf.Clamp01(value);
    }

    public void SetText(string text)
    {
        if (loadingText)
            loadingText.text = text;
    }
}