using System.Collections.Generic;
using System;

[System.Serializable]
public class SaveData
{
    public GameSaveData game;
    public QuestSaveData quest;
    public List<TreeSaveInfo> trees = new();
    public TutorialSaveData tutorial;
}

[System.Serializable]
public class GameSaveData
{
    public int level;
    public int coins;
    public int xp;
    public int prevXp;
    public int nextXp;
    public float currentWater;
    public float pollution;

    public List<StockEntry> seedStocks = new();
    public List<StockEntry> fruitStocks = new();
}

[System.Serializable]
public class QuestSaveData
{
    public List<QuestSaveInfo> quests = new();
    public List<QuestQueueSaveInfo> queues = new();
}

[System.Serializable]
public class StockEntry
{
    public string name;
    public int amount;
}

[System.Serializable]
public class QuestSaveInfo
{
    public string questID;
    public int progress;
    public float startTime; // 🔥 FIXED: Tambahkan startTime untuk timer
    public float remainingTime;
    public float finishTime;
    public bool completed;
    public bool failed;
    public bool claimed;
    public bool active;
}

[System.Serializable]
public class QuestQueueSaveInfo
{
    public QuestDificulty difficulty;
    public int currentIndex;
    public string activeQuestID;
    public bool hasActiveNPC;
    public int returnPointIndex;
}

[System.Serializable]
public class TreeSaveInfo
{
    public string treeID;
    public string plotID;

    public float posX;
    public float posY;
    public float posZ;

    public float scaleX;
    public float scaleY;
    public float scaleZ;

    public float health;
    public float growTimer;
    public bool isFullyGrown;
    public bool isDead;
    public bool isWithered;
}

[System.Serializable]
public class TutorialSaveData
{
    public TutorialStep currentStep;
    public string expectedNPCQuestID;
    public string expectedClaimQuestID;
    public bool isFinished;
}
