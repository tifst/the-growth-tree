using UnityEngine;

[RequireComponent(typeof(BoxCollider))]
public class PlotHighlight : MonoBehaviour
{
    [Header("Pengaturan Garis")]
    [SerializeField] private Material lineMaterial;
    [SerializeField] private Color lineColor = Color.green;
    [SerializeField] private float lineWidth = 0.05f;
    [SerializeField] private float heightOffset = 0.01f;

    [Header("Efek Animasi")]
    [SerializeField] private bool enablePulse = true;
    [SerializeField] private float pulseSpeed = 2f;
    [SerializeField, Range(0f, 1f)] private float minAlpha = 0.3f;

    [Header("Offset Posisi (jika pivot bukan di tengah)")]
    [SerializeField] private Vector3 positionOffset = new Vector3(2.5f, 0f, -2.5f);

    private LineRenderer[] edges = new LineRenderer[4];
    private BoxCollider box;
    private bool isVisible = false;
    private float pulseTimer = 0f;

    void Start()
    {
        // cari collider utama yang bukan trigger
        BoxCollider[] colliders = GetComponents<BoxCollider>();
        foreach (var col in colliders)
        {
            if (!col.isTrigger)
            {
                box = col;
                break;
            }
        }

        // fallback kalau tidak ada collider non-trigger
        if (box == null) box = GetComponent<BoxCollider>();

        CreateEdges();
        SetEdgesVisible(false);
    }

    void Update()
    {
        if (isVisible && enablePulse)
        {
            pulseTimer += Time.deltaTime * pulseSpeed;
            float alpha = Mathf.Lerp(minAlpha, 1f, (Mathf.Sin(pulseTimer) + 1f) * 0.5f);

            foreach (var lr in edges)
            {
                if (lr != null)
                {
                    Color c = lineColor;
                    c.a = alpha;
                    lr.startColor = lr.endColor = c;
                }
            }
        }
    }

    void CreateEdges()
    {
        Vector3 size = box.size;
        Vector3 center = box.center + positionOffset;
        float halfX = size.x / 2f;
        float halfZ = size.z / 2f;
        float y = transform.position.y + center.y + size.y / 2f + heightOffset;

        Vector3[] corners = new Vector3[]
        {
            transform.TransformPoint(new Vector3(-halfX, y - transform.position.y, -halfZ) + positionOffset),
            transform.TransformPoint(new Vector3(-halfX, y - transform.position.y,  halfZ) + positionOffset),
            transform.TransformPoint(new Vector3( halfX, y - transform.position.y,  halfZ) + positionOffset),
            transform.TransformPoint(new Vector3( halfX, y - transform.position.y, -halfZ) + positionOffset)
        };

        for (int i = 0; i < 4; i++)
        {
            GameObject lineObj = new GameObject($"Edge_{i}");
            lineObj.transform.SetParent(transform);
            var lr = lineObj.AddComponent<LineRenderer>();

            lr.material = lineMaterial;
            lr.startColor = lr.endColor = lineColor;
            lr.startWidth = lr.endWidth = lineWidth;
            lr.useWorldSpace = true;
            lr.positionCount = 2;
            lr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            lr.receiveShadows = false;
            lr.motionVectorGenerationMode = MotionVectorGenerationMode.ForceNoMotion;

            lr.SetPosition(0, corners[i]);
            lr.SetPosition(1, corners[(i + 1) % 4]);

            edges[i] = lr;
        }
    }

    public void SetEdgesVisible(bool visible)
    {
        isVisible = visible;
        foreach (var lr in edges)
        {
            if (lr != null) lr.enabled = visible;
        }
    }
}