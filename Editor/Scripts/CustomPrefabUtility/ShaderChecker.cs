using UnityEditor;
using UnityEngine;

namespace EAUploader.CustomPrefabUtility
{
    public class ShaderChecker
    {
        public static void CheckShaders()
        {
            string[] guids = AssetDatabase.FindAssets("t:Prefab", new[] { "Assets" });

            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                CheckShadersInPrefabs(path);
            }
        }

        public static bool CheckAvatarHasShader(GameObject avatar)
        {
            Renderer[] renderers = avatar.GetComponentsInChildren<Renderer>(true);
            return !ContainsMissingOrProblematicShaderMaterial(renderers);
        }

        public static void CheckShadersInPrefabs(string path = null)
        {
            if (path == null) return;
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            Renderer[] renderers = prefab.GetComponentsInChildren<Renderer>(true);

            if (ContainsMissingOrProblematicShaderMaterial(renderers))
            {
                DisplayShaderIssueMessageBox(prefab.name);
            }
        }

        private static bool ContainsMissingOrProblematicShaderMaterial(Renderer[] renderers)
        {
            foreach (Renderer renderer in renderers)
            {
                foreach (Material material in renderer.sharedMaterials)
                {
                    if (material?.shader?.name == "Hidden/InternalErrorShader" || !ShaderExists(material?.shader?.name))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        private static void DisplayShaderIssueMessageBox(string prefabName)
        {
            string msg1 = T7e.Get("Prefabs with missing or problematic shaders:");
            string msg2 = T7e.Get("Please confirm the required shaders from the avatar distributor.");
            string msg3 = T7e.Get("Why am I seeing this?");
            string message = $"{msg1}\n{prefabName}\n\n{msg2}";

            if (EditorUtility.DisplayDialogComplex(T7e.Get("Shader Issues Found"), message, "OK", msg3, "") == 1)
            {
                Application.OpenURL("https://www.uslog.tech/eauploader-forum/__q-a/siedagajian-tukaranaiera");
            }
        }

        private static bool ShaderExists(string shaderName)
        {
            return Shader.Find(shaderName) != null;
        }
    }
}