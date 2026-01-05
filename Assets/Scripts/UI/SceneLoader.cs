using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class SceneLoader : MonoBehaviour
{
    public static SceneLoader Instance;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // LoadingSystem
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // MENU → GAMEPLAY
    public void LoadGameScene(string gameplayScene)
    {
        StartCoroutine(LoadGameRoutine(gameplayScene));
    }

    IEnumerator SmoothProgress(float from, float to, float speed = 0.5f)
    {
        float p = from;
        while (p < to)
        {
            p += Time.deltaTime * speed;
            LoadingScreenUI.Instance.SetProgress(p);
            UpdateLoadingText(p);
            yield return null;
        }
    }

    void UpdateLoadingText(float p)
    {
        if (p < 0.3f)
            LoadingScreenUI.Instance.SetText("Initializing Game...");
        else if (p < 0.6f)
            LoadingScreenUI.Instance.SetText("Loading Environment...");
        else if (p < 0.9f)
            LoadingScreenUI.Instance.SetText("Preparing World...");
        else if (p < 0.95f)
            LoadingScreenUI.Instance.SetText("Initializing Systems...");
        else if (p < 0.98f)
            LoadingScreenUI.Instance.SetText("Loading Save Data...");
        else
            LoadingScreenUI.Instance.SetText("Finalizing...");
    }

    IEnumerator LoadGameRoutine(string sceneName)
    {
        AudioListener.pause = true;

        LoadingScreenUI.Instance.Show("Loading Scene...");
        LoadingScreenUI.Instance.SetProgress(0f);

        AsyncOperation async = SceneManager.LoadSceneAsync(sceneName);
        async.allowSceneActivation = false;

        // 1️⃣ ENGINE LOAD (0 – 90%)
        while (async.progress < 0.9f)
        {
            float p = async.progress;
            LoadingScreenUI.Instance.SetProgress(p);
            UpdateLoadingText(p);
            yield return null;
        }

        // pastikan mentok di 90%
        LoadingScreenUI.Instance.SetProgress(0.9f);

        // 2️⃣ ACTIVATE SCENE
        async.allowSceneActivation = true;
        yield return null;

        while (SceneManager.GetActiveScene().name != sceneName)
            yield return null;

        // 3️⃣ WAIT GAMEPLAY SYSTEM (90 – 95%)
        yield return SmoothProgress(0.9f, 0.95f, 0.3f);
        yield return WaitForGameplaySystems();

        // 4️⃣ LOAD SAVE / NEW GAME (95 – 98%)
        if (GameSession.ForceNewGame)
        {
            LoadingScreenUI.Instance.SetText("Starting New Game...");
            GameManager.Instance.ResetAllGameState(true);
            GameSession.ForceNewGame = false;
        }
        else if (SaveService.Instance.HasSave())
        {
            LoadingScreenUI.Instance.SetText("Restoring Save Data...");
            yield return SaveLoadSystem.Instance.LoadGameRoutine();
        }
        else
        {
            LoadingScreenUI.Instance.SetText("Creating New World...");
            GameManager.Instance.ResetAllGameState(true);
        }

        FogController.Instance?.SetPollution(GameManager.Instance.pollution);
        
        yield return SmoothProgress(0.95f, 0.98f, 0.4f);

        // 5️⃣ FINALIZE (98 – 100%)
        yield return SmoothProgress(0.98f, 1f, 0.6f);

        yield return new WaitForSeconds(0.2f);
        LoadingScreenUI.Instance.Hide();
        AudioListener.pause = false;
    }

    // GAMEPLAY → MENU
    public void LoadMainMenu(string menuScene = "MainMenuScene")
    {
        StartCoroutine(LoadMenuRoutine(menuScene));
    }

    IEnumerator LoadMenuRoutine(string sceneName)
    {
        // 1️⃣ SAVE GAME
        if (SaveLoadSystem.Instance != null && !SaveLoadSystem.Instance.isLoading)
        {
            LoadingScreenUI.Instance.Show("Saving Game...");
            LoadingScreenUI.Instance.SetProgress(0.05f);
            SaveLoadSystem.Instance.SaveGame();
        }

        // 2️⃣ SHOW LOADING
        LoadingScreenUI.Instance.Show("Returning to Main Menu...");
        LoadingScreenUI.Instance.SetProgress(0.1f);

        AsyncOperation async = SceneManager.LoadSceneAsync(sceneName);
        async.allowSceneActivation = false;

        // 3️⃣ LOAD MENU SCENE (10 – 80%)
        while (async.progress < 0.9f)
        {
            float p = Mathf.Lerp(0.1f, 0.8f, async.progress / 0.9f);
            LoadingScreenUI.Instance.SetProgress(p);
            LoadingScreenUI.Instance.SetText("Loading Menu...");
            yield return null;
        }

        // 4️⃣ ACTIVATE SCENE (80 – 90%)
        LoadingScreenUI.Instance.SetText("Preparing Menu...");
        yield return SmoothProgress(0.8f, 0.9f, 0.4f);

        async.allowSceneActivation = true;
        yield return null;

        while (SceneManager.GetActiveScene().name != sceneName)
            yield return null;

        // 5️⃣ CLEANUP SYSTEM (90 – 95%)
        LoadingScreenUI.Instance.SetText("Cleaning Up...");
        yield return SmoothProgress(0.9f, 0.95f, 0.3f);

        // (optional) reset session flag
        GameSession.ForceNewGame = false;

        // 6️⃣ FINISH (95 – 100%)
        LoadingScreenUI.Instance.SetText("Ready");
        yield return SmoothProgress(0.95f, 1f, 0.5f);

        yield return new WaitForSeconds(0.2f);
        LoadingScreenUI.Instance.Hide();
    }

    // HELPER
    IEnumerator WaitForGameplaySystems()
    {
        float timeout = 5f;
        float t = 0f;

        while (SaveLoadSystem.Instance == null ||
            GameManager.Instance == null ||
            QuestManager.Instance == null)
        {
            t += Time.deltaTime;

            if (t > timeout)
            {
                Debug.LogError("⛔ Gameplay systems timeout!");
                yield break;
            }

            yield return null;
        }
    }
}