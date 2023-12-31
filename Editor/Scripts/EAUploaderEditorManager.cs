using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Linq;
using System;

public static class EAUploaderEditorManager
{

    public static void OnEditorManagerLoad()
    {
        ClearJsonFile();
        LoadEditorInfoFromJson();
    }

    static EAUploaderEditorManager()
    {

        ClearJsonFile();
    }

    private static List<EditorRegistration> registeredEditors = new List<EditorRegistration>();

    public static void RegisterEditor(EditorRegistration editorRegistration)
    {
        if (editorRegistration != null && !registeredEditors.Contains(editorRegistration))
        {
            registeredEditors.Add(editorRegistration);


            SaveEditorInfoToJson();
        }
    }

    private static void LoadEditorInfoFromJson()
    {
        if (File.Exists(JsonFilePath))
        {
            string json = File.ReadAllText(JsonFilePath);
            var editorsList = JsonUtility.FromJson<EditorInfoList>(json);


            if (editorsList?.editors != null)
            {
                foreach (var editorInfo in editorsList.editors)
                {
                    registeredEditors.Add(new EditorRegistration
                    {
                        EditorName = editorInfo.EditorName,
                        Description = editorInfo.Description,
                        Version = editorInfo.Version,
                        Author = editorInfo.Author,
                        Url = editorInfo.Url
                    });
                }
            }
        }
    }

    public static IEnumerable<EditorRegistration> GetRegisteredEditors()
    {
        return registeredEditors;
    }

    private static string JsonFilePath => "Packages/com.sabuworks.eauploader/Editor/Scripts/MainWindow/TabContents/SetUp/EditorsManage.json";

    private static void SaveEditorInfoToJson()
    {

        var editorInfos = registeredEditors.Select(editor => new EditorInfo
        {
            MenuName = editor.MenuName,
            EditorName = editor.EditorName,
            Description = editor.Description,
            Version = editor.Version,
            Author = editor.Author,
            Url = editor.Url
        }).ToList();

        var editorsWrapper = new EditorInfoList { editors = editorInfos };

        try
        {
            string json = JsonUtility.ToJson(editorsWrapper, true);
            File.WriteAllText(JsonFilePath, json);
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to save editor information to JSON: {e.Message}");
        }
    }

    private static void ClearJsonFile()
    {
        try
        {

            var editorsWrapper = new EditorInfoList { editors = new List<EditorInfo>() };
            string json = JsonUtility.ToJson(editorsWrapper, true);
            File.WriteAllText(JsonFilePath, json);
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to clear editor information from JSON: {e.Message}");
        }
    }

    [System.Serializable]
    private class EditorInfoList
    {
        public List<EditorInfo> editors;
    }

    [System.Serializable]
    private class EditorInfo
    {
        public string MenuName;
        public string EditorName;
        public string Description;
        public string Version;
        public string Author;
        public string Url;
    }

}

public class EditorRegistration
{
    public string MenuName { get; set; }
    public string EditorName { get; set; }
    public string Description { get; set; }
    public string Version { get; set; }
    public string Author { get; set; }
    public string Url { get; set; }
}
