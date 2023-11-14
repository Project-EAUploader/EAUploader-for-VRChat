using UnityEditor;
using UnityEngine;
using System.IO;
using static labels;
using static styles;

public class Settings
{
    private string[] languages = { "English", "日本語" };
    private int selectedLanguageIndex;
    private string settingsPath = @"Packages\com.sabuworks.eauploader\settings.json";
    private string packageJsonPath = @"Packages\com.sabuworks.eauploader\package.json";

    public Settings()
    {
        // 設定をロードする
        LoadLanguageSetting();
    }

    public void Draw()
    {
        GUILayout.BeginVertical(styles.noBackgroundStyle);
        // Language Dropdown
        GUILayout.Label(labels.T1, h1Style);
        GUILayout.Label(labels.Changelng, styles.h2Style);
        int prevSelectedLanguage = selectedLanguageIndex;
        selectedLanguageIndex = EditorGUILayout.Popup(selectedLanguageIndex, languages);
        if (prevSelectedLanguage != selectedLanguageIndex)
        {
            SaveLanguageSetting();

            // Update language
            labels.UpdateLanguage();
        }

        string version = "N/A";
        if (File.Exists(packageJsonPath))
        {
            string jsonContent = File.ReadAllText(packageJsonPath);
            var packageObj = JsonUtility.FromJson<PackageInfo>(jsonContent);
            version = packageObj.version;
        }

        GUILayout.Label("Version: " + version, h4Style);

        GUILayout.EndVertical();
    }

    private void SaveLanguageSetting()
    {
        File.WriteAllText(settingsPath, $"{{ \"language\": \"{languages[selectedLanguageIndex]}\" }}");
    }

    private void LoadLanguageSetting()
    {
        if (File.Exists(settingsPath))
        {
            string jsonContent = File.ReadAllText(settingsPath);
            var settingsObj = JsonUtility.FromJson<LanguageSetting>(jsonContent);
            selectedLanguageIndex = System.Array.IndexOf(languages, settingsObj.language);
        }
    }

    [System.Serializable]
    private class LanguageSetting
    {
        public string language;
    }

        [System.Serializable]
    private class PackageInfo
    {
        public string version;
    }
}
