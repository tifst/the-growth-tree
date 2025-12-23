using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PopupQuest : MonoBehaviour
{
    public static PopupQuest Instance;

    [Header("Setup")]
    public Camera mainCamera;
    public GameObject popupPrefab; 

    private GameObject popupInstance;
    private TMP_Text popupText;
    private Image popupIcon;
    private TextWriter writer;

    private Transform target;
    public float heightOffset = 1.5f;
    public float widthOffset = 50f;

    void Awake()
    {
        Instance = this;
        if (mainCamera == null)
            mainCamera = Camera.main;
    }

    void LateUpdate()
    {
        if (popupInstance == null || target == null) return;

        Vector3 worldPos = target.position + Vector3.up * heightOffset;
        Vector3 screenPos = mainCamera.WorldToScreenPoint(worldPos);
        screenPos.x += widthOffset;

        if (screenPos.z < 0)
        {
            popupInstance.SetActive(false);
            return;
        }

        popupInstance.transform.position = screenPos;
    }

    public void Show(QuestData data, Transform npcHead)
    {
        Hide(); // bersihkan popup sebelumnya

        popupInstance = Instantiate(popupPrefab, transform);

        popupText = popupInstance.transform.Find("Text").GetComponent<TMP_Text>();
        popupIcon = popupInstance.transform.Find("Icon").GetComponent<Image>();
        writer = popupText.GetComponent<TextWriter>();

        popupIcon.sprite = data.questIcon;

        popupText.text = "";
        writer.Write(data.description, 0.02f);

        target = npcHead;

        popupInstance.SetActive(true);
    }

    public void Hide()
    {
        if (popupInstance != null)
            Destroy(popupInstance);

        popupInstance = null;
        target = null;
    }
}
