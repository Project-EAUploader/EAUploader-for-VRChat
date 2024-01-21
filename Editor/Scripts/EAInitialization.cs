using UnityEditor;
using UnityEngine;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using System.IO;
using System.Linq;
using static EAUploader;
using static CustomPrefabUtility;
using static AssetImportProcessor;
using static EAUploaderEditorManager;
using static ShaderChecker;
using VRC.SDK3A.Editor;

[InitializeOnLoad]
public class CombinedInitialization
{
    static bool isBuilding = false;

    static CombinedInitialization()
    {
        CombinedOnLoad();
    }

    [InitializeOnLoadMethod]
    private static void CombinedOnLoad()
    {
        RegisterBuildEvents();
        if (!isBuilding)
        {
            PerformInitialization();
        }
    }

    private static void RegisterBuildEvents()
    {
        if (VRCSdkControlPanel.TryGetBuilder<IVRCSdkAvatarBuilderApi>(out var builder))
        {
            builder.OnSdkBuildStart += OnBuildStart;
            builder.OnSdkBuildFinish += OnBuildFinish;
        }
    }

    private static void OnBuildStart(object sender, object target)
    {
        isBuilding = true;
    }

    private static void OnBuildFinish(object sender, object target)
    {
        isBuilding = false;
        PerformInitialization();
    }

    private static void PerformInitialization()
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
        OpenEAUploaderWindow();
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

    private static void OpenEAUploaderWindow()
    {
        // 既存のウィンドウを検索
        var windows = Resources.FindObjectsOfTypeAll<EditorWindow>()
            .Where(window => window.GetType().Name == "EAUploader").ToList();

        Debug.Log($"EAUploader windows found: {windows.Count}");

        if (windows.Count == 0)
        {
            Debug.Log("Attempting to open EAUploader...");
            bool result = EditorApplication.ExecuteMenuItem("EAUploader/MainWindow");
            Debug.Log($"EAUploader opened: {result}");
        }
        else
        {
            Debug.Log("Focusing on existing EAUploader window.");
            windows[0].Focus();
        }
    }
}
