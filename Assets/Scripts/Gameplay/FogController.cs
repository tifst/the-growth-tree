using UnityEngine;

public class FogController : MonoBehaviour
{
    public static FogController Instance;

    [Header("Fog Start Range")]
    public float fogStartClean = 25f;   // polusi 0%
    public float fogStartDirty = -25f;   // polusi 100%

    [Header("Fog End Range")]
    public float fogEndClean = 125f;     // polusi 0%
    public float fogEndDirty = 26f;     // polusi 100%

    [Header("Fog Colors")]
    public Color lowPollutionColor = new Color(0.85f, 0.95f, 1f);
    public Color highPollutionColor = new Color(0.4f, 0.45f, 0.4f);

    private float pollution01 = 0f;

    void Awake()
    {
        Instance = this;

        RenderSettings.fog = true;
        RenderSettings.fogMode = FogMode.Linear;
    }

    void Update()
    {
        ApplyFog();
    }

    // dipanggil dari GameManager
    public void SetPollution(float percent)   // percent: 0–100
    {
        pollution01 = Mathf.InverseLerp(0f, 100f, percent);
    }

    private void ApplyFog()
    {
        // Remap pollution → Start fog
        float start = Mathf.Lerp(fogStartClean, fogStartDirty, pollution01);

        // Remap pollution → End fog
        float end = Mathf.Lerp(fogEndClean, fogEndDirty, pollution01);

        // Smooth transition
        RenderSettings.fogStartDistance = Mathf.Lerp(
            RenderSettings.fogStartDistance, 
            start, 
            Time.deltaTime * 2f
        );

        RenderSettings.fogEndDistance = Mathf.Lerp(
            RenderSettings.fogEndDistance, 
            end, 
            Time.deltaTime * 2f
        );

        // Fog color
        RenderSettings.fogColor = Color.Lerp(
            lowPollutionColor,
            highPollutionColor,
            pollution01
        );
    }
}