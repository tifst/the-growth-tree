using UnityEngine;
using System.IO;

public static class GameSession
{
    public static bool ForceNewGame = false;
}

public class SaveService : MonoBehaviour
{
    public static SaveService Instance;

    private const string SAVE_KEY = "SAVE_GAME_DATA";
    private string SavePath => Application.persistentDataPath + "/save.json";

    void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public bool HasSave()
    {
#if UNITY_WEBGL
        return PlayerPrefs.HasKey(SAVE_KEY);
#else
        return File.Exists(SavePath);
#endif
    }

    public void Write(string json)
    {
#if UNITY_WEBGL
        PlayerPrefs.SetString(SAVE_KEY, json);
        PlayerPrefs.Save();
#else
        File.WriteAllText(SavePath, json);
#endif
    }

    public string Read()
    {
#if UNITY_WEBGL
        return PlayerPrefs.GetString(SAVE_KEY, "");
#else
        return File.Exists(SavePath) ? File.ReadAllText(SavePath) : "";
#endif
    }

    public void Delete()
    {
#if UNITY_WEBGL
        PlayerPrefs.DeleteKey(SAVE_KEY);
        PlayerPrefs.Save();
#else
        if (File.Exists(SavePath))
            File.Delete(SavePath);
#endif

        Debug.Log("💥 Save data deleted");
    }
}