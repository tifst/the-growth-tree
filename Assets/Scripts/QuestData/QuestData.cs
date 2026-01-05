using UnityEngine;

public enum QuestType { Main, Side, Random }
public enum QuestDificulty { Easy, Medium, Hard }
public enum QuestGoalType { BuySeed, PlantTree, HarvestFruit, SellFruit, DeliverItem}

[CreateAssetMenu(fileName = "NewQuest", menuName = "Game/Quest Data")]
public class QuestData : ScriptableObject
{
    [Header("Identitas Quest")]
    public string questID;
    public string questTitle;

    [TextArea(1, 2)]
    public string shortDescription;

    [TextArea(3, 5)]
    public string description;
    public Sprite questIcon;

    [Header("NPC Quest")]
    public GameObject npcPrefab;

    public QuestType type;
    public QuestDificulty difficulty;
    public QuestGoalType goalType;

    [Header("Syarat Quest")]
    public string targetName;  
    public int requiredAmount = 1;
    public bool forceNewNPC = false; 

    [Header("Reward")]
    public int rewardXP;
    public int rewardCoins;

    [Header("Optional Timer")]
    public bool hasTimer = false;
    public float duration = 0f;

    [Header("Status Quest")]
    public bool isRepeatable;
}