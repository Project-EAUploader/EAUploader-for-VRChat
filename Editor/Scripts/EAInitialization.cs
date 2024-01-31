#if !EA_ONBUILD
#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using System;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using static EAUploader;
using static CustomPrefabUtility;
using static AssetImportProcessor;
using static EAUploaderEditorManager;
using static ShaderChecker;
using VRC.SDK3A.Editor;

public class CombinedInitialization
{
    static bool isBuilding = false;
    private static readonly string sdkStatusFilePath = "Assets/EAUploader/SDKStatus.json";
    private static bool initializationPerformed = false;

    [InitializeOnLoadMethod]
    private static void InitializeOnLoad()
    {
        EditorApplication.update += WaitForIdle;
        RegisterSDKCallback();
        CheckBuildStatus();
    }

    private static void WaitForIdle()
    {
        if (!EditorApplication.isCompiling && !EditorApplication.isPlayingOrWillChangePlaymode)
        {
            // エディタがアイドル状態になったら、初期化処理を実行
            if (!initializationPerformed && !IsBuilding())
            {
                PerformInitialization();
                initializationPerformed = true;

                // イベントを解除
                EditorApplication.update -= WaitForIdle;
            }
        }
    }

    private static void RegisterSDKCallback()
    {
        VRCSdkControlPanel.OnSdkPanelEnable += AddBuildHook;
    }

    private static void AddBuildHook(object sender, EventArgs e)
    {
        if (VRCSdkControlPanel.TryGetBuilder<IVRCSdkAvatarBuilderApi>(out var builder))
        {
            builder.OnSdkBuildStart += OnBuildStart;
            builder.OnSdkBuildFinish += OnBuildFinish;
        }
    }

    private static void OnBuildStart(object sender, object target)
    {
        UpdateSDKStatus(true);
    }

    private static void OnBuildFinish(object sender, object target)
    {
        UpdateSDKStatus(false);
    }

    private static void UpdateSDKStatus(bool isBuilding)
    {
        var sdkStatus = new { IsBuilding = isBuilding };
        File.WriteAllText(sdkStatusFilePath, JsonConvert.SerializeObject(sdkStatus));
    }

    private static void CheckBuildStatus()
    {
        if (File.Exists(sdkStatusFilePath))
        {
            string json = File.ReadAllText(sdkStatusFilePath);
            var sdkStatus = JsonConvert.DeserializeObject<dynamic>(json);
            isBuilding = sdkStatus != null && sdkStatus.IsBuilding;
        }
    }

    private static void PerformInitialization()
    {
        if (IsBuilding())
        {
            Debug.Log("Initialization skipped because a build is in progress.");
            return;
        }

        // プロジェクト内の全てのプレハブを取得
        string[] prefabGuids = AssetDatabase.FindAssets("t:Prefab", new[] { "Assets" });
        foreach (string guid in prefabGuids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            FileInfo fileInfo = new FileInfo(path);
            if (IsFileLocked(fileInfo))
            {
                Debug.Log($"Prefab at path {path} is currently locked. Initialization delayed.");
                return; // ロックされているプレハブがある場合初期化を中断
            }
        }

        EditorUtility.DisplayProgressBar("Initialization", "Initializing CustomPrefabUtility...", 0.0f);
        EnsurePrefabManagerExists();
        EditorUtility.DisplayProgressBar("Initialization", "Initializing EAUploader...", 0.2f);
        EAUploaderInitializeOnLoad();
        EditorUtility.DisplayProgressBar("Initialization", "Initializing EAUploaderEditorManager...", 0.4f);
        EAUploaderEditorManagerOnLoad();
        CustomPrefabUtilityOnUnityLoad();
        EditorUtility.DisplayProgressBar("Initialization", "Initializing SpriteImportProcessor...", 0.6f);
        AssetImportProcessorOnEditorLoad();
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

    private static bool IsFileLocked(FileInfo file)
    {
        try
        {
            using (FileStream stream = file.Open(FileMode.Open, FileAccess.Read, FileShare.None))
            {
                stream.Close();
            }
        }
        catch (IOException)
        {
            // ファイルがロックされているか、別のエラーが発生
            return true;
        }

        // ファイルはロックされていない
        return false;
    }

    private static bool IsBuilding()
    {
        if (File.Exists(sdkStatusFilePath))
        {
            string json = File.ReadAllText(sdkStatusFilePath);
            var sdkStatus = JsonConvert.DeserializeObject<dynamic>(json);
            return sdkStatus != null && sdkStatus.IsBuilding;
        }
        return false;
    }
}
#endif
#endif