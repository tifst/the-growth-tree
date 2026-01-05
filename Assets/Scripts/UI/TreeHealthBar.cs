using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class TreeHealthBar : MonoBehaviour
{
    public static TreeHealthBar Instance;

    [Header("Referensi")]
    [SerializeField] private Camera mainCamera;
    [SerializeField] private Slider healthBarPrefab;

    [Header("Smooth Settings")]
    public float smoothSpeed = 6f;

    private Dictionary<GrowTree, Slider> activeBars = new();
    private Dictionary<GrowTree, float> visualValues = new();
    private Dictionary<GrowTree, bool> forceInstant = new();

    void Awake()
    {
        Instance = this;
        if (mainCamera == null) mainCamera = Camera.main;
    }

    void LateUpdate()
    {
        List<GrowTree> toRemove = new();

        foreach (var pair in activeBars)
        {
            GrowTree tree = pair.Key;
            Slider bar = pair.Value;

            if (tree == null || bar == null)
            {
                toRemove.Add(tree);
                continue;
            }

            // 🔥 POHON MATI → HILANGKAN BAR
            if (tree.IsTreeDead())
            {
                bar.gameObject.SetActive(false);
                toRemove.Add(tree);
                continue;
            }

            // World → Screen
            Vector3 screenPos =
                mainCamera.WorldToScreenPoint(tree.transform.position + Vector3.up * 2f);

            if (screenPos.z <= 0)
            {
                bar.gameObject.SetActive(false);
                continue;
            }

            bar.gameObject.SetActive(true);
            bar.transform.position = screenPos;

            // ================= SMOOTH VALUE =================
            float target = tree.CurrentHealthPercent;

            // pertama kali / masuk area → langsung set
            if (forceInstant.TryGetValue(tree, out bool instant) && instant)
            {
                visualValues[tree] = target;
                bar.value = target;
                forceInstant[tree] = false; // selesai
            }
            else
            {
                // hanya heal / decay yang smooth
                visualValues[tree] = Mathf.Lerp(
                    visualValues[tree],
                    target,
                    Time.deltaTime * smoothSpeed
                );

                bar.value = visualValues[tree];
            }

            // ================= COLOR =================
            Image fill = bar.fillRect.GetComponent<Image>();
            if (fill != null)
            {
                Color healthy = Color.green;
                Color dry = Color.red;
                fill.color = Color.Lerp(dry, healthy, bar.value);
            }
        }

        // Cleanup
        foreach (var dead in toRemove)
        {
            if (activeBars.ContainsKey(dead))
            {
                Destroy(activeBars[dead].gameObject);
                activeBars.Remove(dead);
                visualValues.Remove(dead);
                forceInstant.Remove(dead);
            }
        }
    }

    // ================= PUBLIC API =================
    public void ShowTreeBar(GrowTree tree)
    {
        if (tree == null) return;
        if (activeBars.ContainsKey(tree)) return;

        Slider newBar = Instantiate(healthBarPrefab, transform);
        newBar.transform.SetAsFirstSibling();
        newBar.gameObject.SetActive(true);

        float value = tree.CurrentHealthPercent;

        newBar.value = value;

        activeBars.Add(tree, newBar);
        visualValues.Add(tree, value);

        // ⛔ jangan animasi dulu
        forceInstant[tree] = true;
    }

    public void HideTreeBar(GrowTree tree)
    {
        if (tree == null) return;

        if (activeBars.ContainsKey(tree))
        {
            Destroy(activeBars[tree].gameObject);
            activeBars.Remove(tree);
            visualValues.Remove(tree);
        }
    }
}