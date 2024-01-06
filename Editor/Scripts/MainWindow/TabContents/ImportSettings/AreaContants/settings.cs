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
    private string settingsPath ="Packages/tech.uslog.eauploader/settings.json";
    private string packageJsonPath ="Packages/tech.uslog.eauploader/package.json";

    public Settings()
    {
        // 設定をロード
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
        // ドロップダウンバー
        selectedLanguageIndex = EditorGUILayout.Popup(selectedLanguageIndex, languages, GUILayout.ExpandWidth(false));
        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();
        if (prevSelectedLanguage != selectedLanguageIndex)
        {
            SaveLanguageSetting();
            // Update language
            labels.UpdateLanguage();
            LanguageUtility.OnChangeEvent(languages[selectedLanguageIndex]);

            /* !! SDKの翻訳機能（API非公開）（許諾待ち）
            // 確認ダイアログを表示
            bool userConfirmed = EditorUtility.DisplayDialog(
                "Confirm Language Change",
                "Are you sure you want to change the language setting and restart Unity?",
                "Yes",
                "No"
            );


            // ユーザーが「Yes」を選択した場合のみ処理を実行
            if (userConfirmed)
            {
                EditorUtility.DisplayProgressBar("Updating Language", "Savinf Language Settings...", 0.1f);

                SaveLanguageSetting();

                EditorUtility.DisplayProgressBar("Updating Language", "Updating Labels...", 0.3f);
                // Update language
                labels.UpdateLanguage();

                /* !! SDKの翻訳機能はVRChatと相談中 
                // SDK翻訳更新
                EditorUtility.DisplayProgressBar("Updating Language", "Updating from " + languages[prevSelectedLanguage] + " to " + languages[selectedLanguageIndex], 0.6f);
                string oldLanguageCode = GetLanguageCode(languages[prevSelectedLanguage]);
                string newLanguageCode = GetLanguageCode(languages[selectedLanguageIndex]);
                UpdateLanguageSpecificContent(oldLanguageCode, newLanguageCode);

                EditorUtility.DisplayProgressBar("Updating Language", "Restart Unity...", 0.8f);
                // Unityエディタのパスを取得
                var unityEditorPath = Process.GetCurrentProcess().MainModule.FileName;
                var projectPath = Directory.GetCurrentDirectory();

                // エディタを再起動するバッチファイルを作成
                string batchScript = Path.Combine(projectPath, "RestartUnity.bat");
                File.WriteAllText(batchScript, $"@echo off\nstart \"\" \"{unityEditorPath}\" -projectPath \"{projectPath}\"\nexit");

                EAUploader.skipOnDisable = true;

                // バッチファイルを実行してUnityエディタを再起動
                Process.Start(batchScript);

                EditorUtility.ClearProgressBar();

                // 現在のUnityエディタプロセスを終了
                EditorApplication.Exit(0);

                EditorApplication.ExecuteMenuItem("VRChat SDK/Reload SDK");
                */
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

            // Debug.Log($"Loaded language setting: {savedLanguageCode} (index: {selectedLanguageIndex})");
        }
        else
        {
            // ファイルが存在しない場合はデフォルト（英語）を選択
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
