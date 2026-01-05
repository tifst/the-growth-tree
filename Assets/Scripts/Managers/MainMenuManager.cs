using UnityEngine;
using UnityEngine.SceneManagement;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class MainMenuManager : MonoBehaviour
{
    [Header("Pengaturan Scene")]
    [Tooltip("Nama scene gameplay harus PERSIS sama dengan yang ada di Build Settings")]
    public string gameSceneName = "MainScene";

    [Header("Panel Referensi (Drag dari Hierarchy)")]
    public GameObject mainMenuCanvas;   // Panel Menu Utama
    public GameObject creditsPanel;     // Panel Credit
    public GameObject infoPanel;        // Panel Info / Controls (BARU)

    [Header("Reset Confirmation")]
    public GameObject resetConfirmPanel;

    // --- FUNGSI UTAMA ---
    void Update()
    {
        if (resetConfirmPanel.activeSelf || creditsPanel.activeSelf || infoPanel.activeSelf)
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                // Tutup panel yang aktif
                if (resetConfirmPanel.activeSelf)
                    CloseReset();
                else if (creditsPanel.activeSelf)
                    HideCredits();
                else if (infoPanel.activeSelf)
                    HideInfo();
            }
        }
    }
    
    public void PlayGame()
    {
        if (Application.CanStreamedLevelBeLoaded(gameSceneName))
        {
            SceneLoader.Instance.LoadGameScene(gameSceneName);
            GameSession.ForceNewGame = false;
        }
        else
        {
            Debug.LogError("Scene '" + gameSceneName + "' nggak ketemu! Cek Build Settings.");
        }
    }

    public void QuitGame()
    {
        Debug.Log("Keluar Game...");
        #if UNITY_EDITOR
            EditorApplication.isPlaying = false;
        #else
            Application.Quit();
        #endif
    }

    // --- LOGIKA CREDIT ---
    public void ShowCredits()
    {
        if (creditsPanel != null)
        {
            mainMenuCanvas.SetActive(false);
            creditsPanel.SetActive(true);
        }
        else Debug.Log("Panel Credits belum dipasang!");
    }

    public void HideCredits()
    {
        if (creditsPanel != null && mainMenuCanvas != null)
        {
            creditsPanel.SetActive(false);
            mainMenuCanvas.SetActive(true);
        }
    }

    // --- LOGIKA CONTROLS ---
    public void ShowInfo()
    {
        if (infoPanel != null)
        {
            mainMenuCanvas.SetActive(false); // Tutup menu utama
            infoPanel.SetActive(true);       // Buka info
        }
        else Debug.Log("Info Panel belum dipasang!");
    }

    public void HideInfo()
    {
        if (infoPanel != null && mainMenuCanvas != null)
        {
            infoPanel.SetActive(false);      // Tutup info
            mainMenuCanvas.SetActive(true);  // Buka menu utama
        }
    }

    public void ResetGame()
    {
        SaveService.Instance?.Delete();
        GameSession.ForceNewGame = true;

        CloseReset();
    }

    public void ShowResetConfirm()
    {
        if (resetConfirmPanel != null)
        {
            resetConfirmPanel.SetActive(true);
        }
    }

    public void CloseReset()
    {
        if (resetConfirmPanel != null)
        {
            resetConfirmPanel.SetActive(false);
        }
    }
}
