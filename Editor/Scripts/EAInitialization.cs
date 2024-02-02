#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using System;
using System.IO;
using System.Linq;
using static CustomPrefabUtility;
using static EAUploaderEditorManager;
using static ShaderChecker;

public class EAInitialization
{
    private static bool initializationPerformed = false;
    public static bool onBuild = false;

    [InitializeOnLoadMethod]
    private static void InitializeOnLoad()
    {
        EditorApplication.update += WaitForIdle;
    }

    private static void WaitForIdle()
    {
        if (!EditorApplication.isCompiling && !EditorApplication.isPlayingOrWillChangePlaymode)
        {
            // エディタがアイドル状態になったら、初期化処理を実行
            if (!initializationPerformed)
            {
                // イベントを解除
                EditorApplication.update -= WaitForIdle;
                initializationPerformed = true;

                PerformInitialization(); 
            }
        }
    }

    private static void PerformInitialization()
    {
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
        EditorUtility.DisplayProgressBar("Initialization", "Initializing EAUploaderEditorManager...", 0.4f);
        EAUploaderEditorManagerOnLoad();
        // CustomPrefabUtilityOnUnityLoad();
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
     
    [Serializable]
    public class SDKStatus
    {
        public bool IsBuilding;
    }
}
#endif