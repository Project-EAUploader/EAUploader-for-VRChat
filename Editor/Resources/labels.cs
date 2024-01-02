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
    /// <summary>
    /// 言語設定に基づいてラベルおよびアイコン付きラベルを取得するクラス
    /// </summary>
    private static Dictionary<int, string> translations;
    private static string language;

    static labels()
    {
        UpdateLanguage();
    }

    public static void UpdateLanguage()
    {
        language = LanguageUtility.GetCurrentLanguage();
        LoadTranslations();
    }

    private static void LoadTranslations()
    {
        string basePath = "Packages/com.sabuworks.eauploader/Editor/Resources/Translation";
        string translationFilePath = Path.Combine(basePath, $"translations_{language}.json");

        if (File.Exists(translationFilePath))
        {
            string jsonContent = File.ReadAllText(translationFilePath);
            LocalizationData loadedData = JsonUtility.FromJson<LocalizationData>(jsonContent);

            translations = new Dictionary<int, string>();
            foreach (var item in loadedData.items)
            {
                translations[item.key] = item.value;
            }
        }
        else
        {
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