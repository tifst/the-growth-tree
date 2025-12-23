using UnityEngine;
using UnityEditor;

public class FindNonConvexMeshColliders
{
    [MenuItem("Tools/Find Non-Convex MeshColliders")]
    static void Find()
    {
        // API baru Unity: FindObjectsByType
        MeshCollider[] colliders = Object.FindObjectsByType<MeshCollider>(
            FindObjectsSortMode.None
        );

        int count = 0;

        foreach (var mc in colliders)
        {
            if (!mc.convex)
            {
                Debug.Log("Non-Convex MeshCollider: " + mc.gameObject.name, mc.gameObject);
                count++;
            }
        }

        Debug.Log($"Total NON-CONVEX found: {count}");
    }
}