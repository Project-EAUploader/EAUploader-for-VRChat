using UnityEditor;
using UnityEngine;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using System.IO;
using static EAUploader;
using static CustomPrefabUtility;
using static AssetImportProcessor;
using static EAUploaderEditorManager;
using static ShaderChecker;

public class CombinedInitialization
{
    /// <summary>
    /// Unity起動時からのEAUplaoderの処理はここから呼び出す
    /// </summary>
    [InitializeOnLoadMethod]
    private static void CombinedOnLoad()
    {
        EditorUtility.DisplayProgressBar("Initialization", "Initializing CustomPrefabUtility...", 0.0f);
        EnsurePrefabManagerExists();
        CustomPrefabUtilityOnUnityLoad();
        EditorUtility.DisplayProgressBar("Initialization", "Initializing EAUploaderEditorManager...", 0.2f);
        EAUploaderEditorManagerOnLoad();
        EditorUtility.DisplayProgressBar("Initialization", "Initializing SpriteImportProcessor...", 0.4f);
        AssetImportProcessorOnEditorLoad();
        EditorUtility.DisplayProgressBar("Initialization", "Initializing EAUploader...", 0.6f);
        EAUploaderInitializeOnLoad();
        EditorUtility.DisplayProgressBar("Initialization", "Initializing ShaderChecker...", 0.8f);
        ShaderCheckerOnLoad();
        EditorUtility.ClearProgressBar();
        
        EAUploader.ShowWindow();

        // UpdateManager.ShowWindow(); 検討中
    }

    private static void EnsurePrefabManagerExists()
    {
        string filePath = "Assets/EAUploader/PrefabManager.json";
        if (!File.Exists(filePath))
        {
            string directoryPath = Path.GetDirectoryName(filePath);
            if (!Directory.Exists(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
            }
            File.WriteAllText(filePath, "{}");
        }
    }

    private static void CustomPrefabUtilityOnUnityLoad()
    {
        OnCustomPrefabUtility();
    }

    private static void AssetImportProcessorOnEditorLoad()
    {
        var processor = new AssetImportProcessor();
        processor.OnEditorLoad();
    }

    private static void EAUploaderInitializeOnLoad()
    {
        OnEAUploader();
    }

    private static void EAUploaderEditorManagerOnLoad()
    {
        OnEditorManagerLoad();
    }

    private static void ShaderCheckerOnLoad()
    {
        OnShaderChecker();
    }
}
