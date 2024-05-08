using EAUploader.CustomPrefabUtility;
using UnityEditor;
using UnityEngine;

namespace EAUploader
{
    public class AssetImportProcessor : AssetPostprocessor
    {
        private static void OnPostprocessAllAssets(
            string[] importedAssets,
            string[] deletedAssets,
            string[] movedAssets,
            string[] movedFromAssetPaths)
        {
            foreach (var asset in importedAssets)
            {
                if (asset.EndsWith(".prefab"))
                {
                    var path = AssetDatabase.GetAssetPath(AssetDatabase.LoadAssetAtPath<GameObject>(asset));
                    ShaderChecker.CheckShadersInPrefabs(path);
                    PrefabManager.ImportPrefab(path);
                }
            }
        }
    }
}