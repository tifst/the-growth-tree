using System;

public static class TutorialEvents
{
    public static Action OnMove;
    public static Action<string> OnInteractNPC;
    public static Action<string> OnClaimQuest;
    public static Action OnOpenQuest;
    public static Action OnBuySeed;
    public static Action OnReachPlot;
    public static Action OnPlant;
    public static Action OnWater;
    public static Action OnRefill;
    public static Action OnTreeGrow;
    public static Action OnTreeShake;
    public static Action OnPickupFruit;
    public static Action OnOpenInventory;
    public static Action OnSellFruit;
}