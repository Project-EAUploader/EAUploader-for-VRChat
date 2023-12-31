using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using System.IO;


public class ShaderChecker
{
    public static void OnShaderChecker()
    {
        EditorApplication.update += CheckShadersOnStartup;
    }

    private static void CheckShadersOnStartup()
    {
        EditorApplication.update -= CheckShadersOnStartup;
        CheckShadersInPrefabs();
    }

    public static void CheckShadersInPrefabs()
    {
        string[] allPrefabPaths = AssetDatabase.FindAssets("t:Prefab");
        HashSet<string> problematicPrefabs = new HashSet<string>();

        foreach (string prefabPath in allPrefabPaths)
        {
            string assetPath = AssetDatabase.GUIDToAssetPath(prefabPath);
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);

            Renderer[] renderers = prefab.GetComponentsInChildren<Renderer>(true);
            foreach (Renderer renderer in renderers)
            {
                foreach (Material material in renderer.sharedMaterials)
                {
                    if (material != null && material.shader != null)
                    {
                        if (material.shader.name == "Hidden/InternalErrorShader" || !ShaderExists(material.shader.name))
                        {
                            problematicPrefabs.Add(prefab.name);

                        }
                    }
                }
            }
        }

        if (problematicPrefabs.Count > 0)
        {
            string message = $"Prefabs with missing or problematic shaders:\n{string.Join("\n", problematicPrefabs)}\n\nPlease check the guide to obtaining this Prefab.";
            if (EditorUtility.DisplayDialogComplex("Shader Issues Found", message, "OK", "Why am I seeing this?", "") == 1)
            {
                Application.OpenURL("https://uslog.tech/eauploader");
            }
        }
    }

    private static bool ShaderExists(string shaderName)
    {
        return Shader.Find(shaderName) != null;
    }

    private class MyAssetPostprocessor : AssetPostprocessor
    {
        static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths)
        {
            foreach (string path in importedAssets)
            {
                if (path.EndsWith(".prefab"))
                {
                    CheckShadersInPrefabs();
                    break;
                }
            }
        }
    }
}
