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

    public Button claimButton;

    private TMP_Text claimButtonText;
    private Image claimButtonImage;

    private QuestData data;
    private int progress;
    private bool claimed = false;
    public bool isClaimed => claimed;

    // AUTO FIND — dipanggil SETIAP KALI sebelum dipakai
    private void ResolveButtonComponents()
    {
        if (claimButton == null)
            claimButton = GetComponentInChildren<Button>(true);

        if (claimButton != null)
        {
            if (claimButtonText == null)
                claimButtonText = claimButton.GetComponentInChildren<TMP_Text>(true);

            if (claimButtonImage == null)
                claimButtonImage = claimButton.GetComponentInChildren<Image>(true);
        }
    }

    public void Setup(QuestData q)
    {
        data = q;
        progress = 0;

        ResolveButtonComponents();

        titleText.text = $"{q.questTitle} ({q.difficulty})";
        shortDescText.text = q.shortDescription;
        iconImage.sprite = q.questIcon;
        countText.text = $"{q.requiredAmount}x";

        rewardText.text = $"+{q.rewardCoins} Coins   +{q.rewardXP} XP";
        durationText.text = q.hasTimer ? $"{q.duration:F0}s" : "None";

        progressText.text = $"0 / {q.requiredAmount}";
        progressFill.fillAmount = 0f;

        SetClaimButton(false);

        claimButton.onClick.RemoveAllListeners();
        claimButton.onClick.AddListener(() =>
        {
            if (!claimed)
                QuestManager.Instance.ClaimQuest(q);
        });
    }

    private void SetClaimButton(bool enabled)
    {
        ResolveButtonComponents();

        claimButton.interactable = enabled;

        if (enabled)
        {
            claimButtonText.text = "Claim";
            claimButtonText.color = Color.white;
            claimButtonImage.color = new Color32(60, 200, 60, 255);
        }
        else
        {
            claimButtonText.text = "Claim";
            claimButtonText.color = new Color32(150, 150, 150, 255);
            claimButtonImage.color = new Color32(100, 100, 100, 255);
        }
    }

    public void UpdateProgress(int now, int max)
    {
        progress = now;

        progressText.text = $"{now} / {max}";
        progressFill.fillAmount = Mathf.Clamp01((float)now / max);
    }

    public void MarkCompleted(float time)
    {
        if (claimed) return;

        durationText.text = "Completed";
        durationText.color = new Color32(80, 255, 80, 255);

        SetClaimButton(true);
    }

    public void MarkClaimed()
    {
        claimed = true;

        ResolveButtonComponents();

        claimButton.interactable = false;
        claimButtonText.text = "Claimed";
        claimButtonText.color = new Color32(180, 180, 180, 255);
        claimButtonImage.color = new Color32(120, 120, 120, 255);
    }

    public void MarkFailed()
    {
        ResolveButtonComponents();

        durationText.text = "Failed";
        durationText.color = new Color32(255, 80, 80, 255);

        claimButton.interactable = false;
        claimButtonText.text = "Failed";
        claimButtonText.color = new Color32(200, 100, 100, 255);
        claimButtonImage.color = new Color32(120, 60, 60, 255);
    }
}