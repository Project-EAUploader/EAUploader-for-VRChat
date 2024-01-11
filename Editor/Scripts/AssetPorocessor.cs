using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using System.IO;
using System.Linq;

public class AssetImportProcessor : AssetPostprocessor
{
    private static readonly List<string> targetFolderPaths = new List<string>
    {
        "Assets/EAUploader/PrefabPreviews",
        "Assets/EAUploader/MarketThumbnails",
        "Assets/EAUploader/MyList",
        "Packages/tech.uslog.eauploader/Editor/Resources/Info",
        "Packages/tech.uslog.eauploader/Editor/Resources/icons"
    };

    public void OnEditorLoad()
    {
        OnPreprocessTexture();
    }

    void OnPreprocessTexture()
    {
        foreach (var path in targetFolderPaths)
        {
            if (assetPath.Contains(path))
            {
                var importer = (TextureImporter)assetImporter;
                importer.textureType = TextureImporterType.Sprite;
            }
        }
    }

    static void OnPostprocessAllAssets(
        string[] importedAssets, 
        string[] deletedAssets, 
        string[] movedAssets, 
        string[] movedFromAssetPaths)
    {
        ProcessPrefabChanges(importedAssets);
        ProcessPrefabChanges(deletedAssets);
        ProcessPrefabChanges(movedAssets);
        ProcessUnityPackageImports(importedAssets);
    }

    static void ProcessUnityPackageImports(string[] importedAssets)
    {
        foreach (string asset in importedAssets)
        {
            if (Path.GetExtension(asset) == ".unitypackage")
            {
                RegisterNewPrefabsFromPackage(asset);
            }
        }
    }

    public static void ProcessPrefabChanges(string[] assetPaths)
    {
        foreach (string path in assetPaths)
        {
            if (path.EndsWith(".prefab"))
            {
                CustomPrefabUtility.Processor(new string[] { path }, new string[0], new string[0], new string[0]);
                EABuilder.avatarsListUpdated = true;
            }
        }
        Manager.RefreshPrefabList();
    }

    static void RegisterNewPrefabsFromPackage(string packagePath)
    {
        var beforeImport = GetAllPrefabPathsInProject();
        EditorApplication.update += () => ProcessImportedPrefabs(packagePath, beforeImport);
    }

    static void ProcessImportedPrefabs(string packagePath, HashSet<string> beforeImport)
    {
        var afterImport = GetAllPrefabPathsInProject();
        var newPrefabs = afterImport.Except(beforeImport);
        foreach (var prefabPath in newPrefabs)
        {
            CustomPrefabUtility.RegisterNewPrefab(prefabPath);
        }
        EditorApplication.update -= () => ProcessImportedPrefabs(packagePath, beforeImport);
    }

    static HashSet<string> GetAllPrefabPathsInProject()
    {
        var prefabPaths = new HashSet<string>();
        var guids = AssetDatabase.FindAssets("t:Prefab", new[] { "Assets" });
        foreach (var guid in guids)
        {
            var path = AssetDatabase.GUIDToAssetPath(guid);
            prefabPaths.Add(path);
        }
        return prefabPaths;
    }
}
