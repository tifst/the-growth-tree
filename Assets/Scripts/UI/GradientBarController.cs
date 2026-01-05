using UnityEngine;
using UnityEngine.UI;

public class GradientBarController : MonoBehaviour
{
    private Image fillImage;

    [Header("Gradient Colors (Inspector)")]
    public Color MinColor = Color.red;
    public Color MaxColor = Color.green;

    [Header("Animation")]
    [SerializeField] private float lerpSpeed = 5f;

    private float targetFill = 1f;
    private Color currentMaxColor;
    private bool useOverrideColor = false;

    void Awake()
    {
        fillImage = GetComponent<Image>();
        currentMaxColor = MaxColor;
    }

    void Update()
    {
        if (fillImage == null) return;

        fillImage.fillAmount =
            Mathf.Lerp(fillImage.fillAmount, targetFill, Time.deltaTime * lerpSpeed);

        Color max = useOverrideColor ? currentMaxColor : MaxColor;

        fillImage.color =
            Color.Lerp(MinColor, max, fillImage.fillAmount);
    }

    public void UpdateBar(float value)
    {
        targetFill = Mathf.Clamp01(value);
        useOverrideColor = false;
    }

    public void UpdateBar(float value, Color overrideMaxColor)
    {
        targetFill = Mathf.Clamp01(value);
        currentMaxColor = overrideMaxColor;
        useOverrideColor = true;
    }

    public void ResetColor()
    {
        useOverrideColor = false;
        currentMaxColor = MaxColor;
    }
}