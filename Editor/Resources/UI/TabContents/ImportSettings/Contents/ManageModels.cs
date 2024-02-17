    using Cysharp.Threading.Tasks;
using EAUploader.CustomPrefabUtility;
using System;
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

            var searchButton = root.Q<Button>("searchButton");
            searchButton.clicked += UpdateModelList;

            UpdateModelList();
        }

        private static void UpdateModelList()
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

        private static void ShowLargeImage(Texture2D image)
        {
            var window = ScriptableObject.CreateInstance<EditorWindow>();
            window.titleContent = new GUIContent("プレビュー画面");
            window.minSize = new Vector2(500, 500);

            var preview = new Image { image = image, style = { flexGrow = 1 } };
            window.rootVisualElement.Add(preview);

            window.Show();
        }

        private static VisualElement CreatePrefabItem(PrefabInfo prefab)
        {
            var item = new VisualElement
            {
                style =
                {
                    flexDirection = new StyleEnum<FlexDirection>(FlexDirection.Row),
                    flexWrap = new StyleEnum<Wrap>(Wrap.Wrap),
                    alignItems = new StyleEnum<Align>(Align.Center),
                    paddingTop = 10
                }
            };

            var preview = new Image { image = prefab.Preview, style = { width = 100, height = 100 } };
            preview.RegisterCallback<MouseUpEvent>(evt => ShowLargeImage(prefab.Preview));
            item.Add(preview);

            var label = new Label { text = Path.GetFileNameWithoutExtension(prefab.Path) };
            item.Add(label);

            var buttons = new[]
            {
                new { Text = "Change Name", Action = (Action)(() => ChangePrefabName(prefab.Path)) },
                new { Text = "Copy as New Name", Action = (Action)(() => CopyPrefabAsNewName(prefab.Path)) },
                new { Text = "Delete", Action = (Action)(() => DeletePrefab(prefab.Path)) }
            };

            foreach (var button in buttons)
            {
                var buttonElement = new Button { text = button.Text };
                buttonElement.clicked += button.Action;
                item.Add(buttonElement);
            }

            return item;
        }
       
        private static void ChangePrefabName(string prefabPath)
        {
            var renameWindow = ScriptableObject.CreateInstance<CustomPrefabUtility.RenamePrefabWindow>();
            if (renameWindow.ShowWindow(prefabPath)) UpdateModelList();
        }

        private static void CopyPrefabAsNewName(string prefabPath)
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

                var renameWindow = new CustomPrefabUtility.RenamePrefabWindow();
                renameWindow.ShowWindow(newPrefabPath);

                UpdateModelList();
            }
        }

        private static void DeletePrefab(string prefabPath)
        {
            if (CustomPrefabUtility.PrefabManager.ShowDeletePrefabDialog(prefabPath)) 
            {
                UpdateModelList();
            }
        }
    }
}
