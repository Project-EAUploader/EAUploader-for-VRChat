using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace EAUploader_beta
{
    internal class EAUploaderCore
    {
        public static string selectedPrefabPath = null;
        public static bool HasVRM = false;

        [InitializeOnLoadMethod]
        private static void Initialize()
        {
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

        private static void OpenEAUploaderWindow()
        {
            // 既存のウィンドウを検索
            var windows = Resources.FindObjectsOfTypeAll<EditorWindow>()
                .Where(window => window.GetType().Name == "EAUploader_Beta").ToList();

            Debug.Log($"EAUploader windows found: {windows.Count}");

            if (windows.Count == 0)
            {
                Debug.Log("Attempting to open EAUploader...");
                bool result = EditorApplication.ExecuteMenuItem("EAUploader_beta/Open EAUploader");
                Debug.Log($"EAUploader opened: {result}");
            }
            else
            {
                Debug.Log("Focusing on existing EAUploader window.");
                windows[0].Focus();
            }
        }
    }
}
