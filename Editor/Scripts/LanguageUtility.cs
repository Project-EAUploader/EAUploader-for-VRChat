using UnityEngine;
using System.Collections.Generic;
using System.IO;

public static class LanguageUtility
{
    public delegate void LanguageChangedHandler(string newLanguage);
    public static event LanguageChangedHandler OnLanguageChanged;

    public static string GetCurrentLanguage()
    {
        string settingsPath = @"Packages\com.sabuworks.eauploader\settings.json";

        if (File.Exists(settingsPath))
        {
            string jsonContent = File.ReadAllText(settingsPath);
            var settings = JsonUtility.FromJson<LanguageSetting>(jsonContent);

            return settings.language;
        }



    }

    public static void OnChangeEvent(string language)
    {
        OnLanguageChanged?.Invoke(language);
    }
}