using System.Collections.Generic;
using UnityEngine;

public class GlobalUIManager : MonoBehaviour
{
    public static GlobalUIManager Instance;
    public GameObject pauseMenuPanel;
    public GameObject HUDCanvas;

    // Stack panel yg benar-benar UI (inventory, shop, quest, crafting, dsb.)
    private Stack<GameObject> uiStack = new Stack<GameObject>();

    // Panel yang tidak menghitung sebagai UI blocking
    [Header("Excluded Panels (HUD/Prompt/etc)")]
    public List<GameObject> excludedPanels = new List<GameObject>();

    public bool IsAnyPanelOpen => uiStack.Count > 0;

    void Awake()
    {
        Instance = this;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
            HandleEscape();
    }

    //  OPEN PANEL
    public void OpenPanel(GameObject panel)
    {
        if (panel == null) return;

        if (panel == pauseMenuPanel)
        {
            panel.SetActive(true);
            return;
        }

        if (uiStack.Count > 0)
        {
            Debug.Log($"[UI] Cannot open '{panel.name}' because '{uiStack.Peek().name}' is already open.");
            return;
        }

        // Jika panel termasuk EXCLUDED → jangan masuk stack
        if (excludedPanels.Contains(panel))
        {
            panel.SetActive(true);
            return;
        }

        // Panel UI normal → masuk stack
        if (!panel.activeSelf)
        {
            panel.SetActive(true);
            uiStack.Push(panel);

            // Lock player ketika UI pertama dibuka
            if (uiStack.Count == 1)
                PlayerLockManager.Instance.Lock();

            PauseMenuManager.Instance.uiIsBlockingPause = uiStack.Count > 0;

            Debug.Log($"[UI] Opened Panel: {panel.name}, Total Open: {uiStack.Count}");
        }
    }

    //  CLOSE PANEL
    public void ClosePanel(GameObject panel)
    {
        if (panel == null) return;

        if (panel == pauseMenuPanel)
        {
            panel.SetActive(false);
            return;
        }

        // Jika panel EXCLUDED → cukup setActive(false)
        if (excludedPanels.Contains(panel))
        {
            panel.SetActive(false);
            return;
        }

        // UI normal (stack)
        if (panel.activeSelf)
        {
            panel.SetActive(false);

            if (uiStack.Count > 0 && uiStack.Peek() == panel)
                uiStack.Pop();

            PauseMenuManager.Instance.uiIsBlockingPause = uiStack.Count > 0;

            Debug.Log($"[UI] Closed Panel: {panel.name}, Left: {uiStack.Count}");
        }

        // Kalau stack kosong → player bebas & pause bisa dipakai
        if (uiStack.Count == 0)
        {
            if (PauseMenuManager.Instance != null && PauseMenuManager.Instance.IsPaused)
                return;

            HUDCanvas.SetActive(true);
            PlayerLockManager.Instance.Unlock();
            PauseMenuManager.Instance.uiIsBlockingPause = false;
            PauseMenuManager.Instance.LockInputFor(0.25f);
        }
    }

    //  ESC LOGIC
    private void HandleEscape()
    {
        // Jika ada UI active dalam stack → tutup panel paling atas
        if (uiStack.Count > 0)
        {
            ClosePanel(uiStack.Peek());
            return;
        }

        // Jika hanya panel HUD/prompt → tetap boleh buka pause menu
        PauseMenuManager.Instance.TogglePause();
    }

    // Optional: close all real UI
    public void CloseAllPanels()
    {
        foreach (var p in uiStack)
            p.SetActive(false);

        uiStack.Clear();

        PlayerLockManager.Instance.Unlock();
        PauseMenuManager.Instance.uiIsBlockingPause = false;
    }
}