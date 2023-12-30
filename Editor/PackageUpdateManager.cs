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
        GetWindow<UpdateManager>("Update Manager").StartUpdateProcess();
    }

    private void OnGUI()
    {
        GUI.backgroundColor = Color.white;

        if (!updateCompleted)
        {
            string localVersion = ReadLocalVersion();
            GUILayout.Label($"{localVersion} から {latestVersion} へ更新中...");
        }
        else
        {
            GUILayout.Label("更新が完了しました。");

            if (GUILayout.Button("EAUploaderを開く"))
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
        await DownloadUpdateInfo();
    }

    private async UniTask DownloadUpdateInfo()
    {

        UnityWebRequest request = UnityWebRequest.Get(updateInfoUrl);
        await request.SendWebRequest();

        if (request.result != UnityWebRequest.Result.Success)
        {
            return;
        }

        string json = request.downloadHandler.text;

        UpdateInfoArray updateArray = JsonUtility.FromJson<UpdateInfoArray>(json);
        if (updateArray == null || updateArray.updates == null)
        {
            return;
        }

        await ProcessAllUpdates(updateArray.updates);
    }


    private async Task ProcessAllUpdates(UpdateInfo[] allUpdates)
    {
        if (allUpdates == null)
        {
            this.Close();
            return;
        }

        string localVersion = ReadLocalVersion();
        var updatesToApply = allUpdates.Where(u => IsNewerVersion(u.version, localVersion)).ToArray();

        if (updatesToApply.Length == 0)
        {
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
                await StartFileUpdate(file.path, baseUrl + file.url);
            }
        }

        var latestVersion = allUpdates.Select(u => new Version(u.version)).OrderByDescending(v => v).FirstOrDefault();

        if (latestVersion != null)
        {
            await UpdateLocalVersionFile(latestVersion.ToString());
            OnUpdateCompleted(latestVersion.ToString());
        }

        return;
    }

    private async Task UpdateLocalVersionFile(string newVersion)
    {

        UpdateInfo localVersionInfo = new UpdateInfo { version = newVersion };

        string json = JsonUtility.ToJson(localVersionInfo);

        File.WriteAllText(localVersionPath, json);
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
    }


    private void RemoveFile(string filePath)
    {
        if (File.Exists(filePath))
        {
            File.Delete(filePath);
        }
    }
}
