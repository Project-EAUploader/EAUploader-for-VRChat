using UnityEditor;
using UnityEngine;
using System.IO;
using System.Linq;
using System.Collections.Generic;

internal class Prefabop
{
    // Refreshes the list of prefabs in the Assets directory.
    public static void Refresh(out string[] prefabPaths, out string[] prefabNames)
    {
        prefabPaths = Directory.GetFiles("Assets/", "*.prefab", SearchOption.AllDirectories);
        prefabNames = prefabPaths.Select(x => Path.GetFileNameWithoutExtension(x)).ToArray();
    }

    // Deletes all prefab instances in the scene and sets up the selected prefab.
    public static GameObject SetUpPrefab(string path)
    {
        // Delete all existing prefabs in the scene
        GameObject[] allGameObjects = UnityEngine.Object.FindObjectsOfType<GameObject>();
        foreach (GameObject go in allGameObjects)
        {
            if (PrefabUtility.GetCorrespondingObjectFromSource(go) != null)
            {
                // If the GameObject is an instance of a prefab, delete it
                UnityEngine.Object.DestroyImmediate(go);
            }
        }

        // Now, create and set up the selected prefab
        GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
        if (prefab != null)
        {
            GameObject instance = PrefabUtility.InstantiatePrefab(prefab) as GameObject;
            if (instance != null)
            {
                instance.transform.position = Vector3.zero;
                return instance;
            }
            else
            {
                Debug.LogError("Failed to instantiate prefab: " + path);
                return null;
            }
        }
        else
        {
            Debug.LogError("Failed to load prefab: " + path);
            return null;
        }
    }
}
