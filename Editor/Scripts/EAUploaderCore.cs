using EAUploader.CustomPrefabUtility;
using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace EAUploader
{
    [InitializeOnLoad]
    public class EAUploaderCore
    {
        [Serializable]
        private class PackageJson
        {
            public string version;
        }

        public static event Action<string> SelectedPrefabPathChanged;

        private static string _selectedPrefabPath = null;
        public static string selectedPrefabPath
        {
            get { return _selectedPrefabPath; }
            set
            {
                if (_selectedPrefabPath != value)
                {
                    _selectedPrefabPath = value;
                    SelectedPrefabPathChanged?.Invoke(value);
                }
            }
        }

        private const string EAUPLOADER_ASSET_PATH = "Assets/EAUploader";
        private static bool initializationPerformed = false;
        public static bool HasVRM = false;

        static EAUploaderCore()
        {
            Debug.Log("EAUploader is starting...");
            EditorApplication.update += OnEditorUpdate;
        }

        private static void OnEditorUpdate()
        {
            if (!EditorApplication.isCompiling && !EditorApplication.isPlayingOrWillChangePlaymode && !initializationPerformed)
            {
                EditorApplication.update -= OnEditorUpdate; // Unregister after initialization
                initializationPerformed = true;
                PerformInitialization();
            }
        }

        private static void PerformInitialization()
        {
            InitializeEAUploader();
            EAUploaderEditorManager.OnEditorManagerLoad();
            ShaderChecker.CheckShaders();
            PrefabManager.Initialize();
            CheckIsVRMAvailable();
            OpenEAUploaderWindow();
        }

        private static void CheckIsVRMAvailable()
        {
            try
            {
                string manifestPath = "Packages/packages-lock.json";
                if (File.Exists(manifestPath))
                {
                    string manifestContent = File.ReadAllText(manifestPath);
                    HasVRM = manifestContent.Contains("\"com.vrmc.univrm\"") && manifestContent.Contains("\"jp.pokemori.vrm-converter-for-vrchat\"");
                }
                else
                {
                    Debug.LogError("Manifest file not found.");
                }
            }
            catch (Exception e)
            {
                Debug.LogError("Failed to check for packages: " + e.Message);
            }
        }

        private static void InitializeEAUploader()
        {
            // Create New folder
            if (!AssetDatabase.IsValidFolder(EAUPLOADER_ASSET_PATH))
            {
                AssetDatabase.CreateFolder("Assets", "EAUploader");
            }

            string prefabManagerPath = $"{EAUPLOADER_ASSET_PATH}/PrefabManager.json";
            if (!File.Exists(prefabManagerPath))
            {
                File.WriteAllText(prefabManagerPath, "{}");
            }
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
                bool result = EditorApplication.ExecuteMenuItem("EAUploader/Open EAUploader");
                Debug.Log($"EAUploader opened: {result}");
            }
            else
            {
                Debug.Log("Focusing on existing EAUploader window.");
                windows[0].Focus();
            }
        }

        public static string GetVersion()
        {
            // Get version from package.json
            string packageJsonPath = "Packages/tech.uslog.eauploader/package.json";
            if (File.Exists(packageJsonPath))
            {
                string packageJson = File.ReadAllText(packageJsonPath);
                return T7e.Get("Version: ") + JsonUtility.FromJson<PackageJson>(packageJson).version;
            }
            else
            {
                return T7e.Get("Version: ") + "Unknown";
            }
        }

        [MenuItem("EAUploader/Reload")]
        public static void ReloadSDK()
        {
            selectedPrefabPath = null;
            EAUploaderEditorManager.OnEditorManagerLoad();
            ShaderChecker.CheckShadersInPrefabs();
            CustomPrefabUtility.PrefabManager.Initialize();
            CheckIsVRMAvailable();
        }
    }
}
