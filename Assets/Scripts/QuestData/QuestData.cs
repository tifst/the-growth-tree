using UnityEngine;

public enum QuestType { Main, Side, Random }
public enum QuestDificulty { Easy, Medium, Hard }
public enum QuestGoalType { PlantTree, HarvestFruit, SellFruit, DeliverItem, TalkToNPC }

[CreateAssetMenu(fileName = "NewQuest", menuName = "Game/Quest Data")]
public class QuestData : ScriptableObject
{
    [Header("Identitas Quest")]
    public string questID;
    public string questTitle;

    [TextArea(1, 2)]
    public string shortDescription; // 🔥 Deskripsi ringkas (untuk HUD / popup)

    [TextArea(3, 5)]
    public string description; // 🔥 Deskripsi lengkap

    public Sprite questIcon; // 🔥 Ikon quest (untuk HUD, Notification, UI Panel)

    public QuestType type;
    public QuestDificulty difficulty;
    public QuestGoalType goalType;

    [Header("Syarat Quest")]
    public string targetName;  
    public int requiredAmount = 1;

    [Header("Reward")]
    public int rewardXP;
    public int rewardCoins;
    public float rewardPollutionReduction;

    [Header("Optional Timer")]
    public bool hasTimer = false;
    public float duration = 0f;

    [Header("Status Quest")]
    public bool isRepeatable;
}