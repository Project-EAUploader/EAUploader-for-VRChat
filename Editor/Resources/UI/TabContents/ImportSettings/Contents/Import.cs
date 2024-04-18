using EAUploader.CustomPrefabUtility;
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
        private static List<ThemeInfo> themeInfos = ThemeUtility.GetAvailableThemes();

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
            root.Q<ShadowButton>("import_shaders").clicked += ImportShadowButtonClicked;
            root.Q<ShadowButton>("show_existing_shaders").clicked += ShowExistingShaders;
            root.Q<ShadowButton>("import_folder").clicked += ImportFolderButtonClicked;
            root.Q<ShadowButton>("import_vrm").clicked += ImportVRMButtonClicked;

            root.Q<DropdownField>("language").choices = languageInfos.Select(x => x.display).ToList();
            root.Q<DropdownField>("language").index = languageInfos.FindIndex(x => x.name == LanguageUtility.GetCurrentLanguage());
            root.Q<DropdownField>("language").RegisterValueChangedCallback((evt) =>
            {
                if (evt.newValue != null)
                {
                    var selectedLanguage = languageInfos.Find(x => x.display == evt.newValue);
                    if (selectedLanguage != null && selectedLanguage.name != LanguageUtility.GetCurrentLanguage())
                    {
                        var previousLanguage = LanguageUtility.GetCurrentLanguage();
                        var confirmed = ShowConfirmationDialog();
                        if (confirmed)
                        {
                            LanguageUtility.ChangeLanguage(selectedLanguage.name);
                            ExecuteMenuItem("VRChat SDK/Reload SDK");
                        }
                        else
                        {
                            root.Q<DropdownField>("language").value = languageInfos.Find(x => x.name == previousLanguage).display;
                        }
                    }
                }
            });

            root.Q<DropdownField>("thema").choices = themeInfos.Select(x => x.display).ToList();
            root.Q<DropdownField>("thema").index = themeInfos.FindIndex(x => x.name == ThemeUtility.GetCurrentTheme());
            root.Q<DropdownField>("thema").RegisterValueChangedCallback((evt) =>
            {
                if (evt.newValue != null)
                {
                    var selectedTheme = themeInfos.Find(x => x.display == evt.newValue);
                    if (selectedTheme != null && selectedTheme.name != ThemeUtility.GetCurrentTheme())
                    {
                        ThemeUtility.ChangeTheme(selectedTheme.name);
                        ApplyTheme(selectedTheme.name);
                    }
                }
            });

            ApplyTheme(ThemeUtility.GetCurrentTheme());

            root.Q<Label>("version").text = EAUploaderCore.GetVersion();
            root.Q<ShadowButton>("send_feedback").clicked += () => DiscordWebhookSender.OpenDiscordWebhookSenderWindow();
            root.Q<ShadowButton>("exit_unity").clicked += () =>
            {
                if (EditorUtility.DisplayDialog(T7e.Get("Confirm Exit"), T7e.Get("Are you sure you want to exit Unity?"), T7e.Get("Yes"), T7e.Get("No")))
                {
                    EditorApplication.Exit(0);
                }
            };
        }

        private static void ImportPrefabButtonClicked()
        {
            ImportAsset(EditorUtility.OpenFilePanelWithFilters(T7e.Get("Import Avatar"), "", new[] { T7e.Get("Import a .prefab file or .unitypackage file."), "prefab,unitypackage", "All files", "*" }));
        }

        private static void ImportShadowButtonClicked()
        {
            ImportAsset(EditorUtility.OpenFilePanelWithFilters(T7e.Get("Import Shaders"), "", new[] { T7e.Get("Import a .unitypackage file."), "unitypackage", "All files", "*" }));
        }

        private static void ShowExistingShaders()
        {
            var shaderList = ShaderChecker.GetExistingShaders();
            var shaderListString = string.Join("\n", shaderList);
            EditorUtility.DisplayDialog(T7e.Get("Existing Shaders"), shaderListString, "OK");
        }

        private static void ImportFolderButtonClicked()
        {
            ImportAllAssetsFromFolder(EditorUtility.OpenFolderPanel(T7e.Get("Import from folder"), "", ""));
        }

        private static void ImportVRMButtonClicked()
        {
#if HAS_VRM
            VRMImporter.ImportVRM();
#endif
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
                    AssetDatabase.ImportPackage(filePath, false);
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

        private static bool ShowConfirmationDialog()
        {
            return EditorUtility.DisplayDialog(T7e.Get("Language Change Confirmation"), T7e.Get("Are you sure you want to change the language?"), T7e.Get("Yes"), T7e.Get("No"));
        }

        private static void ExecuteMenuItem(string menuPath)
        {
            var menuItem = EditorApplication.ExecuteMenuItem(menuPath);
            if (!menuItem)
            {
                Debug.LogError($"Failed to execute menu item: {menuPath}");
            }
        }

        private static void ApplyTheme(string theme)
        {
            var root = EditorWindow.GetWindow<EAUploader>().rootVisualElement;
            root.RemoveFromClassList("white");
            root.RemoveFromClassList("dark");
            root.AddToClassList(theme);
        }
    }

    internal class ThemeInfo
    {
        public string name;
        public string display;

        public ThemeInfo(string name, string display)
        {
            this.name = name;
            this.display = display;
        }
    }

    internal class ThemeUtility
    {
        private static List<ThemeInfo> availableThemes = new List<ThemeInfo>
        {
            new ThemeInfo("white", T7e.Get("Light")),
            new ThemeInfo("dark", T7e.Get("Dark"))
        };

        public static List<ThemeInfo> GetAvailableThemes() => availableThemes;

        public static string GetCurrentTheme()
        {
            return EditorPrefs.GetString("EAUploader.Theme", "white");
        }

        public static void ChangeTheme(string theme)
        {
            EditorPrefs.SetString("EAUploader.Theme", theme);
        }
    }
}