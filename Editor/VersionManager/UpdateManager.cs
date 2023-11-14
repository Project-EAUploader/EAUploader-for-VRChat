using UnityEngine;
using UnityEditor;
using UnityEngine.Networking;
using System;
using System.IO;
using System.Collections;

public static class PackageVersionReader
{
    private const string PackagePath = @"Packages\com.sabuworks.eauploader\package.json";

    public static string GetCurrentVersion()
    {
        string jsonContent = File.ReadAllText(PackagePath);
        var packageInfo = JsonUtility.FromJson<PackageInfo>(jsonContent);
        return packageInfo.version;
    }

    [System.Serializable]
    private class PackageInfo
    {
        public string version;
    }
}

[InitializeOnLoad]
public class StartupVersionChecker
{
    static StartupVersionChecker()
    {
        EditorApplication.update += CheckVersion;
    }

    private static void CheckVersion()
    {
        EditorApplication.update -= CheckVersion;

        string currentVersion = PackageVersionReader.GetCurrentVersion();
        string language = LanguageUtility.GetCurrentLanguage();
        string updateMessage = GetRichTextForLanguage(language);

        GitHubVersionFetcher.FetchLatestVersion((latestRelease) =>
        {
            if (latestRelease.tag_name != currentVersion)
            {
                UpdateNotificationWindow.ShowWindow(latestRelease.tag_name, updateMessage, latestRelease.download_url);
            }
        });
    }

    private static string GetRichTextForLanguage(string language)
    {
        switch (language)
        {
            case "日本語":
                return "<color=red><size=20><b>リリース 0.1.0 - 開発中途</b></size></color>\n\n"+
                "<size=16><color=black>・プロジェクト公開\n・多くのバグ, 未実装の機能が含まれます\n　正式リリースまでしばらくお待ちください</color></size>";
            case "English":
            default:
                return "<color=red><size=20><b>Release 0.1.0 - In Development</b></size></color>\n\n"+
                "<size=16><color=black>・Initial project release\n・Contains numerous bugs and unimplemented features.    Please wait for the official release.</color></size>";
        }
    }
}

public class UpdateNotificationWindow : EditorWindow
{
    private string versionInfo;
    private string richTextInfo;

    public static void ShowWindow(string version, string richText, string downloadUrl)
    {
        UpdateNotificationWindow window = (UpdateNotificationWindow)EditorWindow.GetWindow(typeof(UpdateNotificationWindow));
        window.versionInfo = version;
        window.richTextInfo = richText;
        window.Show();
    }

    void OnGUI()
    {
        GUILayout.Label("New Version Available: " + versionInfo, EditorStyles.boldLabel);
        GUILayout.TextArea(richTextInfo, GUILayout.ExpandHeight(true));
    }
}

public static class GitHubVersionFetcher
{
    private const string RepositoryURL = "https://api.github.com/repos/Sabu006/EAUploader-for-VRChat/releases/latest";

    public static IEnumerator FetchLatestVersion(System.Action<GitHubRelease> callback)
    {
        using (UnityWebRequest webRequest = UnityWebRequest.Get(RepositoryURL))
        {
            yield return webRequest.SendWebRequest();

            if (webRequest.isNetworkError || webRequest.isHttpError)
            {
                Debug.LogError("Failed to fetch the latest version: " + webRequest.error);
            }
            else
            {
                var json = webRequest.downloadHandler.text;
                var latestRelease = JsonUtility.FromJson<GitHubRelease>(json);
                callback?.Invoke(latestRelease);
            }
        }
    }

    public static IEnumerator DownloadLatestVersion(string downloadUrl, System.Action<bool> callback)
    {
        using (UnityWebRequest webRequest = UnityWebRequest.Get(downloadUrl))
        {
            yield return webRequest.SendWebRequest();

            if (webRequest.isNetworkError || webRequest.isHttpError)
            {
                Debug.LogError("Failed to download the latest version: " + webRequest.error);
                callback?.Invoke(false);
            }
            else
            {
                string packageFilePath = Path.Combine(Application.dataPath, "DownloadedUpdate.unitypackage");
                File.WriteAllBytes(packageFilePath, webRequest.downloadHandler.data);
                ApplyUpdate(packageFilePath, callback);
            }
        }
    }

    private static void ApplyUpdate(string packageFilePath, System.Action<bool> callback)
    {
        try
        {
            AssetDatabase.ImportPackage(packageFilePath, true);
            callback?.Invoke(true);
        }
        catch (Exception e)
        {
            Debug.LogError("Failed to apply update: " + e.Message);
            callback?.Invoke(false);
        }
    }

    [System.Serializable]
    public class GitHubRelease
    {
        public string tag_name;
        public string name;
        public string body;
        public string published_at;
        public string download_url; // ダウンロードURL
    }
}
