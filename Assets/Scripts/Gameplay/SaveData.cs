using System.Collections.Generic;
using System;

[System.Serializable]
public class SaveData
{
    public int level, coins, xp, prevXp, nextXp;
    public float currentWater, pollution;

    public List<StockEntry> seedStocks = new();
    public List<StockEntry> fruitStocks = new();

    public List<TreeSaveInfo> trees = new();
}

[System.Serializable]
public class StockEntry
{
    public string name;
    public int amount;
}


[System.Serializable]
public class TreeSaveInfo
{
    public string treeID;

    public float posX, posY, posZ;
    public float scaleX, scaleY, scaleZ;

    public float health;
    public float growTimer;
    public bool isFullyGrown;
    public bool isDead;
    public bool isWithered;
}
