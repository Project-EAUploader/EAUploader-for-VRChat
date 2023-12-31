using UnityEditor;
using UnityEngine;
using System.IO;
using System.Collections.Generic;
using Process = System.Diagnostics.Process;
using static labels;
using static styles;
using VRC.SDK3A.Editor;
using VRC.SDKBase.Editor;
using VRC.SDKBase.Editor.Api;

public class Settings
{
    private string[] languages = { "English", "日本語" };
    private int selectedLanguageIndex;
    private string settingsPath ="Packages/com.sabuworks.eauploader/settings.json";
    private string packageJsonPath ="Packages/com.sabuworks.eauploader/package.json";

    public Settings()
    {

        LoadLanguageSetting();
    }

    public void Draw()
    {
        GUILayout.BeginVertical(styles.noBackgroundStyle);

        GUILayout.Label(Get(101), h1LabelStyle);
        GUILayout.BeginHorizontal();

        GUILayout.FlexibleSpace();

        GUILayout.Label(Getc("language_black", 100), NoMargeh2LabelStyle, GUILayout.Height(30));
        int prevSelectedLanguage = selectedLanguageIndex;

        selectedLanguageIndex = EditorGUILayout.Popup(selectedLanguageIndex, languages, GUILayout.ExpandWidth(false));

        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();
        if (prevSelectedLanguage != selectedLanguageIndex)
        {
            SaveLanguageSetting();

            labels.UpdateLanguage();
            LanguageUtility.OnChangeEvent(languages[selectedLanguageIndex]);
            
        }
        else
        {
            selectedLanguageIndex = prevSelectedLanguage;
        }

        string version = "N/A";
        if (File.Exists(packageJsonPath))
        {
            string jsonContent = File.ReadAllText(packageJsonPath);
            var packageObj = JsonUtility.FromJson<PackageInfo>(jsonContent);
            version = packageObj.version;
        }

        GUILayout.Label(Get(114) + version, h2LabelStyle);

        if (GUILayout.Button(Getc("feedback", 117), SubButtonStyle))
        {
            DiscordWebhookSender.OpenDiscordWebhookSenderWindow();
        }

        GUILayout.EndVertical();
    }

    private void SaveLanguageSetting()
    {
        File.WriteAllText(settingsPath, $"{{ \"language\": \"{GetLanguageCode(languages[selectedLanguageIndex])}\" }}");
    }

    private void LoadLanguageSetting()
    {
        if (File.Exists(settingsPath))
        {
            string jsonContent = File.ReadAllText(settingsPath);
            var settingsObj = JsonUtility.FromJson<LanguageSetting>(jsonContent);


            string savedLanguageCode = settingsObj.language;
            string savedLanguageName = GetLanguageName(savedLanguageCode);
            selectedLanguageIndex = System.Array.IndexOf(languages, savedLanguageName);


        }
        else
        {

            selectedLanguageIndex = 0;
        }
    }

    private string GetLanguageCode(string language)
    {
        switch (language)
        {
            case "日本語":
                return "ja";
            default:
                return "en";
        }
    }

    private string GetLanguageName(string code)
    {
        switch (code)
        {
            case "ja":
                return "日本語";
            default:
                return "English";
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
