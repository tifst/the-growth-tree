using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class QuestItem : MonoBehaviour
{
    [Header("UI ELEMENTS")]
    public TMP_Text titleText;
    public TMP_Text shortDescText;
    public TMP_Text durationText;
    public TMP_Text rewardText;

    public Image iconImage;
    public TMP_Text countText;

    public Image progressFill;
    public TMP_Text progressText;

    [Header("Claim Button")]
    public Button claimButton;
    public TMP_Text claimButtonText;     // 🔥 assign langsung
    public Image claimButtonImage;        // 🔥 assign langsung

    [Header("Panel")]
    public Image panelBackground;         // 🔥 background panel

    private QuestData data;
    private int progress;

    // ================= STATE =================
    private bool claimed;
    private bool completed;
    private bool failed;

    public bool isClaimed => claimed;
    public bool isCompleted => completed;
    public bool isFailed => failed;

    // ================= SETUP =================
    public void Setup(QuestData q, bool isRestore = false)
    {
        data = q;

        claimed = false;
        completed = false;
        failed = false;

        titleText.text = $"{q.questTitle} ({q.difficulty})";
        shortDescText.text = q.shortDescription;
        iconImage.sprite = q.questIcon;
        countText.text = q.requiredAmount.ToString();

        rewardText.text = $"+{q.rewardCoins} Coins   +{q.rewardXP} XP";
        rewardText.gameObject.SetActive(true);

        durationText.text = q.hasTimer ? $"{q.duration:F0}s" : "";
        durationText.color = Color.black;

        // 🔥 RESET HANYA JIKA BUKAN RESTORE
        if (!isRestore)
        {
            progress = 0;
            progressText.text = $"0/{q.requiredAmount}";
            progressFill.fillAmount = 0f;
        }

        progressFill.gameObject.SetActive(true);

        ResetPanelVisual();

        claimButton.gameObject.SetActive(true);
        SetClaimButton(false);

        claimButton.onClick.RemoveAllListeners();
        claimButton.onClick.AddListener(() =>
        {
            if (!claimed && completed)
                QuestManager.Instance.ClaimQuest(q);
        });
    }

    // ================= PROGRESS =================
    public void UpdateProgress(int now, int max)
    {
        if (completed || failed) return;

        progress = now;
        progressText.text = $"{now}/{max}";
        progressFill.fillAmount = Mathf.Clamp01((float)now / max);
    }

    // ================= STATE CHANGE =================
    // 🔵 COMPLETED (BELUM CLAIM)
    public void MarkCompleted()
    {
        if (claimed || failed) return;

        completed = true;

        progress = data.requiredAmount;
        progressText.text = $"{data.requiredAmount}/{data.requiredAmount}";
        progressFill.fillAmount = 1f;

        durationText.text = "Completed";
        durationText.color = Color.blue;

        SetClaimButton(true);
    }

    // 🟢 CLAIMED
    public void MarkClaimed()
    {
        if (!completed || claimed) return;

        claimed = true;

        FadePanel();
        rewardText.gameObject.SetActive(false);

        progressFill.color = Color.green;
        progressText.color = Color.black;

        claimButton.interactable = false;
        claimButtonText.text = "Claimed";

        if (claimButtonImage != null)
            claimButtonImage.enabled = false;
    }

    // 🔴 FAILED
    public void MarkFailed()
    {
        if (completed || claimed) return;

        failed = true;

        FadePanel();
        rewardText.gameObject.SetActive(false);

        durationText.text = "Failed";
        durationText.color = Color.red;

        progressFill.color = Color.gray;
        progressText.color = Color.red;

        claimButton.gameObject.SetActive(false);
    }

    // ================= UI HELPERS =================
    void SetClaimButton(bool enabled)
    {
        claimButton.interactable = enabled;
        claimButtonText.text = "Claim";

        // Fade visual
        float alpha = enabled ? 1f : 0.7f;

        if (claimButtonImage != null)
        {
            Color imgColor = claimButtonImage.color;
            imgColor.a = alpha;
            claimButtonImage.color = imgColor;
        }

        if (claimButtonText != null)
        {
            Color txtColor = claimButtonText.color;
            txtColor.a = alpha;
            claimButtonText.color = txtColor;
        }
    }

    void FadePanel()
    {
        if (panelBackground == null) return;

        Color c = panelBackground.color;
        c.a = 0.8f;
        panelBackground.color = c;
    }

    void ResetPanelVisual()
    {
        if (panelBackground == null) return;

        Color c = panelBackground.color;
        c.a = 1f;
        panelBackground.color = c;
    }
}