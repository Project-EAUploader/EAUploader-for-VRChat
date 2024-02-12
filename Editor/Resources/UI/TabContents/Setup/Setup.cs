using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace EAUploader.UI.Setup
{
    internal class Main
    {
        private static Dictionary<string, Texture2D> prefabsWithPreview = new Dictionary<string, Texture2D>();
        private static VisualElement root;
        private static ScrollView modelList;
        private static Components.Preview preview;

        public static void ShowContent(VisualElement rootElement)
        {
            root = rootElement;

            var visualTree = Resources.Load<VisualTreeAsset>("UI/TabContents/Setup/Setup");
            visualTree.CloneTree(root);

            modelList = root.Q<ScrollView>("model_list");

            GetModelList();

            preview = new Components.Preview(root.Q("avatar_preview"), EAUploaderCore.selectedPrefabPath);

            preview.ShowContent();

            ButtonClickHandler();
        }

        private static void GetModelList()
        {
            prefabsWithPreview = CustomPrefabUtility.PrefabManager.GetPrefabList();
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

        private static VisualElement CreatePrefabItem(KeyValuePair<string, Texture2D> prefab)
        {
            var item = new Button(() =>
            {
                EAUploaderCore.selectedPrefabPath = prefab.Key;
                preview.UpdatePreview(prefab.Key);
            })
            {
                style =
                            {
                                flexDirection = FlexDirection.Row,
                                alignItems = Align.Center,
                                marginTop = 5
                            }
            };

            var previewImage = new Image { image = prefab.Value, scaleMode = ScaleMode.ScaleToFit, style = { width = 100, height = 100 } };
            item.Add(previewImage);

            var label = new Label(Path.GetFileNameWithoutExtension(prefab.Key));
            item.Add(label);

            return item;
        }

        private static void ButtonClickHandler()
        {
            var resetButton = root.Q<Button>("reset_view");
            resetButton.clicked += ResetButtonClicked;
            var changeNameButton = root.Q<Button>("change_name");
            changeNameButton.clicked += ChangeNameButtonClicked;
            var deleteButton = root.Q<Button>("delete_model");
            deleteButton.clicked += DeleteButtonClicked;
        }

        private static void ResetButtonClicked()
        {
            preview.ResetPreview();
        }

        private static void ChangeNameButtonClicked()
        {
            var renameWindow = ScriptableObject.CreateInstance<CustomPrefabUtility.RenamePrefabWindow>();
            if (renameWindow.ShowWindow(EAUploaderCore.selectedPrefabPath)) 
            {
                GetModelList();
            }
        }

        private static void DeleteButtonClicked()
        {
            if (CustomPrefabUtility.PrefabManager.ShowDeletePrefabDialog(EAUploaderCore.selectedPrefabPath))
            {
                GetModelList();
            }
        }
    }
}