using UnityEngine;
using System.Collections;

public class WaterSpray : MonoBehaviour
{
    [Header("Referensi")]
    [SerializeField] private ParticleSystem ps;
    [SerializeField] private Transform sprayPoint;
    [SerializeField] private LayerMask treeMask;
    [SerializeField] private GameObject wateringCan;

    [Header("Settings")]
    [SerializeField] private float sprayRange = 3f;
    public float refillRatePerSecond = 100f;

    [Header("Costs")]
    public float waterCostPerHP = 1f;
    public float waterWastePerSecond = 10f;

    [Header("Audio Settings")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip spraySound;
    [SerializeField] private AudioClip refillSound;

    private bool spraying = false;
    private bool refilling = false;

    private Coroutine refillCR;

    private void Update()
    {
        if (refilling) return;

        if (Input.GetKeyDown(KeyCode.Q)) TryStartSpray();
        else if (Input.GetKeyUp(KeyCode.Q)) StopSpray();

        if (spraying)
            SprayLogic();
    }

    private void TryStartSpray()
    {
        if (GameManager.Instance.currentWater <= 0)
        {
            StartCoroutine(ShowEmptyWaterPrompt());
            return;
        }

        spraying = true;
        wateringCan?.SetActive(true);
        ps?.Play();

        if (audioSource != null && spraySound != null && !audioSource.isPlaying)
        {
            audioSource.clip = spraySound;
            audioSource.loop = true;
            audioSource.Play();
        }
    }

    private IEnumerator ShowEmptyWaterPrompt()
    {
        PromptUI.Instance.Show("Air habis!", this);
        yield return new WaitForSeconds(2f);
        PromptUI.Instance.Hide(this);
    }

    private void StopSpray()
    {
        spraying = false;
        wateringCan?.SetActive(false);
        ps?.Stop();

        if (audioSource != null && audioSource.isPlaying && audioSource.clip == spraySound)
        {
            audioSource.Stop();
            audioSource.loop = false;
        }
    }

    private void SprayLogic()
    {
        if (!sprayPoint) sprayPoint = transform;

        if (Physics.Raycast(sprayPoint.position, sprayPoint.forward, out RaycastHit hit, sprayRange, treeMask))
        {
            GrowTree tree = hit.collider.GetComponentInParent<GrowTree>();

            if (tree != null)
            {
                if (tree.CurrentHealthPercent >= 0.4f)
                {
                    GameManager.Instance.ModifyWater(waterWastePerSecond * Time.deltaTime);
                    return;
                }

                HealTree(tree);
                return;
            }
        }

        GameManager.Instance.ModifyWater(waterWastePerSecond * Time.deltaTime);

        if (GameManager.Instance.currentWater <= 0)
            StopSpray();
    }

    private void HealTree(GrowTree tree)
    {
        float missingHP = tree.maxHealth - tree.currentHealth;
        if (missingHP <= 0.01f) return;

        float requiredWater = missingHP * waterCostPerHP;

        if (GameManager.Instance.currentWater < requiredWater)
        {
            float healAmount = Mathf.Min(GameManager.Instance.currentWater / waterCostPerHP, missingHP);
            tree.currentHealth += healAmount;

            GameManager.Instance.ModifyWater(GameManager.Instance.currentWater); 
            StopSpray();
            return;
        }

        tree.currentHealth = tree.maxHealth;
        GameManager.Instance.ModifyWater(requiredWater);
    }

    // =============== REFILL SYSTEM ===============

    public void StartRefill()
    {
        if (refilling) return;

        refilling = true;
        refillCR = StartCoroutine(RefillRoutine());
    }

    public void StopRefill()
    {
        if (!refilling) return;

        refilling = false;

        if (refillCR != null)
            StopCoroutine(refillCR);

        PromptUI.Instance.Hide(this);
        PromptUI.Instance.Show("Refill dibatalkan!", this);

        if (audioSource != null) audioSource.Stop();
    }

    private IEnumerator RefillRoutine()
    {
        StopSpray();

        float max = GameManager.Instance.maxWater;

        if (GameManager.Instance.currentWater >= max - 0.01f)
        {
            PromptUI.Instance.Show("Water already full!", this);
            yield return new WaitForSeconds(2f);
            PromptUI.Instance.Hide(this);
            refilling = false;
            refillCR = null;
            yield break;
        }

        PromptUI.Instance.Show("Refilling water...", this);

        if (audioSource != null && refillSound != null)
        {
            audioSource.clip = refillSound;
            audioSource.loop = true; // Loop karena ngisi butuh waktu
            audioSource.Play();
        }

        while (refilling && GameManager.Instance.currentWater < max)
        {
            float add = refillRatePerSecond * Time.deltaTime;

            GameManager.Instance.currentWater = Mathf.Min(GameManager.Instance.currentWater + add, max);

            if (GameManager.Instance.uiManager != null)
                GameManager.Instance.uiManager.UpdateWater(GameManager.Instance.currentWater, max);

            if (UIManager.Instance != null && UIManager.Instance.waterBar != null)
                UIManager.Instance.waterBar.UpdateBar(GameManager.Instance.currentWater / max, Color.cyan);

            yield return null;
        }

        if (audioSource != null) audioSource.Stop();

        if (!refilling)
        {
            refillCR = null;
            yield break;
        }

        PromptUI.Instance.Show("Water refilled!", this);
        yield return new WaitForSeconds(2f);
        PromptUI.Instance.Hide(this);

        refilling = false;
        refillCR = null;
    }
}
