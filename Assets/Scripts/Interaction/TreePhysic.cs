using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class TreePhysic : MonoBehaviour, IInteractable
{
    private GrowTree growTree;
    private FruitSpawner fruitSpawner;

    [Header("Leaf Shake Effect")]
    [SerializeField] private ParticleSystem leafEffect;

    [Header("Shake Settings")]
    public float shakeStrength = 5f;
    public float dropDelay = 0.5f;

    [Header("Wind Settings")]
    public float windStrength = 1f;
    public float windSpeed = 0.5f;
    public float windDropInterval = 10f;
    public int windDropCount = 1;

    private bool isShaking = false;
    private bool playerNear = false;
    private bool tutorialShakeInvoked = false;

    private Quaternion originalRot;

    // ===== IInteractable =====
    public InputType InputKey => InputType.E;
    public string PromptMessage
    {
        get
        {
            if (!growTree) return "";

            // 🔥 fisik hanya hidup jika owner = Physic
            if (growTree.GetActionOwner() != TreeActionOwner.Physic)
                return "";

            if (isShaking)
                return "Tree is shaking...";

            return "[E] Shake Tree";
        }
    }

    void Awake()
    {
        growTree = GetComponent<GrowTree>();
        fruitSpawner = GetComponent<FruitSpawner>();
        AutoFindLeafEffect();
    }

    void Start()
    {
        originalRot = transform.rotation;
        StartCoroutine(WindRoutine());

        if (leafEffect != null)
            leafEffect.Stop();
    }

    void Update()
    {
        if (growTree == null) return;

        // 🔥 kalau bukan physic owner → jangan bisa interact
        if (growTree.GetActionOwner() != TreeActionOwner.Physic)
            return;

        // 🌬️ Wind sway
        float sway = Mathf.Sin(Time.time * windSpeed) * windStrength;
        transform.rotation = originalRot * Quaternion.Euler(0, 0, sway);
    }

    public void Interact()
    {
        if (!playerNear) return;
        if (growTree == null) return;
        if (!growTree.IsFullyGrown) return;
        if (growTree.IsTreeDead()) return;
        if (isShaking) return;

        StartCoroutine(ShakeTree());
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        playerNear = true;
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        playerNear = false;
    }

    IEnumerator ShakeTree()
    {
        if (leafEffect == null) yield break;

        isShaking = true;

        PromptManager.Instance.RefreshContext(this, PromptMessage);

        if (!tutorialShakeInvoked &&
            TutorialManager.Instance != null &&
            TutorialManager.Instance.currentStep == TutorialStep.TreeShake)
        {
            tutorialShakeInvoked = true;
            TutorialEvents.OnTreeShake?.Invoke();
        }

        leafEffect.Play();

        if (growTree.IsFullyGrown)
            StartCoroutine(DropFruitsOneByOne(10));

        float t = 0f;

        while (playerNear && !growTree.IsTreeDead())
        {
            t += Time.deltaTime;
            float strength = Mathf.Sin(t * 20f) * shakeStrength;
            transform.rotation = originalRot * Quaternion.Euler(0, 0, strength);
            yield return null;
        }

        transform.rotation = originalRot;
        leafEffect.Stop();

        isShaking = false;

        // ⬅️ refresh balik
        if (playerNear)
            PromptManager.Instance.RefreshContext(this, PromptMessage);
        else
            PromptManager.Instance.HideContext(this);
    }

    IEnumerator DropFruitsOneByOne(int count)
    {
        if (fruitSpawner == null) yield break;

        List<GameObject> fruits = fruitSpawner.GetActiveFruits();
        int drop = Mathf.Min(count, fruits.Count);

        for (int i = 0; i < drop; i++)
        {
            if (fruits[i] != null)
            {
                fruits[i].transform.parent = null;

                var rb = fruits[i].GetComponent<Rigidbody>();
                if (rb)
                {
                    rb.isKinematic = false;
                    rb.useGravity = true;
                    rb.AddForce(Vector3.down * 2f, ForceMode.Impulse);
                }
            }
            yield return new WaitForSeconds(dropDelay);
        }
    }

    IEnumerator WindRoutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(windDropInterval);

            if (growTree == null) continue;
            if (!growTree.IsFullyGrown) continue;
            if (growTree.IsTreeDead()) continue;

            StartCoroutine(DropFruitsOneByOne(windDropCount));
        }
    }

    private void AutoFindLeafEffect()
    {
        if (leafEffect != null) return;

        foreach (var ps in GetComponentsInChildren<ParticleSystem>(true))
        {
            if (ps.name.ToLower().Contains("leaf"))
            {
                leafEffect = ps;
                break;
            }
        }
    }

    void OnEnable()
    {
        GrowTree.OnTreeFullyGrown += HandleTreeFullyGrown;
        GrowTree.OnActionOwnerChanged += HandleOwnerChanged;
    }

    void OnDisable()
    {
        GrowTree.OnTreeFullyGrown -= HandleTreeFullyGrown;
        GrowTree.OnActionOwnerChanged -= HandleOwnerChanged;
    }

    void HandleTreeFullyGrown(GrowTree tree)
    {
        if (tree != growTree) return;
        if (!playerNear) return;

        PromptManager.Instance.RefreshContext(this, PromptMessage);
    }

    void HandleOwnerChanged(GrowTree tree)
    {
        if (tree != growTree) return;

        // 🔥 kalau bukan physic → HILANGKAN PROMPT SHAKE
        if (growTree.GetActionOwner() != TreeActionOwner.Physic)
        {
            PromptManager.Instance.HideContext(this);
            return;
        }

        // balik ke shake (kalau player masih dekat)
        if (playerNear)
            PromptManager.Instance.RefreshContext(this, PromptMessage);
    }
}
