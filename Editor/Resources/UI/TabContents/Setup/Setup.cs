using Cysharp.Threading.Tasks;
using EAUploader.CustomPrefabUtility;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using static UnityEditor.Progress;

namespace EAUploader.UI.Setup
{
    internal class Main
    {
        private static List<PrefabInfo> prefabsWithPreview = new List<PrefabInfo>();
        private static VisualElement root;
        private static ScrollView modelList;
        internal static Components.Preview preview;
        private static bool isEditorInfoLoaded = false;
        private static List<EditorRegistration> editorRegistrations;
        private static Dictionary<string, bool> editorFoldouts = new Dictionary<string, bool>();

        public static void ShowContent(VisualElement rootElement)
        {
            root = rootElement;

            var visualTree = Resources.Load<VisualTreeAsset>("UI/TabContents/Setup/Setup");
            visualTree.CloneTree(root);

            modelList = root.Q<ScrollView>("model_list");

            GetModelList();

            preview = new Components.Preview(root.Q("avatar_preview"), EAUploaderCore.selectedPrefabPath);

            preview.ShowContent();

            if (EAUploaderCore.selectedPrefabPath != null)
            {
                UpdatePrefabInto(EAUploaderCore.selectedPrefabPath);
                preview.UpdatePreview(EAUploaderCore.selectedPrefabPath);
            }

            BuildEditor();

            ButtonClickHandler();

            root.Q<Button>("find_extentions").clicked += () =>
            {
                Application.OpenURL("https://www.uslog.tech/eauploader-plug-ins");
            };
        }

        private static void GetModelList()
        {
            prefabsWithPreview = CustomPrefabUtility.PrefabManager.GetAllPrefabsWithPreview();
            modelList.Clear();
            AddPrefabsToModelList();
        }

        private static void AddPrefabsToModelList()
        {
            foreach (var prefab in prefabsWithPreview)
            {
                var item = CreatePrefabItem(prefab);
                modelList.Add(item);
            }
        }

        private static VisualElement CreatePrefabItem(PrefabInfo prefab)
        {
            var item = new PrefabItemButton(prefab);
            return item;
        }

        internal static void UpdatePrefabInto(string prefabPath)
        {
            var prefabInfo = root.Q<VisualElement>("prefab_info");
            prefabInfo.Clear();

            var prefabName = new Label(T7e.Get("Now selecting: ") + Path.GetFileNameWithoutExtension(prefabPath))
            {
                style =
                {
                    fontSize = 20,
                    unityFontStyleAndWeight = FontStyle.Bold
                }
            };
            prefabInfo.Add(prefabName);

            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
            var prefabHeight = new Label(T7e.Get("Height: ") + Utility.GetAvatarHeight(prefab) + "m")
            {
                style =
                {
                    fontSize = 15,
                    unityFontStyleAndWeight = FontStyle.Normal
                }
            };
            prefabInfo.Add(prefabHeight);
        }

        private static void ButtonClickHandler()
        {
            var changeNameButton = root.Q<Button>("change_name");
            changeNameButton.clicked += ChangeNameButtonClicked;
            var pinButton = root.Q<Button>("pin_model");
            pinButton.clicked += PinButtonClicked;
            var deleteButton = root.Q<Button>("delete_model");
            deleteButton.clicked += DeleteButtonClicked;
        }

        private static void ChangeNameButtonClicked()
        {
            var renameWindow = ScriptableObject.CreateInstance<CustomPrefabUtility.RenamePrefabWindow>();
            if (renameWindow.ShowWindow(EAUploaderCore.selectedPrefabPath))
            {
                GetModelList();
            }
        }

        private static void PinButtonClicked()
        {
            CustomPrefabUtility.PrefabManager.PinPrefab(EAUploaderCore.selectedPrefabPath);
            GetModelList();
        }

        private static void DeleteButtonClicked()
        {
            if (CustomPrefabUtility.PrefabManager.ShowDeletePrefabDialog(EAUploaderCore.selectedPrefabPath))
            {
                GetModelList();
            }
        }

        private static void BuildEditor()
        {
            if (!isEditorInfoLoaded)
            {
                editorRegistrations = new List<EditorRegistration>(EAUploaderEditorManager.GetRegisteredEditors());
                foreach (var editor in editorRegistrations)
                {
                    editorFoldouts[editor.EditorName] = false;
                }
                isEditorInfoLoaded = true;
            }

            var editorList = root.Q<VisualElement>("avatar_editor_list");
            editorList.Clear();

            foreach (var editor in editorRegistrations)
            {
                var editorItem = CreateEditorItem(editor);
                editorList.Add(editorItem);
            }
        }

        private static VisualElement CreateEditorItem(EditorRegistration editor)
        {
            var item = new VisualElement
            {
                style =
                {
                    alignItems = Align.Center,
                    marginTop = 5
                }
            };

            var item_button = new Button(() =>
            {
                EditorApplication.ExecuteMenuItem(editor.MenuName);
            })
            {
                text = editor.EditorName
            };

            item.Add(item_button);

            var foldoutButton = new Button(() =>
            {
                editorFoldouts[editor.EditorName] = !editorFoldouts[editor.EditorName];
                BuildEditor();
            })
            {
                text = "More"
            };

            item.Add(foldoutButton);

            if (editorFoldouts[editor.EditorName])
            {
                var editorContent = new VisualElement();
                editorContent.Add(new Label(T7e.Get("Description: ") + editor.EditorName));
                editorContent.Add(new Label(T7e.Get("Version: ") + editor.MenuName));
                editorContent.Add(new Label(T7e.Get("Author: ") + editor.Author));

                if (!string.IsNullOrEmpty(editor.Url))
                {
                    editorContent.Add(new Button(() =>
                    {
                        Application.OpenURL(editor.Url);
                    })
                    {
                        text = T7e.Get("Open the URL")
                    });
                }

                item.Add(editorContent);
            }

            return item;
        }
    }

    internal class PrefabItemButton : Button
    {
        public PrefabItemButton(PrefabInfo prefab)
        {
            var previewImage = new Image { image = prefab.Preview, scaleMode = ScaleMode.ScaleToFit, style = { width = 100, height = 100 } };
            Add(previewImage);

            var label = new Label(Path.GetFileNameWithoutExtension(prefab.Path));
            Add(label);

            clicked += () =>
            {
                EAUploaderCore.selectedPrefabPath = prefab.Path;
                Main.UpdatePrefabInto(prefab.Path);
                Main.preview.UpdatePreview(prefab.Path);
            };
        }
    }
}