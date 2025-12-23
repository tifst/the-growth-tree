using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    [Header("Audio Source")]
    public AudioSource musicSource; // Untuk BGM (Loop)
    public AudioSource sfxSource;   // Untuk SFX Sekali lewat (Win/Lose)

    [Header("Background Music Clips")]
    public AudioClip bgmMainMenu;
    public AudioClip bgmGameplay;

    [Header("Game Over Clips")]
    public AudioClip sfxWin;
    public AudioClip sfxLose;

    [Header("Volume Settings")]
    [Range(0f, 1f)] public float winVolume = 1f;
    [Range(0f, 1f)] public float fadeSpeed = 1f; // kecepatan fade (1 = normal)

    private Coroutine fadeCoroutine;

    void Awake()
    {
        // Singleton
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        string currentScene = SceneManager.GetActiveScene().name;
        PlayMusicForScene(currentScene);
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    // EVENT: Scene Loaded
    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        PlayMusicForScene(scene.name);
    }

    // LOGIKA BGM TIAP SCENE
    void PlayMusicForScene(string sceneName)
    {
        if (sceneName == "MainMenu")
        {
            PlayMusic(bgmMainMenu);
        }
        else if (sceneName == "MainScene")
        {
            PlayMusic(bgmGameplay);
        }
    }

    // FUNGSI BGM UTAMA
    public void PlayMusic(AudioClip clip)
    {
        if (musicSource == null || clip == null) return;

        musicSource.Stop();
        musicSource.clip = clip;
        musicSource.loop = true;
        musicSource.volume = 1f;
        musicSource.Play();
    }

    // MUSIK KETIKA MENANG / KALAH
    public void PlayWinMusic()
    {
        if (musicSource) musicSource.Stop();
        if (sfxSource && sfxWin)
            sfxSource.PlayOneShot(sfxWin, winVolume);
    }

    public void PlayLoseMusic()
    {
        if (musicSource) musicSource.Stop();
        if (sfxSource && sfxLose)
            sfxSource.PlayOneShot(sfxLose);
    }

    // DIPAKAI SAAT PLAYER TEKAN CONTINUE (LANJUT MAIN)
    public void continueGameplay()
    {
        if (sfxSource.isPlaying) sfxSource.Stop();

        // Fade in gameplay BGM
        if (fadeCoroutine != null)
            StopCoroutine(fadeCoroutine);

        fadeCoroutine = StartCoroutine(FadeInMusic(bgmGameplay, 1f));
    }

    // DIPAKAI SAAT RETRY (ULANG GAME)
    public void retryGameplay()
    {
        if (sfxSource.isPlaying) sfxSource.Stop();

        if (fadeCoroutine != null)
            StopCoroutine(fadeCoroutine);

        PlayMusic(bgmGameplay);
    }

    // FUNGSI BANTUAN: FADE IN
    private IEnumerator FadeInMusic(AudioClip clip, float targetVolume)
    {
        if (musicSource == null || clip == null) yield break;

        musicSource.Stop();
        musicSource.clip = clip;
        musicSource.volume = 0f;
        musicSource.loop = true;
        musicSource.Play();

        while (musicSource.volume < targetVolume)
        {
            musicSource.volume += Time.unscaledDeltaTime * fadeSpeed;
            yield return null;
        }

        musicSource.volume = targetVolume;
    }
}