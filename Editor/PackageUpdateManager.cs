/*

    自動更新機能はインストーラーで代替

using UnityEngine;
using UnityEditor;
using Cysharp.Threading.Tasks;
using UnityEngine.Networking;
using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using static styles;
using static labels;

[Serializable]
public class UpdateInfo
{
    public string version;
    public string[] remove;
    public UpdateFile[] update;
}

[Serializable]
public class UpdateInfoArray
{
    public UpdateInfo[] updates;
}

[Serializable]
public class UpdateFile
{
    public string path;
    public string url;
}

public class UpdateManager : EditorWindow
{
    private string updateInfoUrl = "https://raw.githubusercontent.com/Project-EAUploader/EAUploader-Content-Delivery/main/index.json";
    private string localVersionPath = "Packages/com.sabuworks.eauploader/Editor/PatchVersion.json";
    private string baseUrl = "https://raw.githubusercontent.com/Project-EAUploader/EAUploader-Content-Delivery/main/";
    private string currentStatusMessage = "";
    private bool updateCompleted = false;
    private string latestVersion = "";


    [MenuItem("EAUploader/Patch Update")]
    public static void ShowWindow()
    {
        var window = GetWindow<UpdateManager>(Get(850), true);
        window.minSize = new Vector2(400, 200);
        window.maxSize = new Vector2(400, 200);
        window.StartUpdateProcess();
    }

    private void OnGUI()
    {
        EditorGUI.DrawRect(new Rect(0, 0, maxSize.x, maxSize.y), Color.white);

        if (!updateCompleted)
        {
            string localVersion = ReadLocalVersion();
            GUILayout.Label(Get(851), h2LabelStyle);
            GUILayout.Label($"{localVersion} --> {latestVersion}", h2LabelStyle);
            GUILayout.Label(Get(852), h2LabelStyle);
        }
        else
        {
            GUILayout.Label(Get(853), h1LabelStyle);

            if (GUILayout.Button(Get(854), MainButtonStyle))
            {
                EAUploader.ShowWindow();
                this.Close();
            }
        }
    }

    private void OnUpdateCompleted(string newVersion)
    {
        updateCompleted = true;
        latestVersion = newVersion;
        Repaint();
    }

    public async Task StartUpdateProcess()
    {
        Debug.Log("Start Update Process");
        await DownloadUpdateInfo();
    }

    private async UniTask DownloadUpdateInfo()
    {
        Debug.Log("Downloading update info");

        UnityWebRequest request = UnityWebRequest.Get(updateInfoUrl);
        await request.SendWebRequest();

        if (request.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError("Failed to download update info: " + request.error);
            return;
        }

        string json = request.downloadHandler.text;
        Debug.Log("JSON received: " + json);

        UpdateInfoArray updateArray = JsonUtility.FromJson<UpdateInfoArray>(json);
        if (updateArray == null || updateArray.updates == null)
        {
            Debug.LogError("Failed to parse update info");
            return;
        }

        Debug.Log("Parsed update info successfully");
        await ProcessAllUpdates(updateArray.updates);
    }


    private async Task ProcessAllUpdates(UpdateInfo[] allUpdates)
    {
        if (allUpdates == null)
        {
            Debug.LogError("No updates to process");
            this.Close();
            return;
        }

        string localVersion = ReadLocalVersion();
        Debug.Log("Local version: " + localVersion);
        var updatesToApply = allUpdates.Where(u => IsNewerVersion(u.version, localVersion)).ToArray();

        Debug.Log($"Number of updates to apply: {updatesToApply.Length}");

        if (updatesToApply.Length == 0)
        {
            Debug.Log("No updates to apply");
            this.Close();
            return;
        }
        else
        {
            EAUploader.CloseWindow();
        }

        HashSet<string> updatedPaths = new HashSet<string>();
        foreach (var update in updatesToApply)
        {
            if (update.remove != null)
            {
                foreach (string filePath in update.remove)
                {
                    RemoveFile(filePath);
                }
            }

            foreach (UpdateFile file in update.update)
            {
                Debug.Log($"Adding file to update queue: {file.path}");
                await StartFileUpdate(file.path, baseUrl + file.url);
            }
        }

        var latestVersion = allUpdates.Select(u => new Version(u.version)).OrderByDescending(v => v).FirstOrDefault();

        if (latestVersion != null)
        {
            await UpdateLocalVersionFile(latestVersion.ToString());
            OnUpdateCompleted(latestVersion.ToString());
        }

        Debug.Log("All updates processed");
        return;
    }

    private async Task UpdateLocalVersionFile(string newVersion)
    {
        Debug.Log($"Updating local version file to {newVersion}");

        UpdateInfo localVersionInfo = new UpdateInfo { version = newVersion };
        
        string json = JsonUtility.ToJson(localVersionInfo);

        File.WriteAllText(localVersionPath, json);
        Debug.Log("Local version file updated.");
    }

    private string ReadLocalVersion()
    {
        if (File.Exists(localVersionPath))
        {
            string json = File.ReadAllText(localVersionPath);
            try
            {
                UpdateInfo localVersionInfo = JsonUtility.FromJson<UpdateInfo>(json);
                return localVersionInfo.version;
            }
            catch (Exception ex)
            {
                Debug.LogError($"Failed to parse local version file: {ex.Message}");
                return "0.0.0";
            }
        }
        return "0.0.0";
    }

    private bool IsNewerVersion(string version1, string version2)
    {
        if (string.IsNullOrWhiteSpace(version1) || string.IsNullOrWhiteSpace(version2))
        {
            Debug.LogError("Invalid version string: version1 or version2 is null or whitespace.");
            return false;
        }

        try
        {
            var v1 = new Version(version1);
            var v2 = new Version(version2);
            return v1 > v2;
        }
        catch (Exception ex)
        {
            Debug.LogError($"Error comparing versions: {ex.Message}");
            return false;
        }
    }

    private async UniTask StartFileUpdate(string filePath, string fileUrl)
    {
        UnityWebRequest request = UnityWebRequest.Get(fileUrl);
        await request.SendWebRequest();

        if (request.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError($"Error downloading file '{filePath}': {request.error}");
            return;
        }

        byte[] data = request.downloadHandler.data;

        string directoryPath = Path.GetDirectoryName(filePath);
        if (!Directory.Exists(directoryPath))
        {
            Directory.CreateDirectory(directoryPath);
        }

        File.WriteAllBytes(filePath, data);

        Debug.Log($"File downloaded: {filePath}");
    }


    private void RemoveFile(string filePath)
    {
        if (File.Exists(filePath))
        {
            File.Delete(filePath);
            Debug.Log($"Removed file: {filePath}");
        }
    }
}
*/