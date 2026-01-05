using UnityEngine;

public enum TutorialStep
{
    None,
    Move,
    OpenQuest,
    BuySeed,
    ReachPlot,
    Plant,
    Water,
    Refill,
    TreeGrow,
    TreeShake,
    PickupFruit,
    OpenInventory,
    SellFruit,

    ClaimQuest,
    InteractNPC,

    Finished
}

public class TutorialManager : MonoBehaviour
{
    public static TutorialManager Instance;

    public TutorialStep currentStep = TutorialStep.None;

    [Header("Scene Targets")]
    public Transform buyShop;
    public Transform sellShop;
    public Transform plot;
    public Transform well;

    // ===== INTERNAL STATE =====
    string expectedNPCQuestID;
    string expectedClaimQuestID;

    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    void OnEnable()
    {
        TutorialEvents.OnMove += HandleMove;
        TutorialEvents.OnInteractNPC += HandleInteractNPC;
        TutorialEvents.OnOpenQuest += HandleOpenQuest;
        TutorialEvents.OnBuySeed += HandleBuySeed;
        TutorialEvents.OnReachPlot += HandleReachPlot;
        TutorialEvents.OnPlant += HandlePlant;
        TutorialEvents.OnWater += HandleWater;
        TutorialEvents.OnRefill += HandleRefill;
        TutorialEvents.OnTreeGrow += HandleTreeGrow;
        TutorialEvents.OnClaimQuest += HandleClaimQuest;
        TutorialEvents.OnTreeShake += HandleTreeShake;
        TutorialEvents.OnPickupFruit += HandlePickupFruit;
        TutorialEvents.OnOpenInventory += HandleOpenInventory;
        TutorialEvents.OnSellFruit += HandleSellFruit;
    }

    void OnDisable()
    {
        TutorialEvents.OnMove -= HandleMove;
        TutorialEvents.OnInteractNPC -= HandleInteractNPC;
        TutorialEvents.OnOpenQuest -= HandleOpenQuest;
        TutorialEvents.OnBuySeed -= HandleBuySeed;
        TutorialEvents.OnReachPlot -= HandleReachPlot;
        TutorialEvents.OnPlant -= HandlePlant;
        TutorialEvents.OnWater -= HandleWater;
        TutorialEvents.OnRefill -= HandleRefill;
        TutorialEvents.OnTreeGrow -= HandleTreeGrow;
        TutorialEvents.OnClaimQuest -= HandleClaimQuest;
        TutorialEvents.OnTreeShake -= HandleTreeShake;
        TutorialEvents.OnPickupFruit -= HandlePickupFruit;
        TutorialEvents.OnOpenInventory -= HandleOpenInventory;
        TutorialEvents.OnSellFruit -= HandleSellFruit;
    }

    void Start()
    {
        // Kalau ada save, ImportState akan memanggil StartStep sendiri
        if (SaveLoadSystem.Instance != null &&
            SaveLoadSystem.Instance.HasSave())
            return;

        StartStep(TutorialStep.Move);
    }

    // =============== EVENT HANDLERS ==================
    void HandleMove()
    {
        if (currentStep != TutorialStep.Move) return;
        expectedNPCQuestID = "E1";
        StartStep(TutorialStep.InteractNPC);
    }

    void HandleInteractNPC(string questID)
    {
        if (currentStep != TutorialStep.InteractNPC) return;
        if (questID != expectedNPCQuestID) return;
        
        // BRANCHING BERDASARKAN NPC
        if (questID == "E1")
            StartStep(TutorialStep.OpenQuest);

        else if (questID == "E2")
            StartStep(TutorialStep.ReachPlot);

        else if (questID == "E3")
            StartStep(TutorialStep.TreeShake);

        else if (questID == "E4")
            StartStep(TutorialStep.OpenInventory);
    }

    void HandleClaimQuest(string questID)
    {
        if (currentStep != TutorialStep.ClaimQuest) return;
        if (questID != expectedClaimQuestID) return;

        // BRANCH CLAIM (BISA 3+)
        if (questID == "E1")
        {
            expectedNPCQuestID = "E2";
            StartStep(TutorialStep.InteractNPC);
        }

        else if (questID == "E2")
        {
            expectedNPCQuestID = "E3";
            StartStep(TutorialStep.InteractNPC);
        }

        else if (questID == "E3")
        {
            expectedNPCQuestID = "E4";
            StartStep(TutorialStep.InteractNPC);
        }

        else if (questID == "E4")
            StartStep(TutorialStep.Finished);
    }

    void HandleOpenQuest()
    {
        if (currentStep != TutorialStep.OpenQuest) return;
        StartStep(TutorialStep.BuySeed);
    }

    void HandleBuySeed()
    {
        if (currentStep != TutorialStep.BuySeed) return;
        expectedClaimQuestID = "E1";
        StartStep(TutorialStep.ClaimQuest);
    }

    void HandleReachPlot()
    {
        if (currentStep != TutorialStep.ReachPlot) return;
        StartStep(TutorialStep.Plant);
    }

    void HandlePlant()
    {
        if (currentStep != TutorialStep.Plant) return;
        StartStep(TutorialStep.Water);
    }

    void HandleWater()
    {
        if (currentStep != TutorialStep.Water) return;
        StartStep(TutorialStep.Refill);
    }

    void HandleRefill()
    {
        if (currentStep != TutorialStep.Refill) return;
        StartStep(TutorialStep.TreeGrow);
    }

    void HandleTreeGrow()
    {
        if (currentStep != TutorialStep.TreeGrow) return;
        expectedClaimQuestID = "E2";
        StartStep(TutorialStep.ClaimQuest);
    }

    void HandleTreeShake()
    {
        if (currentStep != TutorialStep.TreeShake) return;
        StartStep(TutorialStep.PickupFruit);
    }

    void HandlePickupFruit()
    {
        if (currentStep != TutorialStep.PickupFruit) return;
        expectedClaimQuestID = "E3";
        StartStep(TutorialStep.ClaimQuest);
    }

    void HandleOpenInventory()
    {
        if (currentStep != TutorialStep.OpenInventory) return;
        StartStep(TutorialStep.SellFruit);
    }

    void HandleSellFruit()
    {
        if (currentStep != TutorialStep.SellFruit) return;
        expectedClaimQuestID = "E4";
        StartStep(TutorialStep.ClaimQuest);
    }

    string GetInteractNPCNarrative(string questID)
    {
        return questID switch
        {
            "E1" => "Talk to the Mayor",
            "E2" => "Talk to the Mayor again",
            "E3" => "Talk to the Mayor again",
            "E4" => "Talk to the Merchant",
            _ => "Talk to the NPC"
        };
    }

    string GetClaimQuestNarrative(string questID)
    {
        return questID switch
        {
            "E1" => "Claim your first quest reward",
            "E2" => "Claim the planting reward",
            "E3" => "Claim the harvesting reward",
            "E4" => "Claim the selling reward",
            _ => "Claim the quest reward"
        };
    }

    // ================== STEP SETUP ===================
    void StartStep(TutorialStep step)
    {
        currentStep = step;

        GuideManager.Instance.HideNarrative();

        if (step != TutorialStep.InteractNPC)
        {
            GuideManager.Instance.ClearTutorialGuide();
        }

        switch (step)
        {
            case TutorialStep.Move:
                GuideManager.Instance.ShowNarrative("Use WASD to move");
                break;

            case TutorialStep.InteractNPC:
                GuideManager.Instance.ShowNarrative(GetInteractNPCNarrative(expectedNPCQuestID));
                Transform npc = ResolveNPCTarget(expectedNPCQuestID);
                GuideManager.Instance.PointTo(npc);

                // 🔥 JIKA NPC SAMA, PAKSA AKTIFKAN
                GuideManager.Instance.EnsureTutorialArrowActive();
                break;

            case TutorialStep.ClaimQuest:
                GuideManager.Instance.ShowNarrative(GetClaimQuestNarrative(expectedClaimQuestID));
                break;

            case TutorialStep.OpenQuest:
                GuideManager.Instance.ShowNarrative("Press [L] to see all your queest");
                break;

            case TutorialStep.BuySeed:
                GuideManager.Instance.ShowNarrative("Go to the shop and buy seeds");
                GuideManager.Instance.PointTo(buyShop);
                break;

            case TutorialStep.ReachPlot:
                GuideManager.Instance.ShowNarrative("Go to the planting plot");
                GuideManager.Instance.PointTo(plot);
                break;

            case TutorialStep.Plant:
                GuideManager.Instance.ShowNarrative("Plant the seed");
                break;

            case TutorialStep.Water:
                GuideManager.Instance.ShowNarrative("Water the plant when the tree withers");
                break;

            case TutorialStep.Refill:
                GuideManager.Instance.ShowNarrative("Refill the water in the well");
                GuideManager.Instance.PointTo(well);
                break;

            case TutorialStep.TreeGrow:
                GuideManager.Instance.ShowNarrative("Wait until the tree is fully grown. Water it if needed");
                break;

            case TutorialStep.TreeShake:
                GuideManager.Instance.ShowNarrative("Approach the tree and shake it to drop the fruit");
                break;

            case TutorialStep.PickupFruit:
                GuideManager.Instance.ShowNarrative("Pick up the fruit");
                break;

            case TutorialStep.OpenInventory:
                GuideManager.Instance.ShowNarrative("Press [I] to open Inventory");
                break;

            case TutorialStep.SellFruit:
                GuideManager.Instance.ShowNarrative("Sell the fruit to the shop to get coins");
                GuideManager.Instance.PointTo(sellShop);
                break;

            case TutorialStep.Finished:
                GuideManager.Instance.HideNarrative();
                GuideManager.Instance.ClearTutorialGuide();
                PromptManager.Instance.Notify("Tutorial Finished,\nLet's keep planting and taking care\nof the tree to keep the pollution controlled",5f);
                break;
        }
    }
    
    // ================== RESOLVER =====================
    Transform ResolveNPCTarget(string questID)
    {
        QuestTrigger[] npcs = FindObjectsByType<QuestTrigger>(FindObjectsSortMode.None);

        foreach (var npc in npcs)
        {
            if (npc.questToGive != null &&
                npc.questToGive.questID == questID)
            {
                return npc.transform;
            }
        }
        return null;
    }

    public TutorialSaveData ExportState()
    {
        return new TutorialSaveData
        {
            currentStep = currentStep,
            expectedNPCQuestID = expectedNPCQuestID,
            expectedClaimQuestID = expectedClaimQuestID,
            isFinished = currentStep == TutorialStep.Finished
        };
    }

    public void ImportState(TutorialSaveData data)
    {
        if (data == null) return;

        expectedNPCQuestID = data.expectedNPCQuestID;
        expectedClaimQuestID = data.expectedClaimQuestID;

        if (data.isFinished)
        {
            currentStep = TutorialStep.Finished;
            GuideManager.Instance.ClearTutorialGuide();
            GuideManager.Instance.HideNarrative();
            return;
        }

        // Restart step UI
        StartStep(data.currentStep);
    }

    public void ResetAll()
    {
        currentStep = TutorialStep.None;
        expectedNPCQuestID = null;
        expectedClaimQuestID = null;

        GuideManager.Instance.ClearTutorialGuide();
        GuideManager.Instance.HideNarrative();

        // mulai ulang tutorial dari awal
        StartStep(TutorialStep.Move);
    }
}
