using UnityEngine;
using UnityEngine.SceneManagement;

public class GameResultUI : MonoBehaviour
{
    [Header("Nama Scene")]
    public string mainMenuScene = "MainMenu";
    public string gameplayScene = "MainScene"; // dipakai kalau retry

    [SerializeField] private GameObject winPanel;
    [SerializeField] private GameObject losePanel;

    void Awake()
    {
        winPanel.SetActive(false);
        losePanel.SetActive(false);
    }

    void OnEnable()
    {
        GameManager.OnGameOver += ShowResult;
    }

    void OnDisable()
    {
        GameManager.OnGameOver -= ShowResult;
    }

    void ShowResult(bool isWin)
    {
        if (isWin)
            winPanel.SetActive(true);
        else
            losePanel.SetActive(true);
    }

    // TOMBOL RETRY (ulangi dari awal)
    public void OnRetryButton()
    {
        Time.timeScale = 1f;
    
        GameSession.ForceNewGame = true;
        SceneLoader.Instance.LoadGameScene(gameplayScene);

        AudioManager.Instance.retryGameplay();
    }

    // TOMBOL MAIN MENU
    public void OnMenuButton()
    {
        Time.timeScale = 1f;
        AudioManager.Instance.PlayMusic(AudioManager.Instance.bgmMainMenu);
        SceneLoader.Instance.LoadMainMenu(mainMenuScene);
    }

    // TOMBOL CONTINUE (tidak reload scene, lanjut saja)
    public void OnContinueButton()
    {
        // 🔊 Ganti musik jika perlu
        AudioManager.Instance.continueGameplay();

        // 🔄 Hilangkan panel kemenangan
        winPanel.SetActive(false);

        // 🕹️ Balik ke gameplay normal
        Time.timeScale = 1f;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        // Jika ada sistem kontrol player, aktifkan lagi
        if (GameManager.Instance != null)
        {
            PlayerLockManager.Instance.Unlock();
        }

        Debug.Log("Lanjut main tanpa reset data...");
    }
}