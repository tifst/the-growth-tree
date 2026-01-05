using UnityEngine;
using System.Collections;
using System.Linq;

public class SaveLoadSystem : MonoBehaviour
{
    public static SaveLoadSystem Instance;
    public bool isLoading;

    void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    // ================= SAVE =================
    public void SaveGame()
    {
        if (isLoading || SaveService.Instance == null) return;

        SaveData data = new SaveData
        {
            game = GameManager.Instance.ExportState(),
            quest = QuestManager.Instance.ExportState(),
            tutorial = TutorialManager.Instance != null
                ? TutorialManager.Instance.ExportState() : null
        };

        foreach (var tree in FindObjectsByType<GrowTree>(FindObjectsSortMode.None))
        {
            if (tree == null) continue;
            data.trees.Add(tree.ExportState());
        }

        string json = JsonUtility.ToJson(data, true);
        SaveService.Instance.Write(json);
    }

    public bool HasSave()
    {
        return SaveService.Instance != null && SaveService.Instance.HasSave();
    }

    // ================= LOAD (ASYNC) =================
    public IEnumerator LoadGameRoutine()
    {
        if (!HasSave()) yield break;

        isLoading = true;

        string json = SaveService.Instance.Read();
        SaveData data = JsonUtility.FromJson<SaveData>(json);

        // 1️⃣ GAME STATE
        GameManager.Instance.ImportState(data.game);

        // 2️⃣ HAPUS TREE LAMA
        foreach (var old in FindObjectsByType<GrowTree>(FindObjectsSortMode.None))
            Destroy(old.gameObject);

        yield return null;

        // 3️⃣ SPAWN TREE
        foreach (var info in data.trees)
        {
            TreeData td = GameManager.Instance.allTrees
                .Find(t => t.treeName == info.treeID);
            if (!td) continue;

            PlantPlot plot = FindObjectsByType<PlantPlot>(FindObjectsSortMode.None)
                .FirstOrDefault(p => p.plotID == info.plotID);

            if (plot == null)
            {
                Debug.LogWarning("Plot tidak ditemukan: " + info.plotID);
                continue;
            }

            GameObject obj = Instantiate(
                td.treePrefab,
                plot.transform.position + plot.spawnOffset,
                Quaternion.identity,
                plot.transform // 🔥 parent ke plot
            );

            GrowTree tree = obj.GetComponent<GrowTree>();

            // 🔥 LINK BALIK
            tree.parentPlot = plot;
            plot.growTree = tree;

            tree.ImportState(info);
            yield return null;
        }

        // 4️⃣ QUEST (SETELAH TREE ADA)
        QuestManager.Instance.ImportState(data.quest);

        // 5️⃣ TUTORIAL TERAKHIR
        if (data.tutorial != null && TutorialManager.Instance != null)
            TutorialManager.Instance.ImportState(data.tutorial);

        // 6️⃣ UI REFRESH
        GameManager.Instance.uiManager?.UpdateAllUI();
        
        foreach (var plot in FindObjectsByType<PlantPlot>(FindObjectsSortMode.None))
        {
            if (plot.growTree != null)
            {
                TreeHealthBar.Instance?.ShowTreeBar(plot.growTree);
            }
        }

        isLoading = false;
    }
}