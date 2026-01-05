using UnityEngine;
using UnityEditor;

public class FindMissingScripts
{
    [MenuItem("Tools/Find Missing Scripts")]
    static void Find()
    {
        GameObject[] all = GameObject.FindObjectsByType<GameObject>(FindObjectsSortMode.None);

        foreach (var go in all)
        {
            Component[] comps = go.GetComponents<Component>();
            foreach (var c in comps)
            {
                if (c == null)
                {
                    Debug.LogError(
                        $"Missing script on: {go.name}",
                        go
                    );
                }
            }
        }
    }
}