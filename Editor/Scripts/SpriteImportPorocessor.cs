using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using System.IO;
using System.Linq;

public class SpriteImportProcessor : AssetPostprocessor
{
    private static readonly List<string> targetFolderPaths = new List<string>
    {
        "Packages/com.sabuworks.eauploader/Editor/Resources/PrefabPreviews",
        "Packages/com.sabuworks.eauploader/Editor/Resources/Info",
        "com.subuworks.eauploader/Editor/Resources/icons"
    };

    void OnPreprocessTexture()
    {
        foreach (var path in targetFolderPaths)
        {
            if (assetPath.Contains(path))
            {
                var importer = (TextureImporter)assetImporter;
                importer.textureType = TextureImporterType.Sprite;
                break;
            }
        }
    }

    void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths)
    {
        bool assetsChanged = importedAssets.Any(path => targetFolderPaths.Any(targetPath => path.Contains(targetPath))) ||
                            deletedAssets.Any(path => targetFolderPaths.Any(targetPath => path.Contains(targetPath))) ||
                            movedAssets.Any(path => targetFolderPaths.Any(targetPath => path.Contains(targetPath)));

        if (assetsChanged)
        {
            CustomPrefabUtility.Processor(importedAssets, deletedAssets, movedAssets, movedFromAssetPaths);
            EABuilder.avatarsListUpdated = true; // アバターリストが更新されたことを示す
        }
    }

    // [InitializeOnLoadMethod]
    public static void OnEditorLoad()
    {
        foreach (var path in targetFolderPaths)
        {
            AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
        }
    }
}
