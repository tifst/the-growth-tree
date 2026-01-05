using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Reflection;

public class PopupQuest : MonoBehaviour
{
    public static PopupQuest Instance;
    private QuestData currentQuest;
    private MonoBehaviour contextOwner;
    private bool isOpen = false;

    [Header("Setup")]
    public Camera mainCamera;
    public GameObject popupPrefab; 
    private RectTransform popupRect;

    private GameObject popupInstance;
    private TMP_Text popupText;
    private Image popupIcon;
    private Image popupTime;

    private TMP_Text countText;
    private TextWriter writer;
    public float writeSpeed;

    private Transform target;
    public float heightOffset = 1.5f;
    public float widthOffset = 50f;

    void Awake()
    {
        Instance = this;
        if (mainCamera == null)
            mainCamera = Camera.main;
    }

    void Update()
    {
        if (!isOpen) return;
        if (QuestManager.Instance.HasQuest(currentQuest)) return;
        
        if (Input.GetKeyDown(KeyCode.Return))
        {
            AcceptQuest();
        }
    }

    void LateUpdate()
    {
        if (popupRect == null || target == null) return;

        Vector3 worldPos = target.position + Vector3.up * heightOffset;
        Vector3 screenPos = mainCamera.WorldToScreenPoint(worldPos);

        if (screenPos.z < 0)
        {
            popupRect.gameObject.SetActive(false);
            return;
        }

        popupRect.gameObject.SetActive(true);

        RectTransform canvasRect =
            popupRect.root.GetComponent<Canvas>().GetComponent<RectTransform>();

        Vector2 localPos;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvasRect,
            screenPos,
            null, // Screen Space Overlay → null
            out localPos
        );

        localPos.x += widthOffset;

        popupRect.anchoredPosition = localPos;
    }

    public void Show(QuestData data, Transform npcPoint, MonoBehaviour owner)
    {
        Hide();

        contextOwner = owner;
        currentQuest = data;
        isOpen = true;

        popupInstance = Instantiate(popupPrefab, transform);
        popupInstance.transform.SetSiblingIndex(1);

        popupRect = popupInstance.GetComponent<RectTransform>();
        popupText = popupInstance.transform.Find("Text").GetComponent<TMP_Text>();
        popupIcon = popupInstance.transform.Find("Icon").GetComponent<Image>();
        popupTime = popupInstance.transform.Find("Icon/Time").GetComponent<Image>();
        countText = popupInstance.transform.Find("Icon/CountText").GetComponent<TMP_Text>();
        writer = popupText.GetComponent<TextWriter>();

        // ===== TIME UI =====
        if (data.hasTimer) popupTime.gameObject.SetActive(true);
        else popupTime.gameObject.SetActive(false);

        popupIcon.sprite = data.questIcon;

        int amount = data.requiredAmount;
        countText.text = amount > 0 ? amount.ToString() : "";

        popupText.text = "";
        writer.Write(data.description, writeSpeed);

        target = npcPoint;
        popupInstance.SetActive(true);
    }

    void AcceptQuest()
    {
        if (currentQuest == null) return;

        if (!QuestManager.Instance.HasQuest(currentQuest))
        {
            // 🔥 tutorial & quest FINAL DI SINI
            TutorialEvents.OnInteractNPC?.Invoke(currentQuest.questID);
            QuestManager.Instance.StartQuest(currentQuest);
            GuideManager.Instance.RemoveTarget(transform);

            PromptManager.Instance.Notify(
                "New quest taken: " + currentQuest.questTitle
            );

            Hide();
        }
    }

    public void Hide()
    {
        if (popupInstance != null)
            Destroy(popupInstance);

        popupInstance = null;
        target = null;
        currentQuest = null;
        isOpen = false;

        if (contextOwner != null)
            PromptManager.Instance.HideContext(contextOwner);

        contextOwner = null;
    }
}