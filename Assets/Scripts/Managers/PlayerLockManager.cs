using System.Collections.Generic;
using UnityEngine;

public class PlayerLockManager : MonoBehaviour
{
    public static PlayerLockManager Instance;

    public GameObject player;
    public GameObject cameraObject;

    private List<MonoBehaviour> allScripts = new();

    void Awake()
    {
        Instance = this;

        // Ambil semua script dari Player
        allScripts.AddRange(player.GetComponentsInChildren<MonoBehaviour>(true));

        // Ambil semua script dari Camera
        allScripts.AddRange(cameraObject.GetComponentsInChildren<MonoBehaviour>(true));
    }

    public void Lock()
    {
        foreach (MonoBehaviour mb in allScripts)
        {
            if (mb == this) continue;
            mb.enabled = false;
        }

        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }

    public void Unlock()
    {
        foreach (MonoBehaviour mb in allScripts)
        {
            if (mb == this) continue;
            mb.enabled = true;
        }

        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }
}