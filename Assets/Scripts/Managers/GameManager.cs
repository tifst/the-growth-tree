using UnityEngine;
using System.Collections.Generic;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("Player Data")]
    public int level = 1;
    public int coins = 300;

    [Header("XP Settings")]
    public List<int> xpTable = new List<int>() {20, 50, 100, 200, 350, 600, 1000}; 
    private int xpIndex = 0;
    public int xp = 0;
    public int prevXp = 0;
    public int nextXp = 20;

    [Header("Water Data")]
    public float maxWater = 1000f;
    public float currentWater = 500f;

    [Header("Tree & Inventory Data")]
    public List<TreeData> allTrees;
    public Dictionary<string, int> fruitStocks = new();
    public Dictionary<string, int> seedStocks = new();

    [Header("City Data")]
    public float pollution = 75.0f;
    public float maxPollution = 100.0f;

    [Header("Game Result UI")]
    public GameObject winPanel;
    public GameObject losePanel;
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
        SaveLoadSystem.Instance.LoadGame();
        
        if (uiManager == null)
            uiManager = FindFirstObjectByType<UIManager>();

        uiManager?.UpdateAllUI();

        winPanel?.SetActive(false);
        losePanel?.SetActive(false);
    }

    void Update()
    {
        // Pollution fog update
        if (FogController.Instance != null)
            FogController.Instance.SetPollution(pollution);

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
        if (isGameEnded) return;

        if (pollution <= 20f)
            GameOver(true);
        else if (pollution >= 90f)
            GameOver(false);
    }

    private void GameOver(bool isWin)
    {
        isGameEnded = true;
        Time.timeScale = 0f;

        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;

        if (isWin)
        {
            winPanel?.SetActive(true);
            AudioManager.Instance?.PlayWinMusic();
        }
        else
        {
            losePanel?.SetActive(true);
            AudioManager.Instance?.PlayLoseMusic();
        }
    }

    // ================= POLLUTION =================
    public void ModifyPollution(float value)
    {
        pollution = Mathf.Clamp(pollution + value, 0, maxPollution);
        uiManager?.UpdatePollution(pollution, maxPollution);
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
        SaveLoadSystem.Instance.SaveGame();

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
        SaveLoadSystem.Instance.SaveGame();

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
            nextXp += 500;
        }

        uiManager?.UpdateLevel(level);
    }

    // ================= FRUIT STOCK =================
    public void ModifyFruitStock(string fruitName, int amount)
    {
        SaveLoadSystem.Instance.SaveGame();

        if (!fruitStocks.ContainsKey(fruitName))
        {
            Debug.LogWarning($"❌ Fruit '{fruitName}' not registered!");
            return;
        }

        fruitStocks[fruitName] = Mathf.Max(0, fruitStocks[fruitName] + amount);

        uiManager?.UpdateFruits(GetTotalFruits());
        uiManager?.UpdateInventoryUI();
    }

    // ================= SEED STOCK =================
    public void ModifySeedStock(string seedName, int amount)
    {
        SaveLoadSystem.Instance.SaveGame();

        if (!seedStocks.ContainsKey(seedName))
        {
            Debug.LogWarning($"❌ Seed '{seedName}' not registered!");
            return;
        }

        seedStocks[seedName] = Mathf.Max(0, seedStocks[seedName] + amount);

        uiManager?.UpdateSeeds(GetTotalSeeds());
        uiManager?.UpdateInventoryUI(); // penting!
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
    public void ResetGameData()
    {
        level = 1;
        coins = 300;
        xp = 0;
        prevXp = 0;
        nextXp = 20;

        maxWater = 1000f;
        currentWater = 500f;

        pollution = 75f;

        fruitStocks.Clear();
        seedStocks.Clear();

        foreach (var tree in allTrees)
        {
            if (tree == null) continue;
            fruitStocks.TryAdd(tree.treeName, 0);
            seedStocks.TryAdd(tree.treeName, 0);
        }

        uiManager?.UpdateAllUI();  // penting!
    }
}