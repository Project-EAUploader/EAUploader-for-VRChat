using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace EAUploader
{
    public class ShaderChecker
    {
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
                                break;
                            }
                        }
                    }
                }
            }

            if (problematicPrefabs.Count > 0)
            {
                string msg1 = T7e.Get("Prefabs with missing or problematic shaders:");
                string msg2 = T7e.Get("Please confirm the required shaders from the avatar distributor.");
                string msg3 = T7e.Get("Why am I seeing this?");
                string message = $"{msg1}\n{string.Join("\n", problematicPrefabs)}\n\n{msg2}";
                if (EditorUtility.DisplayDialogComplex("Shader Issues Found", message, "OK", msg3, "") == 1)
                {
                    Application.OpenURL("https://www.uslog.tech/eauploader-forum/__q-a/siedagajian-tukaranaiera");
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
}