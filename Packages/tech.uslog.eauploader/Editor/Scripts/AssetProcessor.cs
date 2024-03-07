using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace EAUploader
{
    public class AssetImportProcessor : AssetPostprocessor
    {
        private static readonly List<string> targetFolderPaths = new List<string>
    {
        "Assets/EAUploader/MarketThumbnails",
        "Assets/EAUploader/MyList",
        "Packages/tech.uslog.eauploader/Editor/Resources/icons"
    };

        static void OnPostprocessAllAssets(
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
                    CustomPrefabUtility.PrefabManager.ImportPrefab(path);
                }
            }
        }
    }
}