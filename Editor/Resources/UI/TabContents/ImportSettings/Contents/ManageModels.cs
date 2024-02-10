using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace EAUploader_beta.UI.ImportSettings
{
    internal class ManageModels
    {
        private static Dictionary<string, Texture2D> prefabsWithPreview = new Dictionary<string, Texture2D>();
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
            prefabsWithPreview = CustomPrefabUtility.GetPrefabList()
                .Where(kvp => string.IsNullOrEmpty(searchValue) || kvp.Key.ToLower().Contains(searchValue.ToLower()))
                .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
        }

        private static void AddPrefabsToModelList()
        {
            foreach (var prefab in prefabsWithPreview)
            {
                var item = CreatePrefabItem(prefab);
                modelList.Add(item);
            }
        }

        private static VisualElement CreatePrefabItem(KeyValuePair<string, Texture2D> prefab)
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

            var preview = new Image { image = prefab.Value, style = { width = 100, height = 100 } };
            item.Add(preview);

            var label = new Label { text = Path.GetFileNameWithoutExtension(prefab.Key) };
            item.Add(label);

            var buttons = new[]
            {
                new { Text = "Change Name", Action = (Action)(() => ChangePrefabName(prefab.Key)) },
                new { Text = "Copy as New Name", Action = (Action)(() => CopyPrefabAsNewName(prefab.Key)) },
                new { Text = "Delete", Action = (Action)(() => DeletePrefab(prefab.Key)) }
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
            if (Prefab.RenamePrefab.ShowWindow(prefabPath))
                UpdateModelList();
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

                Prefab.RenamePrefab.ShowWindow(newPrefabPath);

                UpdateModelList();
            }
        }

        private static void DeletePrefab(string prefabPath)
        {
            if (EditorUtility.DisplayDialog("Prefabの消去", "本当にPrefabを消去しますか？", "消去", "キャンセル"))
            {
                CustomPrefabUtility.DeletePrefabPreview(prefabPath);
                CustomPrefabUtility.RemovePrefabFromScene(prefabPath);
                AssetDatabase.DeleteAsset(prefabPath);
                UpdateModelList();
            }
        }
    }
}
