using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace EAUploader
{
    public class EAUploaderEditorManager
    {
        public delegate void EditorRegisteredHandler(EditorRegistration editorRegistration);
        public static event EditorRegisteredHandler OnEditorRegistered;

        public static void OnEditorManagerLoad()
        {
            EnsureJsonFileExists();
            ClearJsonFile();
            LoadEditorInfoFromJson();
        }

        private static List<EditorRegistration> registeredEditors = new List<EditorRegistration>();

        public static void RegisterEditor(EditorRegistration editorRegistration)
        {
            if (editorRegistration != null && !registeredEditors.Contains(editorRegistration))
            {
                registeredEditors.Add(editorRegistration);
                SaveEditorInfoToJson();

                OnEditorRegistered?.Invoke(editorRegistration);
            }
        }

        private static void LoadEditorInfoFromJson()
        {
            if (File.Exists(JsonFilePath))
            {
                string json = File.ReadAllText(JsonFilePath);
                EditorInfoList editorsList = JsonUtility.FromJson<EditorInfoList>(json);

                if (editorsList?.editors != null)
                {
                    foreach (var editorInfo in editorsList.editors)
                    {
                        EditorRegistration registration = new EditorRegistration
                        {
                            EditorName = editorInfo.EditorName,
                            Description = editorInfo.Description,
                            Version = editorInfo.Version,
                            Author = editorInfo.Author,
                            Url = editorInfo.Url
                        };
                        registeredEditors.Add(registration);
                    }
                }
            }
        }

        public static IEnumerable<EditorRegistration> GetRegisteredEditors()
        {
            return registeredEditors;
        }

        private static string JsonFilePath => "Assets/EAUploader/EditorsManage.json";

        private static void SaveEditorInfoToJson()
        {
            var editorInfos = registeredEditors.Select(editor => new EditorInfo
            {
                EditorName = editor.EditorName,
                Description = editor.Description,
                Version = editor.Version,
                Author = editor.Author,
                Url = editor.Url
            }).ToList();

            var editorsWrapper = new EditorInfoList { editors = editorInfos };

            string json = JsonUtility.ToJson(editorsWrapper, true);
            File.WriteAllText(JsonFilePath, json);
        }

        private static void ClearJsonFile()
        {
            var editorsWrapper = new EditorInfoList { editors = new List<EditorInfo>() };
            string json = JsonUtility.ToJson(editorsWrapper, true);
            File.WriteAllText(JsonFilePath, json);
        }

        private static void EnsureJsonFileExists()
        {
            if (!File.Exists(JsonFilePath))
            {
                string directoryPath = Path.GetDirectoryName(JsonFilePath);
                if (!Directory.Exists(directoryPath))
                {
                    Directory.CreateDirectory(directoryPath);
                }

                File.WriteAllText(JsonFilePath, "{}");
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
}