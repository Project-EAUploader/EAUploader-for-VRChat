using System;
using System.IO;
using UnityEngine;

namespace EAUploader
{
    class Config
    {
        public static string SETTINGS_PATH = "Assets/EAUploader/settings.json";

        [Serializable]
        public class ConfigData
        {
            public string language;
            public bool isLibraryOpen;
        }

        public static void CreateDefaultSettings()
        {
            var newSettings = new ConfigData() { language = "ja", isLibraryOpen = true };
            SaveSettings(newSettings);
        }

        public static void SaveSettings(ConfigData settings)
        {
            try
            {
                string jsonContent = JsonUtility.ToJson(settings);
                File.WriteAllText(SETTINGS_PATH, jsonContent);
            }
            catch (IOException ex)
            {
                Debug.LogError($"Error saving language settings: {ex.Message}");
            }
        }

        public static ConfigData LoadSettings()
        {
            try
            {
                if (File.Exists(SETTINGS_PATH))
                {
                    string jsonContent = File.ReadAllText(SETTINGS_PATH);
                    return JsonUtility.FromJson<ConfigData>(jsonContent);
                }
                else
                {
                    return new ConfigData(); // Return default in case of missing file
                }
            }
            catch (IOException ex)
            {
                Debug.LogError($"Error loading language settings: {ex.Message}");
                return new ConfigData();
            }
        }


    }
}