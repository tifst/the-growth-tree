using UnityEngine;
using UnityEngine.SceneManagement;

public class GameResultUI : MonoBehaviour
{
    [Header("Nama Scene")]
    public string mainMenuScene = "MainMenu";
    public string gameplayScene = "MainScene"; // dipakai kalau retry

    // TOMBOL RETRY (ulangi dari awal)
    public void OnRetryButton()
    {
        Time.timeScale = 1f;
        if (GameManager.Instance != null)
            GameManager.Instance.ResetGameData();

        AudioManager.Instance.retryGameplay();
        SceneManager.LoadScene(gameplayScene);
    }

    // TOMBOL MAIN MENU
    public void OnMenuButton()
    {
        Time.timeScale = 1f;
        AudioManager.Instance.PlayMusic(AudioManager.Instance.bgmMainMenu);
        SceneManager.LoadScene(mainMenuScene);
    }

    // TOMBOL CONTINUE (tidak reload scene, lanjut saja)
    public void OnContinueButton()
    {
        // 🔊 Ganti musik jika perlu
        AudioManager.Instance.continueGameplay();

        // 🔄 Hilangkan panel kemenangan
        gameObject.SetActive(false);

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