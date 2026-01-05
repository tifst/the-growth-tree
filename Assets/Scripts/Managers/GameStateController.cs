using UnityEngine;

public class GameStateController : MonoBehaviour
{
    void OnEnable()
    {
        GameManager.OnGameOver += HandleGameOver;
    }

    void OnDisable()
    {
        GameManager.OnGameOver -= HandleGameOver;
    }

    void HandleGameOver(bool isWin)
    {
        Time.timeScale = 0f;

        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;

        if (isWin)
            AudioManager.Instance?.PlayWinMusic();
        else
            AudioManager.Instance?.PlayLoseMusic();
    }
}