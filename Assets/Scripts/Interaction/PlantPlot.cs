using UnityEngine;

public class PlantPlot : MonoBehaviour, IInteractable
{
    [Header("Tree Prefab Data")]
    [SerializeField] private TreeData selectedTreeData;

    [Header("Offset Posisi Spawn")]
    public Vector3 spawnOffset = new Vector3(2.5f, 0f, -2.5f);

    private PlotHighlight highlighter;
    private PlantingUI plantingUI;

    [Header("Audio Settings")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip digSound;

    private GameObject plantedTree;
    public GrowTree growTree;
    public string plotID;

    public bool playerInRange = false;
    bool hasTriggeredReachPlot = false;
    public InputType InputKey => InputType.E; // default, bisa override di PlayerInteract
    public string PromptMessage
    {
        get
        {
            if (growTree == null)
                return "[E] Plant Tree";

            // 🔥 plot hanya hidup jika owner = Plot
            if (growTree.GetActionOwner() != TreeActionOwner.Plot)
                return "";

            if (growTree.IsTreeDead())
                return "[E] Chop Down Dead Tree";

            if (growTree.CanBeWatered())
                return "[Q] Water Tree";

            return "";
        }
    }

    public void Interact()
    {
        if (growTree == null)
        {
            plantingUI?.OpenPlantingMenu(this);
            return;
        }

        if (growTree.IsTreeDead())
        {
            ChopDownTree();
            return;
        }
    }

    void Start()
    {
        highlighter = GetComponent<PlotHighlight>();
        if (highlighter != null)
            highlighter.SetEdgesVisible(false);

        plantingUI = FindFirstObjectByType<PlantingUI>();

        if (growTree == null)
        {
            growTree = GetComponentInChildren<GrowTree>();
            if (growTree != null)
                growTree.parentPlot = this;
        }

        if (audioSource == null)
            audioSource = GetComponent<AudioSource>();
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

        TutorialEvents.OnPlant?.Invoke();

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
            Debug.LogError("Prefab tree TIDAK memiliki GrowTree! Tambahkan ke prefab!");

        // Kasih data tree
        growTree.parentPlot = this;
        growTree.treeData = selectedTreeData;
        growTree.currentHealth = selectedTreeData.startHealth;

        // Tampilkan health bar
        TreeHealthBar.Instance?.ShowTreeBar(growTree);

        // SFX
        if (audioSource != null && digSound != null)
            audioSource.PlayOneShot(digSound);

        var player = FindFirstObjectByType<PlayerInteract>();
        player?.ForceRefreshPlot(this);
    }

    //  TEBANG POHON MATI
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

        var player = FindFirstObjectByType<PlayerInteract>();
        player?.ForceRefreshPlot(this);
    }

    //  PLAYER TRIGGER
    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        if (!hasTriggeredReachPlot)
        {
            hasTriggeredReachPlot = true;
            TutorialEvents.OnReachPlot?.Invoke();
        }

        playerInRange = true;
        SetHighlight(true);

        if (growTree != null)
        {
            TreeHealthBar.Instance?.ShowTreeBar(growTree);
            growTree.SetPlayerNearby(true);
        }
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
    }

    void OnEnable()
    {
        GrowTree.OnActionOwnerChanged += HandleActionChanged;
    }

    void OnDisable()
    {
        GrowTree.OnActionOwnerChanged -= HandleActionChanged;
    }
    
    public void SetPlantedTree(GameObject tree)
    {
        plantedTree = tree;
    }

    void HandleActionChanged(GrowTree tree)
    {
        if (tree != growTree) return;
        if (!playerInRange) return;

        PromptManager.Instance.RefreshContext(this, PromptMessage);
    }
}