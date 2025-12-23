using UnityEngine;
using UnityEngine.UI;

public class GradientBarController : MonoBehaviour
{
    private Image fillImage;

    [Header("Warna")]
    public Color MinColor = Color.red;
    public Color MaxColor = Color.green;
    
    private float targetFill = 1f;
    private Color targetMaxColor; 
    private float lerpSpeed = 5f;

    void Awake()
    {
        fillImage = GetComponent<Image>();
        targetMaxColor = MaxColor; 
    }

    void Update()
    {
        if (fillImage != null)
        {
            fillImage.fillAmount = Mathf.Lerp(fillImage.fillAmount, targetFill, Time.deltaTime * lerpSpeed);
            fillImage.color = Color.Lerp(MinColor, targetMaxColor, fillImage.fillAmount);
        }
    }

    public void UpdateBar(float val, Color colorOverride)
    {
        float norm = Mathf.Clamp01(val);
        targetFill = norm;
        targetMaxColor = colorOverride; 
    }
}
