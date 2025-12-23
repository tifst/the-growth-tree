using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class TreeHealthBar : MonoBehaviour
{
    public static TreeHealthBar Instance;

    [Header("Referensi")]
    [SerializeField] private Camera mainCamera;
    [SerializeField] private Slider healthBarPrefab;

    private Dictionary<GrowTree, Slider> activeBars = new Dictionary<GrowTree, Slider>();

    void Awake()
    {
        Instance = this;
        if (mainCamera == null) mainCamera = Camera.main;
    }

    void LateUpdate()
    {
        // Update posisi dan nilai semua health bar aktif
        List<GrowTree> toRemove = new List<GrowTree>();

        foreach (var pair in activeBars)
        {
            GrowTree tree = pair.Key;
            Slider bar = pair.Value;

            if (tree == null || bar == null)
            {
                toRemove.Add(tree);
                continue;
            }

            // sembunyikan kalau sudah layu atau tumbuh penuh
            if (tree.IsFullyGrown && tree.CurrentHealthPercent >= 1f)
            {
                bar.gameObject.SetActive(false);
                continue;
            }

            // posisi layar (offset sedikit di atas pohon)
            Vector3 screenPos = mainCamera.WorldToScreenPoint(tree.transform.position + Vector3.up * 2f);

            // tampilkan hanya kalau masih di depan kamera
            if (screenPos.z > 0)
            {
                bar.transform.position = screenPos;
                bar.value = tree.CurrentHealthPercent;

                // --- NEW: update color berdasarkan health ---
                Image fill = bar.fillRect.GetComponent<Image>();
                if (fill != null)
                {
                    Color healthy = Color.green;
                    Color dry = Color.red;
                    fill.color = Color.Lerp(dry, healthy, tree.CurrentHealthPercent);
                }

                bar.gameObject.SetActive(true);
            }
            else
            {
                bar.gameObject.SetActive(false);
            }
        }

        // bersihkan bar yang sudah tidak valid
        foreach (var dead in toRemove)
        {
            if (activeBars.ContainsKey(dead))
            {
                Destroy(activeBars[dead].gameObject);
                activeBars.Remove(dead);
            }
        }
    }

    public void ShowTreeBar(GrowTree tree)
    {
        if (tree == null) return;
        if (activeBars.ContainsKey(tree)) return; // sudah ada

        Slider newBar = Instantiate(healthBarPrefab, transform);
        newBar.gameObject.SetActive(true);
        newBar.value = tree.CurrentHealthPercent;
        activeBars.Add(tree, newBar);
    }

    public void HideTreeBar(GrowTree tree)
    {
        if (tree == null) return;

        if (activeBars.ContainsKey(tree))
        {
            Destroy(activeBars[tree].gameObject);
            activeBars.Remove(tree);
        }
    }
}