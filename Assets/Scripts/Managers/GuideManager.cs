using UnityEngine;
using TMPro;
using System.Collections.Generic;

public class GuideManager : MonoBehaviour
{
    public static GuideManager Instance;

    [Header("References")]
    public Camera mainCamera;
    public RectTransform arrowPrefab;

    [Header("Arrow Position")]
    public float sidePadding = 50f;
    public float minVerticalOffset = 40f;
    public float maxVerticalOffset = 140f;
    public float maxDistance = 15f;

    [Header("Arrow Scale")]
    public Vector3 minDistanceScale = new Vector3(1.4f, 1.4f, 1f);
    public Vector3 maxDistanceScale = new Vector3(0.7f, 0.7f, 1f);
    public float scaleLerpSpeed = 8f;

    [Header("Arrow Animation")]
    public float floatAmplitude = 10f;
    public float floatSpeed = 2f;

    [Header("Narrative UI")]
    public GameObject narrativePanel;
    public TMP_Text narrativeText;
    public float writeSpeed = 0.03f;

    private TextWriter writer;

    // ===== INTERNAL =====
    private Transform tutorialTarget;
    private Dictionary<Transform, RectTransform> arrows = new();

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        ClearAllTargets(); // 🔥 reset state

        narrativePanel?.SetActive(false);
    }

    void Update()
    {
        if (arrows.Count == 0) return;

        var toRemove = new List<Transform>();

        foreach (var pair in arrows)
        {
            if (pair.Key == null)
            {
                if (pair.Value != null)
                    Destroy(pair.Value.gameObject);

                toRemove.Add(pair.Key);
                continue;
            }

            UpdateArrow(pair.Key, pair.Value);
        }

        foreach (var t in toRemove)
            arrows.Remove(t);
    }

    // ================= TUTORIAL MODE =====================
    public void PointTo(Transform target)
    {
        ClearTutorialGuide();

        if (!target) return;

        tutorialTarget = target;
        AddTarget(target);
    }

    public void ClearTutorialGuide()
    {
        if (tutorialTarget != null)
            RemoveTarget(tutorialTarget);

        tutorialTarget = null;
    }

    public void EnsureTutorialArrowActive()
    {
        if (tutorialTarget == null) return;

        if (!arrows.ContainsKey(tutorialTarget))
            AddTarget(tutorialTarget);
    }

    // ================= GAMEPLAY MODE =====================
    public void AddTarget(Transform target)
    {
        if (!target || arrows.ContainsKey(target)) return;

        RectTransform arrow = Instantiate(arrowPrefab, transform);
        arrows.Add(target, arrow);
    }

    public void RemoveTarget(Transform target)
    {
        if (!target) return;

        if (arrows.TryGetValue(target, out var arrow))
        {
            Destroy(arrow.gameObject);
            arrows.Remove(target);
        }
    }

    public void ClearAllTargets()
    {
        foreach (var a in arrows.Values)
        {
            if (a != null)
                Destroy(a.gameObject);
        }

        arrows.Clear();
        tutorialTarget = null;
    }

    // ================= ARROW LOGIC =======================
    void UpdateArrow(Transform target, RectTransform arrow)
    {
        Vector3 worldPos = target.position;
        Vector3 screenPos = mainCamera.WorldToScreenPoint(worldPos);
        Vector3 vp = mainCamera.WorldToViewportPoint(worldPos);

        bool behindCamera = screenPos.z < 0;
        bool onScreen =
            !behindCamera &&
            vp.x > 0f && vp.x < 1f &&
            vp.y > 0f && vp.y < 1f;

        float floatY = Mathf.Sin(Time.time * floatSpeed) * floatAmplitude;

        float dist = Vector3.Distance(mainCamera.transform.position, worldPos);
        float t = Mathf.Clamp01(dist / maxDistance);
        Vector3 targetScale = Vector3.Lerp(minDistanceScale, maxDistanceScale, t);
        float dynamicOffset = Mathf.Lerp(maxVerticalOffset, minVerticalOffset, t);

        if (onScreen)
        {
            Vector3 pos = screenPos;
            pos.y += dynamicOffset + floatY;

            arrow.position = pos;
            arrow.rotation = Quaternion.Euler(0, 0, 180f);
        }
        else
        {
            if (vp.z < 0)
            {
                vp.x = 1f - vp.x;
                vp.y = 1f - vp.y;
            }

            Vector2 fromCenter = new Vector2(vp.x - 0.5f, vp.y - 0.5f);
            bool horizontal = Mathf.Abs(fromCenter.x) > Mathf.Abs(fromCenter.y);

            float x, y, rotZ;

            if (horizontal)
            {
                bool isRight = fromCenter.x > 0f;
                x = isRight ? Screen.width - sidePadding : sidePadding;
                y = Mathf.Clamp(screenPos.y, sidePadding, Screen.height - sidePadding);
                rotZ = isRight ? -90f : 90f;
            }
            else
            {
                bool isTop = fromCenter.y > 0f;
                x = Mathf.Clamp(screenPos.x, sidePadding, Screen.width - sidePadding);
                y = isTop ? Screen.height - sidePadding : sidePadding;
                rotZ = isTop ? 0f : 180f;
            }

            arrow.position = new Vector3(x, y + floatY, 0f);
            arrow.rotation = Quaternion.Euler(0, 0, rotZ);
        }

        arrow.localScale = Vector3.Lerp(
            arrow.localScale,
            targetScale,
            Time.deltaTime * scaleLerpSpeed
        );
    }

    // ================= NARRATIVE =========================
    public void ShowNarrative(string text)
    {
        if (!narrativePanel) return;

        narrativePanel.SetActive(true);

        if (!writer)
            writer = narrativeText.GetComponent<TextWriter>();

        narrativeText.text = "";
        writer.Write(text, writeSpeed);
    }

    public void HideNarrative()
    {
        if (narrativePanel)
            narrativePanel.SetActive(false);
    }

    public void ResetAll()
    {
        ClearAllTargets();
        HideNarrative();
    }
}