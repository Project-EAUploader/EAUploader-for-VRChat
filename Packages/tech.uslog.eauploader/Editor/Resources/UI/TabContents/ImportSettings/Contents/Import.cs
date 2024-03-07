using EAUploader.UI.Components;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace EAUploader.UI.ImportSettings
{
    internal class Import
    {
        private static List<LanguageInfo> languageInfos = LanguageUtility.GetAvailableLanguages();

        public static void ShowContent(VisualElement root)
        {
            var visualTree = Resources.Load<VisualTreeAsset>("UI/TabContents/ImportSettings/Contents/Import");
            visualTree.CloneTree(root);

            root.schedule.Execute(() =>
            {
                if (EAUploaderCore.HasVRM)
                {
                    root.Q<ShadowButton>("import_vrm").SetEnabled(true);
                }
                else
                {
                    root.Q<ShadowButton>("import_vrm").SetEnabled(false);
                }
            }).Every(1000);

            root.Q<ShadowButton>("import_prefab").clicked += ImportPrefabButtonClicked;
            root.Q<ShadowButton>("import_folder").clicked += ImportFolderButtonClicked;
            root.Q<ShadowButton>("import_vrm").clicked += ImportVRMButtonClicked;

            root.Q<DropdownField>("language").choices = languageInfos.Select(x => x.display).ToList();
            root.Q<DropdownField>("language").index = languageInfos.FindIndex(x => x.name == LanguageUtility.GetCurrentLanguage());
            root.Q<DropdownField>("language").RegisterValueChangedCallback((evt) =>
            {
                if (evt.newValue != null)
                {
                    var selectedLanguage = languageInfos.Find(x => x.display == evt.newValue);
                    if (selectedLanguage != null)
                    {
                        LanguageUtility.ChangeLanguage(selectedLanguage.name);
                    }
                }
            });

            root.Q<Label>("version").text = EAUploaderCore.GetVersion();
            root.Q<ShadowButton>("send_feedback").clicked += () => DiscordWebhookSender.OpenDiscordWebhookSenderWindow();
        }

        private static void ImportPrefabButtonClicked()
        {
            ImportAsset(EditorUtility.OpenFilePanelWithFilters(T7e.Get("Import Asset"), "", new[] { T7e.Get("Import a .prefab file or .unitypackage file."), "prefab,unitypackage", "All files", "*" }));
        }

        private static void ImportFolderButtonClicked()
        {
            ImportAllAssetsFromFolder(EditorUtility.OpenFolderPanel(T7e.Get("Import from folder"), "", ""));
        }

        private static void ImportVRMButtonClicked()
        {
            EditorApplication.ExecuteMenuItem("VRM0/Import from VRM 0.x");
        }

        private static void ImportAsset(string filePath)
        {
            if (string.IsNullOrEmpty(filePath)) return;

            var fileExtension = Path.GetExtension(filePath)?.ToLower();

            switch (fileExtension)
            {
                case ".prefab":
                    AssetDatabase.ImportAsset(filePath, ImportAssetOptions.Default);
                    break;
                case ".unitypackage":
                    AssetDatabase.ImportPackage(filePath, true);
                    break;
            }
            AssetDatabase.Refresh();
        }

        private static void ImportAllAssetsFromFolder(string folderPath)
        {
            if (string.IsNullOrEmpty(folderPath)) return;

            var allFiles = Directory.GetFiles(folderPath, "*.*", SearchOption.AllDirectories)
                .Where(s => s.EndsWith(".prefab") || s.EndsWith(".unitypackage")).ToList();

            if (allFiles.Count == 0)
            {
                EditorUtility.DisplayDialog(T7e.Get("Asset not found"), T7e.Get("There are no prefab or unitypackage files in the selected folder."), "OK");
                return;
            }

            var fileList = string.Join("\n", allFiles.Select(Path.GetFileName));
            var confirmImport = EditorUtility.DisplayDialog(T7e.Get("Confirm Import"), $"{T7e.Get("The following files will be imported")}:\n{fileList}", T7e.Get("Import"), T7e.Get("Cancel"));
            if (!confirmImport) return;

            foreach (var file in allFiles)
            {
                ImportAsset(file);
            }
        }
    }
}
