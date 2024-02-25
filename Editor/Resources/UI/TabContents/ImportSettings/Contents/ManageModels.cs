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
    internal class ManageModels
    {
        private static List<PrefabInfo> prefabsWithPreview = new List<PrefabInfo>();
        private static VisualElement root;
        private static ScrollView modelList;

        public static void ShowContent(VisualElement rootElement)
        {
            root = rootElement;
            var visualTree = Resources.Load<VisualTreeAsset>("UI/TabContents/ImportSettings/Contents/ManageModels");
            visualTree.CloneTree(root);

            modelList = root.Q<ScrollView>("model_list");

            var searchButton = root.Q<ShadowButton>("searchButton");
            searchButton.clicked += UpdateModelList;

            UpdateModelList();
        }

        internal static void UpdateModelList()
        {
            var searchQuery = root.Q<TextField>("searchQuery").value;
            UpdatePrefabsWithPreview(searchQuery);

            modelList.Clear();
            AddPrefabsToModelList();
        }

        private static void UpdatePrefabsWithPreview(string searchValue = "")
        {
            prefabsWithPreview = PrefabManager.GetAllPrefabsWithPreview();
            if (!string.IsNullOrEmpty(searchValue))
            {
                prefabsWithPreview = prefabsWithPreview.Where(prefab => prefab.Name.Contains(searchValue)).ToList();
            }
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
            var item = new PrefabItem(prefab);

            return item;
        }

    }

    internal class PrefabItem : VisualElement
    {
        public PrefabItem(PrefabInfo prefab)
        {
            var previewImage = new Image
            {
                image = prefab.Preview,
                style = {
                    width = 128,
                    height = 128,
                }
            };
            previewImage.RegisterCallback<MouseUpEvent>(evt => ShowLargeImage(prefab.Preview));
            Add(previewImage);

            var label = new Label { text = prefab.Name, style = { flexGrow = 1, flexShrink = 1 } };
            Add(label);

            var controls = new VisualElement()
            {
                style =
                {
                    position = Position.Absolute,
                    right = 0,
                    flexDirection = FlexDirection.Column,
                    height = new StyleLength(new Length(100, LengthUnit.Percent)),
                }
            };

            var changeNameButton = new Button()
            {
                style =
                {
                    borderBottomLeftRadius = 0,
                    borderBottomRightRadius = 0,
                    flexGrow = 1,
                    justifyContent = Justify.Center
                }
            };
            var changeNameIcon = new MaterialIcon() { icon = "edit", style = { fontSize = 20 } };
            changeNameButton.Add(changeNameIcon);
            changeNameButton.clicked += () => ChangePrefabName(prefab.Path);

            var copyAsNewNameButton = new Button()
            {
                style =
                {
                    borderBottomLeftRadius = 0,
                    borderBottomRightRadius = 0,
                    borderTopLeftRadius = 0,
                    borderTopRightRadius = 0,
                    borderBottomColor = new StyleColor(new Color(0.0784313725f , 0.3921568627f, 0.7058823529f,1)),
                    borderTopColor = new StyleColor(new Color(0.0784313725f , 0.3921568627f, 0.7058823529f,1)),
                    borderBottomWidth = 1,
                    borderTopWidth = 1,
                    flexGrow = 1,
                    justifyContent = Justify.Center
                }
            };
            var copyAsNewNameIcon = new MaterialIcon() { icon = "content_copy", style = { fontSize = 20 } };
            copyAsNewNameButton.Add(copyAsNewNameIcon);
            copyAsNewNameButton.clicked += () => CopyPrefabAsNewName(prefab.Path);

            var deleteButton = new Button()
            {
                style =
                {
                    borderTopLeftRadius = 0,
                    borderTopRightRadius = 0,
                    flexGrow = 1,
                    justifyContent = Justify.Center
                }
            };
            var deleteIcon = new MaterialIcon() { icon = "delete", style = { fontSize = 20 } };
            deleteButton.Add(deleteIcon);
            deleteButton.clicked += () => DeletePrefab(prefab.Path);

            controls.Add(changeNameButton);
            controls.Add(copyAsNewNameButton);
            controls.Add(deleteButton);

            Add(controls);
        }

        private static void ShowLargeImage(Texture2D image)
        {
            var window = ScriptableObject.CreateInstance<EditorWindow>();
            window.titleContent = new GUIContent(UnityEditor.L10n.Tr("Preview"));
            window.minSize = new Vector2(500, 500);

            var preview = new Image { image = image, style = { flexGrow = 1 } };
            window.rootVisualElement.Add(preview);

            window.Show();
        }

        internal static void ChangePrefabName(string prefabPath)
        {
            var renameWindow = ScriptableObject.CreateInstance<RenamePrefabWindow>();
            if (renameWindow.ShowWindow(prefabPath)) ManageModels.UpdateModelList();
        }

        internal static void CopyPrefabAsNewName(string prefabPath)
        {
            string assetName = Path.GetFileNameWithoutExtension(prefabPath);
            string directoryPath = Path.GetDirectoryName(prefabPath);
            string newAssetName = assetName + "_Copy";
            string newPrefabPath = AssetDatabase.GenerateUniqueAssetPath(Path.Combine(directoryPath, newAssetName + ".prefab"));

            UnityEngine.Object originalPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
            if (originalPrefab != null)
            {
                UnityEngine.Object prefabCopy = UnityEngine.Object.Instantiate(originalPrefab);
                PrefabUtility.SaveAsPrefabAsset((GameObject)prefabCopy, newPrefabPath);
                UnityEngine.Object.DestroyImmediate(prefabCopy);

                var renameWindow = new RenamePrefabWindow();
                renameWindow.ShowWindow(newPrefabPath);

                ManageModels.UpdateModelList();
            }
        }

        internal static void DeletePrefab(string prefabPath)
        {
            if (PrefabManager.ShowDeletePrefabDialog(prefabPath))
            {
                ManageModels.UpdateModelList();
            }
        }
    }
}
