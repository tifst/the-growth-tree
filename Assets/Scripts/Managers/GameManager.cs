using UnityEngine;
using System;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    public static event Action<bool> OnGameOver;

    [Header("Player Data")]
    public int level = 1;
    public int coins = 200;

    [Header("XP Settings")]
    public List<int> xpTable = new List<int>() {30, 70, 120, 200, 300, 450, 600, 800, 1000}; 
    private int xpIndex = 0;
    public int xp = 0;
    public int prevXp = 0;
    public int nextXp = 30;

    [Header("Water Data")]
    public float maxWater = 5000f;
    public float currentWater = 500f;

    [Header("Tree & Inventory Data")]
    public List<TreeData> allTrees;
    public Dictionary<string, int> fruitStocks = new();
    public Dictionary<string, int> seedStocks = new();

    [Header("City Data")]
    public float pollution = 70.0f;
    public float maxPollution = 100.0f;
    private bool isGameEnded = false;

    [HideInInspector] public UIManager uiManager;

    private float pollutionTimer = 0f;
    private float pollutionInterval = 20f;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        // Initialize fruit & seed dictionary
        if (allTrees != null)
        {
            foreach (var tree in allTrees)
            {
                if (tree == null) continue;

                fruitStocks.TryAdd(tree.treeName, 0);
                seedStocks.TryAdd(tree.treeName, 0);
            }
        }
    }

    void Start()
    {
        uiManager = FindFirstObjectByType<UIManager>();
        uiManager?.UpdateAllUI();

        FogController.Instance?.SetPollution(pollution);
    }

    void Update()
    {
        pollutionTimer += Time.deltaTime;
        if (pollutionTimer >= pollutionInterval)
        {
            pollutionTimer = 0f;
            ModifyPollution(+1f);
        }
    }

    // ================= GAME CONDITION =================
    public void CheckGameCondition()
    {
        if (SaveLoadSystem.Instance != null && SaveLoadSystem.Instance.isLoading) return;
        if (isGameEnded) return;

        if (pollution <= 0f)
            GameOver(true);
        else if (pollution >= 100f)
            GameOver(false);
    }

    private void GameOver(bool isWin)
    {
        isGameEnded = true;
        OnGameOver?.Invoke(isWin);
    }

    // ================= POLLUTION =================
    public void ModifyPollution(float value)
    {
        pollution = Mathf.Clamp(pollution + value, 0, maxPollution);
        uiManager?.UpdatePollution(pollution, maxPollution);

        if (FogController.Instance != null)
            FogController.Instance.SetPollution(pollution);

        CheckGameCondition();
    }

    // ================= WATER =================
    public void ModifyWater(float value)
    {
        currentWater = Mathf.Clamp(currentWater - value, 0f, maxWater);
        uiManager?.UpdateWater(currentWater, maxWater);
    }

    // ================= XP =================
    public void AddXP(int amount)
    {
        xp += amount;

        // selama xp cukup untuk naik level
        while (xp >= nextXp)
        {
            LevelUp();
        }

        uiManager?.UpdateXP(xp, prevXp, nextXp);
    }

    private void LevelUp()
    {
        level++;

        prevXp = nextXp;

        // Jika masih dalam daftar XP Table → pakai nilai berikutnya
        if (xpIndex < xpTable.Count - 1)
        {
            xpIndex++;
            nextXp = xpTable[xpIndex];
        }
        else
        {
            // Di luar tabel → nextXp bertambah +1000 setiap level
            nextXp += 250;
        }

        uiManager?.UpdateLevel(level);
        PromptManager.Instance.Notify($"LEVEL UP! You are now Level {level}!", 5f);
    }

    // ================= FRUIT STOCK =================
    public void ModifyFruitStock(string fruitName, int amount)
    {
        if (!fruitStocks.ContainsKey(fruitName))
        {
            Debug.LogWarning($"❌ Fruit '{fruitName}' not registered!");
            return;
        }

        fruitStocks[fruitName] = Mathf.Max(0, fruitStocks[fruitName] + amount);

        uiManager?.UpdateInventoryUI();
        SaveLoadSystem.Instance.SaveGame();
    }

    // ================= SEED STOCK =================
    public void ModifySeedStock(string seedName, int amount)
    {
        if (!seedStocks.ContainsKey(seedName))
        {
            Debug.LogWarning($"❌ Seed '{seedName}' not registered!");
            return;
        }

        seedStocks[seedName] = Mathf.Max(0, seedStocks[seedName] + amount);

        uiManager?.UpdateInventoryUI();
        SaveLoadSystem.Instance.SaveGame();
    }

    // ================= GETTERS =================
    public int GetFruitStock(string name) => fruitStocks.TryGetValue(name, out int v) ? v : 0;
    public int GetSeedStock(string name) => seedStocks.TryGetValue(name, out int v) ? v : 0;

    public int GetTotalFruits()
    {
        int total = 0;
        foreach (var kv in fruitStocks)
            total += kv.Value;
        return total;
    }

    public int GetTotalSeeds()
    {
        int total = 0;
        foreach (var kv in seedStocks)
            total += kv.Value;
        return total;
    }

    public int GetTotalItems()
    {
        return GetTotalFruits() + GetTotalSeeds();
    }

    public void AddCoins(int amount)
    {
        coins += amount;
        uiManager?.UpdateCoins(coins);
    }

    // ================= RESET =================
    public void ResetAllGameState(bool saveAfter = false)
    {
        // 1️⃣ RESET GAME CORE
        ResetGameData();

        // 2️⃣ RESET QUEST
        QuestManager.Instance?.ResetAll();

        // 3️⃣ RESET WORLD (TREE)
        foreach (var tree in FindObjectsByType<GrowTree>(FindObjectsSortMode.None))
            Destroy(tree.gameObject);

        // 4️⃣ RESET TUTORIAL
        TutorialManager.Instance?.ResetAll();

        // 5️⃣ RESET GUIDE / PROMPT
        GuideManager.Instance?.ClearAllTargets();
        PromptManager.Instance?.ClearAll();

        // 6️⃣ SAVE JIKA DIMINTA
        if (saveAfter)
            SaveLoadSystem.Instance.SaveGame();
    }

    public void ResetGameData()
    {
        isGameEnded = false;

        level = 1;
        coins = 200;

        xp = 0;
        prevXp = 0;
        xpIndex = 0;
        nextXp = xpTable.Count > 0 ? xpTable[0] : 30;

        currentWater = 500f;
        pollution = 70f;
        
        FogController.Instance?.SetPollution(pollution);

        fruitStocks.Clear();
        seedStocks.Clear();

        foreach (var tree in allTrees)
        {
            if (tree == null) continue;
            fruitStocks[tree.treeName] = 0;
            seedStocks[tree.treeName] = 0;
        }

        uiManager?.UpdateAllUI();
    }

    void SyncXPIndex()
    {
        xpIndex = 0;

        for (int i = 0; i < xpTable.Count; i++)
        {
            if (xpTable[i] == nextXp)
            {
                xpIndex = i;
                return;
            }
        }
    }

    public GameSaveData ExportState()
    {
        GameSaveData data = new GameSaveData
        {
            level = level,
            coins = coins,
            xp = xp,
            prevXp = prevXp,
            nextXp = nextXp,
            currentWater = currentWater,
            pollution = pollution
        };

        foreach (var kv in seedStocks)
            data.seedStocks.Add(new StockEntry { name = kv.Key, amount = kv.Value });

        foreach (var kv in fruitStocks)
            data.fruitStocks.Add(new StockEntry { name = kv.Key, amount = kv.Value });

        return data;
    }
    public void ImportState(GameSaveData data)
    {
        level = data.level;
        coins = data.coins;
        xp = data.xp;
        prevXp = data.prevXp;
        nextXp = data.nextXp;
        currentWater = data.currentWater;
        pollution = data.pollution;

        seedStocks.Clear();
        fruitStocks.Clear();

        foreach (var e in data.seedStocks)
            seedStocks[e.name] = e.amount;

        foreach (var e in data.fruitStocks)
            fruitStocks[e.name] = e.amount;
    }
}