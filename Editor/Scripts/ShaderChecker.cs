using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using static labels;

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
                string msg1 = Get(118);
                string msg2 = Get(119);
                string msg3 = Get(120);
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