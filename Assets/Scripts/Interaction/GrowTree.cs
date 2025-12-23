using UnityEngine;
using System.Collections;

public class GrowTree : MonoBehaviour
{
    [Header("Scriptable Data")]
    [SerializeField] public TreeData treeData;

    [Header("Effects")]
    [SerializeField] private ParticleSystem waterEffect;

    [Header("Prompt")]
    public string waterPrompt = "[Q] Water the Tree";
    public float promptYOffset = 80f;

    // INTERNAL VISUAL
    private Renderer leafRenderer;
    private Material leafMatInstance;
    private Coroutine colorTransitionRoutine;

    // STATE
    public float currentHealth;
    public float maxHealth => treeData != null ? treeData.maxHealth : 100f;
    private float growTimer;
    private bool isFullyGrown = false;
    private bool wasFullyGrown = false;
    private bool recentlyWatered = false;
    private bool isDead = false;
    private bool isWithered = false;

    private Vector3 minScale;
    private bool playerNearby = false;

    public bool IsFullyGrown => isFullyGrown;
    public float CurrentHealthPercent => Mathf.Clamp01(currentHealth / maxHealth);
    public bool IsTreeDead() => currentHealth <= 0f;

    void Start()
    {
        if (treeData == null)
        {
            Debug.LogWarning($"⚠️ TreeData belum di-set pada {name}");
            enabled = false;
            return;
        }

        // Ambil komponen efek dari anak prefab (jika ada)
        AutoFindEffects();

        minScale = treeData.maxScale * 0.1f;
        transform.localScale = minScale;

        currentHealth = treeData.startHealth;

        DetectLeafRenderer();

        if (waterEffect != null)
            waterEffect.Stop();
    }

    void Update()
    {
        if (treeData == null || isDead) return;

        HandleHealthDecay();
        UpdateVisualByHealth();
        HandleGrowth();
        HandleWaterPrompt();

        if (recentlyWatered && waterEffect != null && !waterEffect.isPlaying)
            recentlyWatered = false;
    }

    private void AutoFindEffects()
    {
        if (waterEffect == null)
            waterEffect = GetComponentInChildren<ParticleSystem>(true);
    }

    public void SetPlayerNearby(bool b)
    {
        playerNearby = b;
    }


    // PROMPT AIR
    private void HandleWaterPrompt()
    {
        if (PromptUI.Instance == null) return;

        bool need = CurrentHealthPercent < treeData.waterNeedThreshold && !isDead;

        if (playerNearby && need)
        {
            PromptUI.Instance.Show(waterPrompt, this);

            if (PromptUI.Instance.promptRoot != null)
            {
                RectTransform r = PromptUI.Instance.promptRoot.GetComponent<RectTransform>();
                r.anchoredPosition = new Vector2(0, promptYOffset);
            }
        }
        else
        {
            PromptUI.Instance.Hide(this);
        }
    }


    // HEALTH LOGIC
    private void HandleHealthDecay()
    {
        float decay =
            isFullyGrown ? treeData.grownDecayRate :
            (CurrentHealthPercent < treeData.waterNeedThreshold ?
                treeData.criticalDecayRate :
                treeData.healthyDecayRate);

        currentHealth = Mathf.Clamp(currentHealth - decay * Time.deltaTime, 0f, maxHealth);

        if (currentHealth <= 0f && !isDead)
            HandleTreeDeath();
    }


    // GROWTH
    private void HandleGrowth()
    {
        if (isFullyGrown) return;

        growTimer += Time.deltaTime;
        growTimer += Time.deltaTime * (CurrentHealthPercent >= 0.4f ? 1f : 0.5f);

        float progress = Mathf.Clamp01(growTimer / treeData.timeNeededToGrow);
        transform.localScale = Vector3.Lerp(minScale, treeData.maxScale, progress);

        if (progress >= 1f && !wasFullyGrown)
        {
            wasFullyGrown = true;
            isFullyGrown = true;
            OnFullyGrown();
        }
    }

    private void OnFullyGrown()
    {
        SaveLoadSystem.Instance.SaveGame();

        if (waterEffect != null)
        {
            var fx = Instantiate(waterEffect, transform.position + Vector3.up * 2f, Quaternion.identity);
            fx.Play();
            Destroy(fx.gameObject, 2f);
        }

        GameManager.Instance?.AddXP(treeData.xpRewardPlant);
        GameManager.Instance?.ModifyPollution(-treeData.pollutionReduction);
        QuestManager.Instance?.AddProgress(treeData.treeName, QuestGoalType.PlantTree);

        Debug.Log($"🌳 {treeData.treeName} fully grown!");
    }


    // MATERIAL DAUN
    private void DetectLeafRenderer()
    {
        Renderer[] renderers = GetComponentsInChildren<Renderer>(true);

        foreach (var r in renderers)
        {
            if (r is ParticleSystemRenderer) continue;

            var mats = r.sharedMaterials;
            for (int i = 0; i < mats.Length; i++)
            {
                if (mats[i] == null) continue;

                if (mats[i].name.ToLower().Contains("leaf") ||
                    mats[i].shader.name.ToLower().Contains("foliage"))
                {
                    leafRenderer = r;

                    leafMatInstance = new Material(
                        treeData.healthyLeafMaterial != null ?
                        treeData.healthyLeafMaterial :
                        mats[i]);

                    mats[i] = leafMatInstance;
                    r.materials = mats;
                    return;
                }
            }
        }
    }


    private void UpdateVisualByHealth()
    {
        if (leafMatInstance == null) return;

        float hp = CurrentHealthPercent;

        if (!isWithered && hp < 0.4f)
        {
            isWithered = true;
            StartLeafTransition(treeData.healthyLeafMaterial, treeData.dryLeafMaterial, 2f);
        }
        else if (isWithered && hp > 0.6f)
        {
            isWithered = false;
            StartLeafTransition(treeData.dryLeafMaterial, treeData.healthyLeafMaterial, 2f);
        }
    }


    private void StartLeafTransition(Material fromMat, Material toMat, float duration)
    {
        if (leafMatInstance == null || toMat == null) return;

        if (colorTransitionRoutine != null)
            StopCoroutine(colorTransitionRoutine);

        Color fromTop = leafMatInstance.HasProperty("_TopColor") ?
            leafMatInstance.GetColor("_TopColor") : Color.white;

        Color fromGround = leafMatInstance.HasProperty("_GroundColor") ?
            leafMatInstance.GetColor("_GroundColor") : fromTop;

        Color toTop = toMat.HasProperty("_TopColor") ?
            toMat.GetColor("_TopColor") : Color.white;

        Color toGround = toMat.HasProperty("_GroundColor") ?
            toMat.GetColor("_GroundColor") : toTop;

        colorTransitionRoutine = StartCoroutine(
            LeafColorTransitionRoutine(fromTop, toTop, fromGround, toGround, duration));
    }


    private IEnumerator LeafColorTransitionRoutine(
        Color fromTop, Color toTop,
        Color fromGround, Color toGround,
        float duration)
    {
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.SmoothStep(0f, 1f, elapsed / duration);

            if (leafMatInstance != null)
            {
                if (leafMatInstance.HasProperty("_TopColor"))
                    leafMatInstance.SetColor("_TopColor", Color.Lerp(fromTop, toTop, t));

                if (leafMatInstance.HasProperty("_GroundColor"))
                    leafMatInstance.SetColor("_GroundColor", Color.Lerp(fromGround, toGround, t));
            }

            yield return null;
        }
    }


    // WATERING
    public void TriggerGrowth()
    {
        if (isDead || treeData == null) return;

        if (currentHealth < maxHealth * treeData.waterNeedThreshold)
        {
            if (waterEffect != null && !waterEffect.isPlaying)
                waterEffect.Play();

            recentlyWatered = true;
            currentHealth = Mathf.Clamp(currentHealth, 0f, maxHealth);
        }
    }


    // DEATH
    private void HandleTreeDeath()
    {
        isDead = true;

        if (colorTransitionRoutine != null)
            StopCoroutine(colorTransitionRoutine);

        GetComponent<FruitSpawner>()?.StopAllCoroutines();

        // Hapus visual daun & buah
        foreach (Transform child in transform)
            Destroy(child.gameObject);

        if (treeData.deadTrunkPrefab != null)
            Instantiate(treeData.deadTrunkPrefab, transform.position, transform.rotation, transform);
    }
}