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
        // 設定をロードする
        LoadLanguageSetting();
    }

    public void Draw()
    {
        GUILayout.BeginVertical(styles.noBackgroundStyle);
        // Language Dropdown
        GUILayout.Label(Get(101), h1LabelStyle);
        GUILayout.BeginHorizontal();
        // 中央に配置するために前のスペースを追加
        GUILayout.FlexibleSpace();
        // ラベルを描画
        GUILayout.Label(Getc("language_black", 100), NoMargeh2LabelStyle, GUILayout.Height(30));
        int prevSelectedLanguage = selectedLanguageIndex;
        // ドロップダウンバーを描画
        selectedLanguageIndex = EditorGUILayout.Popup(selectedLanguageIndex, languages, GUILayout.ExpandWidth(false));
        // 中央に配置するために後ろのスペースを追加
        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();
        if (prevSelectedLanguage != selectedLanguageIndex)
        {
            SaveLanguageSetting();
            // Update language
            labels.UpdateLanguage();
            LanguageUtility.OnChangeEvent(languages[selectedLanguageIndex]);
            /* !! SDKの翻訳機能はVRChatと相談中 
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

            // 保存された言語コードに基づいて、対応するインデックスを見つける
            string savedLanguageCode = settingsObj.language;
            string savedLanguageName = GetLanguageName(savedLanguageCode);
            selectedLanguageIndex = System.Array.IndexOf(languages, savedLanguageName);

            // デバッグ用のログ
            Debug.Log($"Loaded language setting: {savedLanguageCode} (index: {selectedLanguageIndex})");
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

    /*
    private void UpdateLanguageSpecificContent(string oldLanguageCode, string newLanguageCode)
    {
        // UXML ファイルの更新
        UXMLLocalizationManager uxmlLocalizationManager = new UXMLLocalizationManager();
        var uxmlPaths = new List<string>
        {
            @"Packages/com.vrchat.avatars/Editor/VRCSDK/SDK3A/Resources/VRCSdkAvatarBuilderBuildLayout.uxml",
            @"Packages/com.vrchat.avatars/Editor/VRCSDK/SDK3A/Resources/VRCSdkAvatarBuilderContentInfo.uxml",
            @"Packages/com.vrchat.avatars/Editor/VRCSDK/SDK3A/Elements/AvatarBuildSuccessNotification/Resources/AvatarBuildSuccessNotification.uxml",
           "Packages/com.vrchat.avatars\Editor\VRCSDK\SDK3A\Elements\AvatarFallbackSelectionErrorNotification\Resources\AvatarFallbackSelectionErrorNotification.uxml",
           "Packages/com.vrchat.avatars\Editor\VRCSDK\SDK3A\Elements\AvatarUploadErrorNotification\Resources\AvatarUploadErrorNotification.uxml",
           "Packages/com.vrchat.avatars\Editor\VRCSDK\SDK3A\Elements\AvatarUploadSuccessNotification\Resources\AvatarUploadSuccessNotification.uxml",
           "Packages/com.vrchat.base\Editor\VRCSDK\Dependencies\VRChat\Elements\ThumbnailFoldout\Resources\ThumbnailFoldout.uxml"
        };
        uxmlLocalizationManager.UpdateUXMLFiles(uxmlPaths);

        // C# スクリプトの更新
        CSharpLocalizationManager csharpLocalizationManager = new CSharpLocalizationManager();
        var scriptPaths = new List<string>
        {
            // ここにパスを追加　忘れないように
           "Packages/com.vrchat.avatars\Editor\VRCSDK\SDK3A\VRCSdkControlPanelAvatarBuilder.cs",
           "Packages/com.vrchat.base\Editor\VRCSDK\Dependencies\VRChat\Validation\Performance\SDKPerformanceDisplay.cs",
           "Packages/com.vrchat.base\Editor\VRCSDK\Dependencies\VRChat\ControlPanel\VRCSdkControlPanelAccount.cs",
           "Packages/com.vrchat.base\Editor\VRCSDK\Dependencies\VRChat\ControlPanel\VRCSdkControlPanelContent.cs",
           "Packages/com.vrchat.base\Editor\VRCSDK\Dependencies\VRChat\Elements\ContentWarningsField\ContentWarningsField.cs"
        };

        foreach (var scriptPath in scriptPaths)
        {
            string filterJsonPath = GenerateFilterJsonPath(scriptPath);
            csharpLocalizationManager.UpdateScriptLanguage(oldLanguageCode, newLanguageCode, scriptPath, filterJsonPath);
        }
    }

    private string GenerateFilterJsonPath(string scriptPath)
    {
        string fileName = Path.GetFileNameWithoutExtension(scriptPath);
        return $@"Packages\com.sabuworks.eauploader\Editor\Resources\Translation\VRCSDK\TransFilter\{fileName}_TransFilter.json";
    }
    */

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
