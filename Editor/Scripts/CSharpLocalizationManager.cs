using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Linq;
using Newtonsoft.Json;
using static LanguageUtility;

public class CSharpLocalizationManager
{
    private static Dictionary<int, string> localizedStrings = new Dictionary<int, string>();


    public CSharpLocalizationManager()
    {
        Debug.Log("CSharpLocalizationManager constructor called");
    }

    private void MarkKeysAsGenerated(string settingsFilePath)
    {
        Dictionary<string, object> settings;
        if (File.Exists(settingsFilePath))
        {
            string settingsContent = File.ReadAllText(settingsFilePath);
            settings = JsonConvert.DeserializeObject<Dictionary<string, object>>(settingsContent);
        }
        else
        {
            settings = new Dictionary<string, object>();
        }
        
        settings["generatedKeys"] = true;
        string updatedSettings = JsonConvert.SerializeObject(settings, Formatting.Indented);
        File.WriteAllText(settingsFilePath, updatedSettings);
    }

    public void ExtractStringsAndSaveToJson(string scriptPath, string jsonFilePath, string filterJsonPath, string settingsFilePath)
    {
        if (!AreKeysGenerated(settingsFilePath))
        {
            List<string> filterStrings = LoadFilterStrings(filterJsonPath);
            var extractedStrings = ExtractStringsFromScript(scriptPath, filterStrings);
            var keys = GenerateKeysForStrings(extractedStrings);
            SaveStringsToJson(keys, jsonFilePath);
            MarkKeysAsGenerated(settingsFilePath);
        }
    }

    private List<string> LoadFilterStrings(string filterJsonPath)
    {
        if (File.Exists(filterJsonPath))
        {
            string jsonContent = File.ReadAllText(filterJsonPath);
            return JsonConvert.DeserializeObject<List<string>>(jsonContent);
        }
        return new List<string>();
    }

    private Dictionary<string, string> ExtractStringsFromScript(string scriptPath, List<string> filterStrings)
    {
        string scriptContent = File.ReadAllText(scriptPath);
        var matches = Regex.Matches(scriptContent, "\"([^\"]*)\"");

        var extractedStrings = new Dictionary<string, string>();
        foreach (Match match in matches)
        {
            string matchedString = match.Groups[1].Value;
            if (filterStrings.Contains(matchedString) && !extractedStrings.ContainsKey(matchedString))
            {
                extractedStrings.Add(matchedString, matchedString);
            }
        }

        return extractedStrings;
    }

    private Dictionary<string, int> GenerateKeysForStrings(Dictionary<string, string> extractedStrings)
    {
        var keys = new Dictionary<string, int>();
        foreach (var pair in extractedStrings)
        {
            int key = pair.Value.GetHashCode();
            keys.Add(pair.Key, key);
        }
        return keys;
    }

    private void SaveStringsToJson(Dictionary<string, int> keys, string jsonFilePath)
    {
        LoadLocalizedStrings(jsonFilePath);  // 既存のローカライズされた文字列を読み込む

        LocalizationData localizationData = new LocalizationData { items = new List<LocalizationItem>() };

        foreach (var pair in keys)
        {
            if (!localizedStrings.ContainsKey(pair.Value))  // 既に存在しないキーのみを追加
            {
                localizationData.items.Add(new LocalizationItem { key = pair.Value, value = pair.Key });
                localizedStrings.Add(pair.Value, pair.Key);
            }
        }

        string json = JsonUtility.ToJson(localizationData, true);
        if (localizationData.items.Count > 0)  // 新しいアイテムがある場合のみファイルに書き込む
        {
            File.WriteAllText(jsonFilePath, json);
        }
    }

    public void UpdateScriptWithStringReferences(string scriptPath, string filterJsonPath)
    {
        List<string> filterStrings = LoadFilterStrings(filterJsonPath);

        string currentLanguage = LanguageUtility.GetCurrentLanguage();
        string jsonFilePath = $@"Packages\com.sabuworks.eauploader\Editor\Resources\Translation\EAUploader\VRCSDK\{currentLanguage}\BuilderTranslation.json";

        LoadLocalizedStrings(jsonFilePath);

        string scriptContent = File.ReadAllText(scriptPath);
        var keys = GenerateKeysForStrings(ExtractStringsFromScript(scriptPath, filterStrings));

        // スクリプト内の文字列をローカライズされた文字列で置き換える
        foreach (var pair in keys)
        {
            string localizedString = GetLocalizedString(pair.Value);
            if (localizedString != null)
            {
                // 実際の文字列置換
                scriptContent = scriptContent.Replace($"\"{pair.Key}\"", $"\"{localizedString}\"");
            }
        }

        File.WriteAllText(scriptPath, scriptContent);
    }

    private Dictionary<int, string> LoadLocalizedStrings(string jsonFilePath)
    {
        var localizedStrings = new Dictionary<int, string>();
        if (File.Exists(jsonFilePath))
        {
            Debug.Log($"Loading localized strings from file: {jsonFilePath}");
            string jsonContent = File.ReadAllText(jsonFilePath);
            var data = JsonUtility.FromJson<LocalizationData>(jsonContent);
            foreach (var item in data.items)
            {
                localizedStrings[item.key] = item.value;
            }
            Debug.Log($"Loaded {localizedStrings.Count} localized strings");
        }
        else
        {
            Debug.LogError($"Localization file not found: {jsonFilePath}");
        }
        return localizedStrings;
    }

    private bool AreKeysGenerated(string settingsFilePath)
    {
        if (File.Exists(settingsFilePath))
        {
            string settingsContent = File.ReadAllText(settingsFilePath);
            var settings = JsonConvert.DeserializeObject<Dictionary<string, object>>(settingsContent);
            return settings.ContainsKey("generatedKeys") && (bool)settings["generatedKeys"];
        }
        return false;
    }

    // EAUploader内からアクセスする場合
    public static string GetLocalizedString(int key)
    {
        if (localizedStrings.TryGetValue(key, out string value))
        {
            return value;
        }
        return null; 
    }

    // 言語変更選択時
    public void UpdateScriptLanguage(string oldLanguage, string newLanguage, string scriptPath, string filterJsonPath)
    {
        Debug.Log($"UpdateScriptLanguage called with oldLanguage: {oldLanguage}, newLanguage: {newLanguage}, scriptPath: {scriptPath}");

        // 既存の言語設定の翻訳を読み込む
        string oldLanguageFilePath = GenerateTranslatedJsonPath(oldLanguage, scriptPath);
        Debug.Log($"Loading old language file: {oldLanguageFilePath}");
        var oldLocalizedStrings = LoadLocalizedStrings(oldLanguageFilePath);
        Debug.Log($"Loaded {oldLocalizedStrings.Count} strings from old language file");

        // 現在の言語設定の翻訳を読み込む
        string newLanguageFilePath = GenerateTranslatedJsonPath(newLanguage, scriptPath);
        Debug.Log($"Loading new language file: {newLanguageFilePath}");
        var newLocalizedStrings = LoadLocalizedStrings(newLanguageFilePath);
        Debug.Log($"Loaded {newLocalizedStrings.Count} strings from new language file");

        // 対象のC#スクリプトを読み込む
        Debug.Log($"Loading C# script: {scriptPath}");
        string scriptContent = File.ReadAllText(scriptPath);

        // 既存の翻訳でスクリプト内の文字列をキーに置き換える
        foreach (var oldItem in oldLocalizedStrings)
        {
            Debug.Log($"Replacing {oldItem.Value} with key {oldItem.Key}");
            scriptContent = scriptContent.Replace($"\"{oldItem.Value}\"", $"\"{oldItem.Key}\"");
        }

        // 新しい翻訳でスクリプト内のキーを文字列に置き換える
        foreach (var newItem in newLocalizedStrings)
        {
            Debug.Log($"Replacing key {newItem.Key} with {newItem.Value}");
            scriptContent = scriptContent.Replace($"\"{newItem.Key}\"", $"\"{newItem.Value}\"");
        }

        // 更新されたスクリプトを保存
        Debug.Log($"Saving updated C# script: {scriptPath}");
        File.WriteAllText(scriptPath, scriptContent);
        Debug.Log("Script update completed");

        EditorApplication.ExecuteMenuItem("VRChat SDK/Reload SDK");
    }

    /* 開発用 */
    public void ExtractAndSaveFilteredStrings(string scriptPath, string jsonFilePath, string filterJsonPath)
    {
        List<string> filterStrings = LoadFilterStrings(filterJsonPath);

        var extractedStrings = ExtractStringsFromScript(scriptPath, filterStrings);
        var keys = GenerateKeysForStrings(extractedStrings);

        // 既存のJSONファイルからローカライズデータを読み込む、または新しいデータを作成
        LocalizationData localizationData = new LocalizationData { items = new List<LocalizationItem>() };
        if (File.Exists(jsonFilePath))
        {
            string existingJson = File.ReadAllText(jsonFilePath);
            localizationData = JsonUtility.FromJson<LocalizationData>(existingJson) ?? localizationData;
        }

        foreach (var pair in keys)
        {
            // 既に存在するキーをチェック
            if (!localizationData.items.Any(item => item.key == pair.Value))
            {
                localizationData.items.Add(new LocalizationItem { key = pair.Value, value = pair.Key });
            }
        }

        // ローカライズデータをJSONファイルに保存
        string json = JsonUtility.ToJson(localizationData, true);
        if (localizationData.items.Count > 0)
        {
            // ファイルが存在しない場合は新しいファイルを作成
            File.WriteAllText(jsonFilePath, json);
        }
    }

    // すべての文字列抽出
    public void ExtractAllStringsAndSaveToJson(string scriptPath, string jsonFilePath)
    {
        string scriptContent = File.ReadAllText(scriptPath);
        var matches = Regex.Matches(scriptContent, "\"([^\"]*)\"");

        List<string> extractedStrings = new List<string>();
        foreach (Match match in matches)
        {
            string matchedString = match.Groups[1].Value;
            extractedStrings.Add(matchedString);
        }

        string json = JsonConvert.SerializeObject(extractedStrings, Formatting.Indented);
        File.WriteAllText(jsonFilePath, json);
    }
    /* */

    private static string GenerateTranslatedJsonPath(string language, string scriptPath)
    {
        string fileName = Path.GetFileNameWithoutExtension(scriptPath);
        return $@"Packages\com.sabuworks.eauploader\Editor\Resources\Translation\EAUploader\VRCSDK\{language}\{fileName}_Translated.json";
    }
}