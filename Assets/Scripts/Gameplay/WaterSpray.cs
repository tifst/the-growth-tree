using UnityEngine;
using System.Collections;

public class WaterSpray : MonoBehaviour
{
    [Header("Visual")]
    [SerializeField] private ParticleSystem sprayFX;
    [SerializeField] private Transform sprayPoint;
    [SerializeField] private LayerMask treeMask;
    [SerializeField] private GameObject wateringCan;

    [Header("Spray Settings")]
    [SerializeField] private float sprayRange = 3f;
    [SerializeField] private float waterWastePerSecond = 10f;
    [SerializeField] private float waterCostPerHP = 1f;

    [Header("Refill Settings")]
    public float refillRatePerSecond = 200f;

    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip spraySound;
    [SerializeField] private AudioClip refillSound;

    private bool spraying;
    private bool refilling;
    public bool IsRefilling => refilling;
    private Coroutine refillCR;
    private WaterSource currentSource;

    void Update()
    {
        if (refilling) return;

        if (Input.GetKeyDown(KeyCode.Q))
            StartSpray();

        if (Input.GetKeyUp(KeyCode.Q))
            StopSpray();

        if (spraying)
            SprayLogic();
    }

    // ================= SPRAY =================
    void StartSpray()
    {
        if (GameManager.Instance.currentWater <= 0f)
        {
            PromptManager.Instance.Notify("Out of water!");
            return;
        }

        sprayFX?.Play();
        spraying = true;
        wateringCan?.SetActive(true);

        PlayLoop(spraySound);
    }

    void StopSpray()
    {
        spraying = false;
        wateringCan?.SetActive(false);
        sprayFX?.Stop();

        StopAudio();
    }

    void SprayLogic()
    {
        if (GameManager.Instance.currentWater <= 0)
        {
            StopSpray();
            PromptManager.Instance.Notify("Out of water!");
            return;
        }
        
        if (!sprayPoint) sprayPoint = transform;

        if (Physics.Raycast(
            sprayPoint.position,
            sprayPoint.forward,
            out RaycastHit hit,
            sprayRange,
            treeMask))
        {
            GrowTree tree = hit.collider.GetComponentInParent<GrowTree>();

            if (tree != null)
            {
                HealTree(tree);
                return;
            }
        }

        GameManager.Instance.ModifyWater(waterWastePerSecond * Time.deltaTime);
    }

    void HealTree(GrowTree tree)
    {
        if (GameManager.Instance.currentWater <= 0f)
        {
            StopSpray();
            return;
        }

        if (!tree.CanBeWatered())
        {
            // air tetap kebuang kalau nyiram pohon sehat
            GameManager.Instance.ModifyWater(waterWastePerSecond * Time.deltaTime);
            return;
        }

        float missingHP = tree.GetMissingHealth();
        float availableWater = GameManager.Instance.currentWater;

        TutorialEvents.OnWater?.Invoke();
        
        // maksimal heal dari air yang ada
        float maxHealFromWater = availableWater / waterCostPerHP;

        float healAmount = Mathf.Min(missingHP, maxHealFromWater);

        if (healAmount <= 0f)
        {
            StopSpray();
            return;
        }

        tree.currentHealth += healAmount;
        tree.OnHealed(); // biar daun balik sehat
        tree.PlayWaterEffect();
        tree.StopGrowEffectAfter(2f);

        float usedWater = healAmount * waterCostPerHP;
        GameManager.Instance.ModifyWater(usedWater);

        if (GameManager.Instance.currentWater <= 0f)
            StopSpray();
    }

    // ================= REFILL =================
    public void StartRefill()
    {
        if (refilling) return;

        refilling = true;
        StopSpray();
        wateringCan?.SetActive(true);

        if (currentSource != null)
        {
            PromptManager.Instance.RefreshContext(
                currentSource,
                "Refilling..."
            );
        }

        refillCR = StartCoroutine(RefillRoutine());
    }

    public void StopRefill()
    {
        if (!refilling) return;

        refilling = false;
        wateringCan?.SetActive(false);

        if (refillCR != null)
            StopCoroutine(refillCR);
    
        PromptManager.Instance.Notify("Refill canceled", 3f);

        StopAudio();
        refillCR = null;
    }

    IEnumerator RefillRoutine()
    {
        PlayLoop(refillSound);

        float max = GameManager.Instance.maxWater;

        while (refilling && GameManager.Instance.currentWater < max)
        {
            float add = refillRatePerSecond * Time.deltaTime;

            GameManager.Instance.currentWater =
                Mathf.Min(GameManager.Instance.currentWater + add, max);

            GameManager.Instance.uiManager?.UpdateWater(
                GameManager.Instance.currentWater, max);

            yield return null;
        }

        StopAudio();
        refilling = false;
        refillCR = null;
        wateringCan?.SetActive(false);

        if (currentSource != null)
        {
            PromptManager.Instance.RefreshContext(
                currentSource,
                "Water is full"
            );
        }
    }

    public void SetSource(WaterSource source)
    {
        currentSource = source;
    }

    public void ClearSource()
    {
        currentSource = null;
    }

    // ================= AUDIO =================
    void PlayLoop(AudioClip clip)
    {
        if (audioSource == null || clip == null) return;

        audioSource.clip = clip;
        audioSource.loop = true;
        audioSource.Play();
    }

    void StopAudio()
    {
        if (audioSource == null) return;

        audioSource.Stop();
        audioSource.loop = false;
    }
}
