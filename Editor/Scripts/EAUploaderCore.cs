using System;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace EAUploader_beta
{
    internal class EAUploaderCore
    {
        public static bool HasVRM = false;

        [InitializeOnLoadMethod]
        private static void Initialize()
        {
            CheckIsVRMAvailable();
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
    }
}
