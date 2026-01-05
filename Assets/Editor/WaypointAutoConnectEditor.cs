using UnityEngine;
using UnityEditor;

public class WaypointAutoConnectEditor
{
    [MenuItem("Tools/Waypoint/Auto Connect Neighbors")]
    static void AutoConnectWaypoints()
    {
        Waypoint[] all = Object.FindObjectsByType<Waypoint>(FindObjectsSortMode.None);

        if (all.Length == 0)
        {
            Debug.LogWarning("Tidak ada Waypoint di scene.");
            return;
        }

        Undo.RecordObjects(all, "Auto Connect Waypoints");

        foreach (var wp in all)
        {
            wp.AutoConnect(all);
            EditorUtility.SetDirty(wp);
        }

        Debug.Log($"Auto-connected {all.Length} waypoints.");
    }
}