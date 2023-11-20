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
                UpdateNotificationWindow.ShowWindow(latestRelease.tag_name, updateMessage);
            }
        });
    }

    private static string GetRichTextForLanguage(string language)
    {
        switch (language)
        {
            case "日本語":
                return "<color=black><size=20><b>新しいバージョンがあります。更新してください。</b></size></color>\n\n";
            case "English":
            default:
                return "<color=black><size=20><b>There is a newer version. Please update.</b></size></color>\n\n";
        }
    }
}

public class UpdateNotificationWindow : EditorWindow
{
    private string versionInfo;
    private string richTextInfo;

    public static void ShowWindow(string version, string richText)
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

    [System.Serializable]
    public class GitHubRelease
    {
        public string tag_name;
        public string name;
        public string body;
        public string published_at;
    }
}
