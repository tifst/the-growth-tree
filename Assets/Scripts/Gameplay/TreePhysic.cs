using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class TreePhysic : MonoBehaviour
{
    private GrowTree growTree;
    private FruitSpawner fruitSpawner;

    [Header("Leaf Shake Effect (Auto-detect)")]
    [SerializeField] private ParticleSystem leafEffect;

    [Header("Shake Settings")]
    public float shakeDuration = 1.5f;
    public float shakeStrength = 5f;
    public float dropDelay = 0.5f;

    [Header("Wind Settings")]
    public float windStrength = 1f;
    public float windSpeed = 0.5f;
    public float windDropInterval = 10f;
    public int windDropCount = 1;

    private bool isShaking = false;
    private bool playerNear = false;
    private Quaternion originalRot;

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
        // 🌬️ Wind sway
        float sway = Mathf.Sin(Time.time * windSpeed) * windStrength;
        transform.rotation = originalRot * Quaternion.Euler(0, 0, sway);

        if (growTree == null || growTree.IsTreeDead()) return;

        // 🌳 Shake only when fully grown & player is near
        if (playerNear && growTree.IsFullyGrown && !isShaking)
            StartCoroutine(ShakeTree());
    }

    private void AutoFindLeafEffect()
    {
        if (leafEffect != null) return;

        var allPS = GetComponentsInChildren<ParticleSystem>(true);
        foreach (var ps in allPS)
        {
            if (ps.name.ToLower().Contains("leaf"))
            {
                leafEffect = ps;
                break;
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
            playerNear = true;
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
            playerNear = false;
    }

    IEnumerator ShakeTree()
    {
        if (growTree == null || growTree.IsTreeDead()) yield break;
        if (leafEffect == null) yield break;

        isShaking = true;
        PromptUI.Instance.Show("Pohon sedang bergoyang!", this);

        leafEffect?.Play();

        // Buah jatuh ketika pohon dewasa
        if (growTree.IsFullyGrown)
            StartCoroutine(DropFruitsOneByOne(3));

        float t = 0f;
        while (t < shakeDuration)
        {
            t += Time.deltaTime;
            float strength = Mathf.Sin(t * 20f) * (shakeStrength * (1f - (t / shakeDuration)));
            transform.rotation = originalRot * Quaternion.Euler(0, 0, strength);
            yield return null;
        }

        transform.rotation = originalRot;
        leafEffect?.Stop();
        PromptUI.Instance.Hide(this);
        isShaking = false;
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
            StartCoroutine(DropFruitsOneByOne(windDropCount));
        }
    }
}