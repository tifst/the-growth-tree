using UnityEditor;
using UnityEngine;
using System.Collections.Generic;

public class RemapMaterial
{
    const string MESH_FOLDER = "Assets/npc_casual_set_00/Mesh";
    const string URP_MAT_FOLDER = "Assets/npc_casual_set_00/MaterialsURP";

    [MenuItem("Tools/NPC Casual Set/Remap Mesh Materials to URP")]
    static void Remap()
    {
        // Load semua material URP
        Dictionary<string, Material> urpMaterials = new Dictionary<string, Material>();

        string[] matGuids = AssetDatabase.FindAssets("t:Material", new[] { URP_MAT_FOLDER });
        foreach (string guid in matGuids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            Material mat = AssetDatabase.LoadAssetAtPath<Material>(path);

            if (mat != null && !urpMaterials.ContainsKey(mat.name))
                urpMaterials.Add(mat.name, mat);
        }

        if (urpMaterials.Count == 0)
        {
            Debug.LogError("Tidak ada material URP ditemukan!");
            return;
        }

        // Scan semua asset di folder mesh
        string[] meshGuids = AssetDatabase.FindAssets("t:GameObject", new[] { MESH_FOLDER });

        int replacedCount = 0;

        foreach (string guid in meshGuids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);

            if (prefab == null) continue;

            bool changed = false;
            Renderer[] renderers = prefab.GetComponentsInChildren<Renderer>(true);

            foreach (Renderer r in renderers)
            {
                Material[] mats = r.sharedMaterials;

                for (int i = 0; i < mats.Length; i++)
                {
                    if (mats[i] == null) continue;

                    if (urpMaterials.TryGetValue(mats[i].name, out Material urpMat))
                    {
                        mats[i] = urpMat;
                        replacedCount++;
                        changed = true;
                    }
                }

                r.sharedMaterials = mats;
            }

            if (changed)
                EditorUtility.SetDirty(prefab);
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log($"Remap selesai. Total material diganti: {replacedCount}");
    }
}
