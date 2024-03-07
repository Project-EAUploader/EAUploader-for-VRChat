using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UIElements;

namespace EAUploader
{
    [Serializable]
    public class LanguageData
    {
        public string language;
    }

    public class LanguageInfo
    {
        public string name { get; set; }
        public string display { get; set; }
    }

    public static class LanguageUtility
    {
        private const string SETTINGS_PATH = "Assets/EAUploader/settings.json";
        private const string TRANSLATE_FOLDER_PATH = "Packages/tech.uslog.eauploader/Editor/Resources/Localization";

        public static string GetCurrentLanguage()
        {
            if (!File.Exists(SETTINGS_PATH))
            {
                CreateDefaultSettings();
            }

            try
            {
                string jsonContent = File.ReadAllText(SETTINGS_PATH);
                var settings = JsonUtility.FromJson<LanguageData>(jsonContent);
                return settings.language;
            }
            catch (IOException ex)
            {
                Debug.LogError($"Error reading language settings: {ex.Message}");
                return "en"; // Or some other default language
            }
        }

        public static void Localization(VisualElement root)
        {
            root.Query<TextElement>().ForEach((e) => e.text = T7e.Get(e.text));
        }

        public static void ChangeLanguage(string language)
        {
            var settings = LoadSettings();
            settings.language = language;
            SaveSettings(settings);
        }

        public static List<LanguageInfo> GetAvailableLanguages()
        {
            List<LanguageInfo> languages = new List<LanguageInfo>
            {
                new LanguageInfo { name = "en", display = "English" }
            };

            try
            {
                string[] jsonFiles = Directory.GetFiles(TRANSLATE_FOLDER_PATH, "*.json");
                foreach (string file in jsonFiles)
                {
                    string fileName = Path.GetFileNameWithoutExtension(file);
                    // Read Json file and get the language name
                    string jsonContent = File.ReadAllText(file);
                    var translations = JsonUtility.FromJson<T7e.LocalizationData>(jsonContent);
                    string languageName = translations.name;
                    languages.Add(new LanguageInfo { name = fileName, display = languageName });
                }
            }
            catch (IOException ex)
            {
                Debug.LogError($"Error retrieving available languages: {ex.Message}");
            }

            return languages;
        }

        private static void CreateDefaultSettings()
        {
            var newSettings = new LanguageData() { language = "ja" };
            SaveSettings(newSettings);
        }

        private static LanguageData LoadSettings()
        {
            try
            {
                if (File.Exists(SETTINGS_PATH))
                {
                    string jsonContent = File.ReadAllText(SETTINGS_PATH);
                    return JsonUtility.FromJson<LanguageData>(jsonContent);
                }
                else
                {
                    return new LanguageData(); // Return default in case of missing file
                }
            }
            catch (IOException ex)
            {
                Debug.LogError($"Error loading language settings: {ex.Message}");
                return new LanguageData();
            }
        }

        private static void SaveSettings(LanguageData settings)
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
    }
}
