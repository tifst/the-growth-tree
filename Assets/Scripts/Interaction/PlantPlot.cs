using UnityEngine;

public class PlantPlot : MonoBehaviour
{
    [Header("Tree Prefab Data")]
    [SerializeField] private TreeData selectedTreeData;

    [Header("Offset Posisi Spawn")]
    [SerializeField] private Vector3 spawnOffset = new Vector3(2.5f, 0f, -2.5f);

    private PlotHighlight highlighter;
    private PlantingUI plantingUI;

    [Header("Audio Settings")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip digSound;

    private GameObject plantedTree;
    private GrowTree growTree;

    public bool playerInRange = false;
    public string plantPrompt = "[E] Plant a Tree";
    public string chopPrompt = "[E] Chop Down Dead Tree";

    void Start()
    {
        highlighter = GetComponent<PlotHighlight>();
        if (highlighter != null)
            highlighter.SetEdgesVisible(false);

        plantingUI = FindFirstObjectByType<PlantingUI>();

        if (audioSource == null)
            audioSource = GetComponent<AudioSource>();
    }

    void Update()
    {
        if (plantingUI == null)
            plantingUI = FindFirstObjectByType<PlantingUI>();

        // 🌱 Menanam pohon
        if (playerInRange && plantedTree == null && Input.GetKeyDown(KeyCode.E))
        {
            plantingUI?.OpenPlantingMenu(this);
        }

        // 🌳 Menebang pohon mati
        if (playerInRange && growTree != null && Input.GetKeyDown(KeyCode.E))
        {
            if (growTree.IsTreeDead())
            {
                ChopDownTree();
                PromptUI.Instance.Show(plantPrompt, this);
            }
        }
    }

    public void SetHighlight(bool active)
    {
        highlighter?.SetEdgesVisible(active);
    }

    public void SelectTreeType(TreeData tree)
    {
        selectedTreeData = tree;
    }

    //  MENANAM POHON DENGAN PREFAB COMPLETE TREE
    public void PlantTree()
    {
        SaveLoadSystem.Instance.SaveGame();

        if (selectedTreeData == null) return;
        if (GameManager.Instance == null) return;
        if (GameManager.Instance.GetSeedStock(selectedTreeData.treeName) <= 0) return;

        // Tidak boleh tanam dua kali kecuali pohon sudah mati
        if (plantedTree != null && !growTree.IsTreeDead())
            return;

        // Kurangi seed
        GameManager.Instance.ModifySeedStock(selectedTreeData.treeName, -1);

        // Spawn posisi
        Vector3 pos = transform.position + spawnOffset;

        // === INI YANG PENTING === 
        // PREFAB TREE SUDAH LENGKAP: GrowTree, TreePhysic, FruitSpawner, Partikel, UI, dll.
        plantedTree = Instantiate(selectedTreeData.treePrefab, pos, Quaternion.identity, transform);

        // Ambil reference komponennya
        growTree = plantedTree.GetComponent<GrowTree>();

        if (growTree == null)
            Debug.LogError("❌ Prefab tree TIDAK memiliki GrowTree! Tambahkan ke prefab!");

        // Kasih data tree
        growTree.treeData = selectedTreeData;

        // Tampilkan health bar
        TreeHealthBar.Instance?.ShowTreeBar(growTree);

        // SFX
        if (audioSource != null && digSound != null)
            audioSource.PlayOneShot(digSound);
    }

    //  TEBA NGPOHON MATI
    private void ChopDownTree()
    {
        SaveLoadSystem.Instance.SaveGame();

        if (plantedTree != null)
        {
            Destroy(plantedTree);
            plantedTree = null;
            growTree = null;
            selectedTreeData = null;

            if (audioSource != null && digSound != null)
                audioSource.PlayOneShot(digSound);
        }
    }

    //  PLAYER TRIGGER
    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        playerInRange = true;
        SetHighlight(true);

        if (growTree != null)
        {
            TreeHealthBar.Instance?.ShowTreeBar(growTree);
            growTree.SetPlayerNearby(true);
        }

        if (plantedTree == null)
            PromptUI.Instance.Show(plantPrompt, this);
        else if (growTree.IsTreeDead())
            PromptUI.Instance.Show(chopPrompt, this);
        else
            PromptUI.Instance.Hide(this);
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        playerInRange = false;
        SetHighlight(false);

        if (growTree != null)
        {
            TreeHealthBar.Instance?.HideTreeBar(growTree);
            growTree.SetPlayerNearby(false);
        }

        PromptUI.Instance?.Hide(this);
    }
}