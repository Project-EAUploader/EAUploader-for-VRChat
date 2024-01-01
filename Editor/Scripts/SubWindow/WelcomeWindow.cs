/*
    【要修正】
    
using UnityEditor;
using UnityEngine;
using UnityEngine.Networking;
using System;
using System.IO;
using System.Threading.Tasks;
using static styles;
using static labels;

public class WelcomeWindow : EditorWindow
{
    private string remotePackageUrl = "https://raw.githubusercontent.com/Project-EAUploader/EAUploader-for-VRChat/main/package.json";
    private string localPackagePath = "Packages/com.sabuworks.eauploader/package.json";
    private string remoteVersion;
    private string localVersion;

    [MenuItem("Window/Welcome Window")]
    public static void ShowWindow()
    {
        var window = GetWindow<WelcomeWindow>("Welcome");
        window.minSize = new Vector2(400, 200);
        window.maxSize = window.minSize;
        window.ShowUtility();  // ウィンドウをポップアップとして表示
    }

    public static void CloseWindow()
    {
        var window = GetWindow<WelcomeWindow>();
        if (window != null)
        {
            window.Close();
        }
    }

    void OnEnable()
    {
        // バージョン情報の取得を待ってからウィンドウを更新
        FetchVersionInfo().ContinueWith(_ => Repaint());
    }

    void OnGUI()
    {
        EditorGUI.DrawRect(new Rect(0, 0, position.width, position.height), Color.white);
        GUILayout.Label("EAUploaderへようこそ", h1LabelStyle);

        Debug.Log($"現在: {localVersion}\nGit hub: {remoteVersion}");

        if (!string.IsNullOrEmpty(remoteVersion) && !string.IsNullOrEmpty(localVersion))
        {
            GUILayout.Label($"最新のバージョン: {remoteVersion}", h4LabelStyle);
            GUILayout.Label($"現在のバージョン: {localVersion}", h4LabelStyle);

            if (new Version(remoteVersion) > new Version(localVersion))
            {
                GUILayout.Label("新しいバージョンがリリースされています。\nVRChat Creator Companion又はGit hubより更新してください。", h2LabelStyle);
            }
        }
    }

    async Task FetchVersionInfo()
    {
        remoteVersion = await GetRemoteVersion(remotePackageUrl);
        localVersion = GetLocalVersion(localPackagePath);

        if (string.IsNullOrEmpty(remoteVersion))
        {
            Debug.LogError("リモートバージョンの取得に失敗しました。");
        }

        if (string.IsNullOrEmpty(localVersion))
        {
            Debug.LogError("ローカルバージョンの取得に失敗しました。");
        }
    }

    async Task<string> GetRemoteVersion(string url)
    {
        using (UnityWebRequest webRequest = UnityWebRequest.Get(url))
        {
            await webRequest.SendWebRequest().ToUniTask(); // ToUniTaskを使用してUniTaskに変換

            if (webRequest.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError($"Error fetching remote package: {webRequest.error}");
                return null;
            }
            else
            {
                var packageInfo = JsonUtility.FromJson<PackageInfo>(webRequest.downloadHandler.text);
                return packageInfo.version;
            }
        }
    }

    string GetLocalVersion(string filePath)
    {
        if (File.Exists(filePath))
        {
            string jsonContent = File.ReadAllText(filePath);
            var packageInfo = JsonUtility.FromJson<PackageInfo>(jsonContent);
            return packageInfo.version;
        }
        return null;
    }

    [Serializable]
    private class PackageInfo
    {
        public string version;
    }
}
*/