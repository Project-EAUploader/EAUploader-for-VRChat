using UnityEngine;
using System.Collections.Generic;
using System.IO;

public static class LanguageUtility
{
    public delegate void LanguageChangedHandler(string newLanguage);
    public static event LanguageChangedHandler OnLanguageChanged;

    public static string GetCurrentLanguage()
    {
        // Debug.Log("GetCurrentLanguage called");
        string settingsPath = "Packages/tech.uslog.eauploader/settings.json";
        // Debug.Log($"Settings path: {settingsPath}");

        if (File.Exists(settingsPath))
        {
            // Debug.Log("Settings file found");
            string jsonContent = File.ReadAllText(settingsPath);
            var settings = JsonUtility.FromJson<LanguageSetting>(jsonContent);

            // Debug.Log($"Language from settings: {language}");
            return settings.language;
        }

        // Debug.LogWarning("Settings file not found, defaulting to English");
        return "en"; // デフォルト
    }

    public static void OnChangeEvent(string language)
    {
        OnLanguageChanged?.Invoke(language);
    }
}