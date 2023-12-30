using UnityEngine;
using System.Collections.Generic;
using System.IO;
using static icons;

[System.Serializable]
public class LocalizationData
{
    public List<LocalizationItem> items;
}

[System.Serializable]
public class LocalizationItem
{
    public int key;
    public string value;
}

public class labels
{
    private static Dictionary<int, string> translations;
    private static string language;

    static labels()
    {
        // Debug.Log("labels static constructor called");
        UpdateLanguage();
    }

    public static void UpdateLanguage()
    {
        // Debug.Log("UpdateLanguage called");
        language = LanguageUtility.GetCurrentLanguage();
        // Debug.Log($"Current language: {language}");
        LoadTranslations();
    }

    private static void LoadTranslations()
    {
        // Debug.Log("LoadTranslations called");
        string basePath = "Packages/com.sabuworks.eauploader/Editor/Resources/Translation";
        string translationFilePath = Path.Combine(basePath, $"translations_{language}.json");
        // Debug.Log($"Translation file path: {translationFilePath}");

        if (File.Exists(translationFilePath))
        {
            // Debug.Log("Translation file found");
            string jsonContent = File.ReadAllText(translationFilePath);
            LocalizationData loadedData = JsonUtility.FromJson<LocalizationData>(jsonContent);

            translations = new Dictionary<int, string>();
            foreach (var item in loadedData.items)
            {
                translations[item.key] = item.value;
            }

            // Debug.Log($"Loaded translations for language: {language}");
        }
        else
        {
            // Debug.LogError($"Translation file not found for language: {language}");
            translations = new Dictionary<int, string>();
        }
    }

    public static string Get(int key, params object[] args)
    {
        if (translations.TryGetValue(key, out string format))
        {
            return string.Format(format, args);
        }
        return $"[{key}]";
    }

    public static GUIContent Getc(string iconname, int key)
    {
        if (key==0)
        {
            Texture2D icon = icons.GetIcon(iconname);
            return new GUIContent("", icon);
        }else
        {
        Texture2D icon = icons.GetIcon(iconname);
        string text = Get(key);

        return new GUIContent(text, icon);
        }
    }
}

[System.Serializable]
public class LanguageSetting
{
    public string language;
}