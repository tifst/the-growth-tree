using UnityEngine;
using System.IO;
using System.Collections.Generic;

public class SaveLoadSystem : MonoBehaviour
{
    public static SaveLoadSystem Instance;
    private string savePath;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        savePath = Application.persistentDataPath + "/save.json";
    }

    // ========================= SAVE =========================
    public void SaveGame()
    {
        SaveData data = new SaveData();
        GameManager gm = GameManager.Instance;

        // -------- Player --------
        data.level = gm.level;
        data.coins = gm.coins;
        data.xp = gm.xp;
        data.prevXp = gm.prevXp;
        data.nextXp = gm.nextXp;
        data.currentWater = gm.currentWater;
        data.pollution = gm.pollution;

        // -------- Inventory (convert Dictionary → List) --------
        data.seedStocks = new List<StockEntry>();
        foreach (var kv in gm.seedStocks)
            data.seedStocks.Add(new StockEntry { name = kv.Key, amount = kv.Value });

        data.fruitStocks = new List<StockEntry>();
        foreach (var kv in gm.fruitStocks)
            data.fruitStocks.Add(new StockEntry { name = kv.Key, amount = kv.Value });

        // -------- Trees --------
        data.trees = new List<TreeSaveInfo>();

        foreach (var tree in FindObjectsByType<GrowTree>(FindObjectsSortMode.None))
        {
            TreeSaveInfo t = new TreeSaveInfo();

            t.treeID = tree.treeData.treeName;

            t.posX = tree.transform.position.x;
            t.posY = tree.transform.position.y;
            t.posZ = tree.transform.position.z;

            t.scaleX = tree.transform.localScale.x;
            t.scaleY = tree.transform.localScale.y;
            t.scaleZ = tree.transform.localScale.z;

            t.health = tree.currentHealth;

            // PRIVATE FIELDS
            t.growTimer = GetPrivateFloat(tree, "growTimer");
            t.isFullyGrown = GetPrivateBool(tree, "isFullyGrown");
            t.isDead = GetPrivateBool(tree, "isDead");
            t.isWithered = GetPrivateBool(tree, "isWithered");

            data.trees.Add(t);
        }

        File.WriteAllText(savePath, JsonUtility.ToJson(data, true));
        Debug.Log("Saved game → " + savePath);
    }

    // Helpers untuk membaca private field
    private float GetPrivateFloat(GrowTree tree, string field)
    {
        return (float)typeof(GrowTree).GetField(field,
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
            .GetValue(tree);
    }

    private bool GetPrivateBool(GrowTree tree, string field)
    {
        return (bool)typeof(GrowTree).GetField(field,
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
            .GetValue(tree);
    }

    // ========================= LOAD =========================
    public void LoadGame()
    {
        if (!File.Exists(savePath))
        {
            Debug.Log("No save file.");
            return;
        }

        string json = File.ReadAllText(savePath);
        SaveData data = JsonUtility.FromJson<SaveData>(json);
        GameManager gm = GameManager.Instance;

        // -------- Player --------
        gm.level = data.level;
        gm.coins = data.coins;
        gm.xp = data.xp;
        gm.prevXp = data.prevXp;
        gm.nextXp = data.nextXp;
        gm.currentWater = data.currentWater;
        gm.pollution = data.pollution;

        // -------- Inventory (convert List → Dictionary) --------
        gm.seedStocks.Clear();
        foreach (var entry in data.seedStocks)
            gm.seedStocks[entry.name] = entry.amount;

        gm.fruitStocks.Clear();
        foreach (var entry in data.fruitStocks)
            gm.fruitStocks[entry.name] = entry.amount;

        // ===== FIX BARU DI SINI =====
        // Register semua TreeData baru
        foreach (var td in gm.allTrees)
        {
            if (!gm.seedStocks.ContainsKey(td.treeName))
                gm.seedStocks[td.treeName] = 0;

            if (!gm.fruitStocks.ContainsKey(td.treeName))
                gm.fruitStocks[td.treeName] = 0;
        }

        // -------- Destroy existing trees --------
        foreach (var tree in FindObjectsByType<GrowTree>(FindObjectsSortMode.None))
            Destroy(tree.gameObject);

        // -------- Load trees --------
        foreach (var info in data.trees)
        {
            TreeData td = gm.allTrees.Find(t => t.treeName == info.treeID);
            if (td == null) continue;

            GameObject obj = Instantiate(td.treePrefab);
            obj.transform.position = new Vector3(info.posX, info.posY, info.posZ);
            obj.transform.localScale = new Vector3(info.scaleX, info.scaleY, info.scaleZ);

            GrowTree tree = obj.GetComponent<GrowTree>();
            tree.currentHealth = info.health;

            // Set private fields
            SetPrivate(tree, "growTimer", info.growTimer);
            SetPrivate(tree, "isFullyGrown", info.isFullyGrown);
            SetPrivate(tree, "wasFullyGrown", info.isFullyGrown);
            SetPrivate(tree, "isDead", info.isDead);
            SetPrivate(tree, "isWithered", info.isWithered);

            tree.SendMessage("UpdateVisualByHealth", SendMessageOptions.DontRequireReceiver);
        }

        if (gm.uiManager == null)
        gm.uiManager = FindFirstObjectByType<UIManager>();

        if (gm.uiManager != null)
            gm.uiManager.UpdateAllUI();
        else
            Debug.LogWarning("⚠ UIManager not found when loading game!");

        Debug.Log("Game loaded!");
    }

    private void SetPrivate(GrowTree tree, string field, object value)
    {
        typeof(GrowTree).GetField(field,
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
            ?.SetValue(tree, value);
    }
}