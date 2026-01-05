using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseMenuManager : MonoBehaviour
{
    public static PauseMenuManager Instance;

    [Header("UI References")]
    public GameObject pauseMenuPanel;

    [Header("Scene Settings")]
    public string mainMenuScene = "MainMenu";

    [HideInInspector] 
    public bool uiIsBlockingPause = false;  // panel lain bisa blok ESC

    private bool isPaused = false;
    public bool IsPaused => isPaused;
    private float inputLockTimer = 0f; // delay anti spam ESC

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        if (pauseMenuPanel != null)
            pauseMenuPanel.SetActive(false);

        Time.timeScale = 1f;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        // Hitung mundur delay input ESC
        if (inputLockTimer > 0f)
            inputLockTimer -= Time.unscaledDeltaTime;

        // Jika ESC masih terkunci → tidak boleh pause
        if (inputLockTimer > 0f) return;

        // Jika ada UI lain terbuka → pause menu diblock
        if (uiIsBlockingPause) return;
    }

    // ====== PUBLIC FUNCTIONS ======
    public void TogglePause()
    {
        if (isPaused)
            ResumeGame();
        else
            PauseGame();
    }

    public void LockInputFor(float seconds)
    {
        inputLockTimer = seconds;
    }

    public void ResumeGame()
    {
        isPaused = false;

        if (pauseMenuPanel != null)
            pauseMenuPanel.SetActive(false);
        
        Time.timeScale = 1f;

        PlayerLockManager.Instance.Unlock();
    }

    public void PauseGame()
    {
        SaveLoadSystem.Instance.SaveGame();

        isPaused = true;

        if (pauseMenuPanel != null)
            pauseMenuPanel.SetActive(true);

        Time.timeScale = 0f;

        PlayerLockManager.Instance.Lock();
    }

    public void QuitToMainMenu()
    {
        Time.timeScale = 1f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        SceneLoader.Instance.LoadMainMenu(mainMenuScene);
    }
}