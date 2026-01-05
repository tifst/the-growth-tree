using UnityEngine;
using System.Collections;
using System;
using System.Linq.Expressions;

public enum TreeActionOwner
{
    None,
    Plot,
    Physic
}

public class GrowTree : MonoBehaviour
{
    public static event Action<GrowTree> OnTreeFullyGrown;
    public static event Action<GrowTree> OnActionOwnerChanged;
    TreeActionOwner lastOwner;
    public PlantPlot parentPlot;

    [Header("Scriptable Data")]
    [SerializeField] public TreeData treeData;

    [Header("Effects")]
    [SerializeField] private ParticleSystem waterEffect;

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
    private bool isLoadedFromSave = false;
    private bool playerNearby = false;
    private bool promptShown;
    private Vector3 minScale;

    public bool IsFullyGrown => isFullyGrown;
    public float CurrentHealthPercent => Mathf.Clamp01(currentHealth / maxHealth);
    public bool IsTreeDead() => isDead;

    public TreeActionOwner GetActionOwner()
    {
        if (IsTreeDead())
            return TreeActionOwner.Plot;

        if (CanBeWatered())
            return TreeActionOwner.Plot;

        if (IsFullyGrown)
            return TreeActionOwner.Physic;

        return TreeActionOwner.None;
    }

    void Start()
    {
        if (treeData == null)
        {
            enabled = false;
            return;
        }

        AutoFindEffects();
        minScale = treeData.maxScale * 0.1f;

        if (!isLoadedFromSave)
        {
            transform.localScale = minScale;
            currentHealth = treeData.startHealth;
        }

        DetectLeafRenderer();

        if (waterEffect != null)
            waterEffect.Stop();
    }

    void Update()
    {
        if (treeData == null || isDead) return;

        var owner = GetActionOwner();
        if (owner != lastOwner)
        {
            lastOwner = owner;
            OnActionOwnerChanged?.Invoke(this);
        }

        HandleHealthDecay();
        UpdateVisualByHealth();
        HandleGrowth();

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
        if (isFullyGrown)
        {
            if (!isLoadedFromSave)
                transform.localScale = treeData.maxScale;
            return;
        }

        float speedMultiplier = CurrentHealthPercent >= 0.4f ? 1f : 0.5f;

        growTimer += Time.deltaTime * speedMultiplier;

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

        OnTreeFullyGrown?.Invoke(this);
        TutorialEvents.OnTreeGrow?.Invoke();

        PlayGrowEffect();
        StartCoroutine(StopGrowEffectAfter(2f));

        GameManager.Instance?.AddXP(treeData.xpRewardPlant);
        GameManager.Instance?.ModifyPollution(-treeData.pollutionReduction);
        QuestManager.Instance?.AddProgress(treeData.treeName, QuestGoalType.PlantTree);

        PromptManager.Instance.Notify($"{treeData.treeName} tree has fully grown!", 3f);
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
        else if (isWithered && hp > 0.4f)
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

    // DEATH
    private void HandleTreeDeath()
    {
        isDead = true;

        if (colorTransitionRoutine != null)
            StopCoroutine(colorTransitionRoutine);

        TreePhysic tp = GetComponent<TreePhysic>();
        if (tp != null)
            tp.StopAllCoroutines();

        GetComponent<FruitSpawner>()?.StopAllCoroutines();

        // Hapus visual daun & buah
        foreach (Transform child in transform)
            Destroy(child.gameObject);

        if (treeData.deadTrunkPrefab != null)
            Instantiate(treeData.deadTrunkPrefab, transform.position, transform.rotation, transform);
        
        GameManager.Instance?.ModifyPollution(Mathf.RoundToInt(0.2f * treeData.pollutionReduction));
        OnActionOwnerChanged?.Invoke(this);
    }

    public bool CanBeWatered()
    {
        return !isDead && CurrentHealthPercent < treeData.waterNeedThreshold;
    }

    public float GetMissingHealth()
    {
        return maxHealth - currentHealth;
    }

    public void HealToFull()
    {
        if (isDead) return;

        currentHealth = maxHealth;

        // FORCE VISUAL BACK TO HEALTHY
        if (isWithered)
        {
            isWithered = false;
            StartLeafTransition(
                treeData.dryLeafMaterial,
                treeData.healthyLeafMaterial,
                3f
            );
        }

        recentlyWatered = true;
    }

    public void OnHealed()
    {
        recentlyWatered = true;
    }

    public TreeSaveInfo ExportState()
    {
        return new TreeSaveInfo
        {
            treeID = treeData.treeName,
            plotID = parentPlot != null ? parentPlot.plotID : "",

            posX = transform.position.x,
            posY = transform.position.y,
            posZ = transform.position.z,

            scaleX = transform.localScale.x,
            scaleY = transform.localScale.y,
            scaleZ = transform.localScale.z,

            health = currentHealth,
            growTimer = growTimer,
            isFullyGrown = isFullyGrown,
            isDead = isDead,
            isWithered = isWithered
        };
    }

    public void ImportState(TreeSaveInfo info)
    {
        if (info == null) return;

        isLoadedFromSave = true;

        currentHealth = info.health;
        growTimer = info.growTimer;
        isFullyGrown = info.isFullyGrown;
        wasFullyGrown = info.isFullyGrown;
        isDead = info.isDead;
        isWithered = info.isWithered;

        transform.position = new Vector3(info.posX, info.posY, info.posZ);
        transform.localScale = new Vector3(info.scaleX, info.scaleY, info.scaleZ);

        UpdateVisualByHealth();
        LateInitialize();

        if (isDead)
            HandleTreeDeath();
    }

    void LateInitialize()
    {
        // cari plot terdekat (karena tree adalah child plot)
        parentPlot = GetComponentInParent<PlantPlot>();

        if (parentPlot != null)
        {
            parentPlot.growTree = this;
            parentPlot.SetPlantedTree(gameObject);
        }
    }

    public void PlayWaterEffect()
    {
        if (waterEffect == null) return;

        // arah ke bawah (default)
        waterEffect.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);

        if (!waterEffect.isPlaying)
            waterEffect.Play();
    }

    void PlayGrowEffect()
    {
        if (waterEffect == null) return;

        // 🔥 balik arah ke atas
        waterEffect.transform.localRotation = Quaternion.Euler(-90f, 0f, 0f);
        waterEffect.transform.localPosition = new Vector3(0, 0, 0);

        waterEffect.Play();
    }

    public IEnumerator StopGrowEffectAfter(float sec)
    {
        yield return new WaitForSeconds(sec);
        if (waterEffect != null)
            waterEffect.Stop();
    }
}