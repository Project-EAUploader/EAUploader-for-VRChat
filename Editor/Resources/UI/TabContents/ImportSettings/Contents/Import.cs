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
                    root.Q<Button>("import_vrm").SetEnabled(true);
                }
                else 
                {
                    root.Q<Button>("import_vrm").SetEnabled(false);
                }
            }).Every(1000);

            root.Q<Button>("import_prefab").clicked += ImportPrefabButtonClicked;
            root.Q<Button>("import_folder").clicked += ImportFolderButtonClicked;
            root.Q<Button>("import_vrm").clicked += ImportVRMButtonClicked;

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
            root.Q<Button>("send_feedback").clicked += () => DiscordWebhookSender.OpenDiscordWebhookSenderWindow(); ;
        }

        private static void ImportPrefabButtonClicked()
        {
            ImportAsset(EditorUtility.OpenFilePanelWithFilters(Translate.Get("Import Asset"), "", new[] { Translate.Get("Import a .prefab file or .unitypackage file."), "prefab,unitypackage", "All files", "*" }));
        }

        private static void ImportFolderButtonClicked()
        {
            ImportAllAssetsFromFolder(EditorUtility.OpenFolderPanel(Translate.Get("Import from folder"), "", ""));
        }

        private static void ImportVRMButtonClicked()
        {
            EditorApplication.ExecuteMenuItem("VRM0/Import from VRM 0.x");
        }

        private static void ImportAsset(string filePath)
        {
            if (string.IsNullOrEmpty(filePath)) return;

            var fileExtension = Path.GetExtension(filePath)?.ToLower();
            var fileName = Path.GetFileNameWithoutExtension(filePath);

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
                EditorUtility.DisplayDialog(Translate.Get("Asset not found"), Translate.Get("There are no prefab or unitypackage files in the selected folder."), "OK");
                return;
            }

            var fileList = string.Join("\n", allFiles.Select(Path.GetFileName));
            var confirmImport = EditorUtility.DisplayDialog(Translate.Get("Confirm Import"), $"{Translate.Get("The following files will be imported")}:\n{fileList}", Translate.Get("Import"), Translate.Get("Cancel"));
            if (!confirmImport) return;

            foreach (var file in allFiles)
            {
                ImportAsset(file);
            }
        }
    }
}
