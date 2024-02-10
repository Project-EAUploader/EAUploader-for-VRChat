using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace EAUploader_beta.UI.Setup
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

            preview = new Components.Preview();

            preview.ShowContent(root.Q("avatar_preview"));

            ButtonClickHandler();
        }

        private static void GetModelList()
        {
            prefabsWithPreview = CustomPrefabUtility.GetPrefabList();
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
            var item = new Button(() => preview.UpdatePreview(prefab.Key))
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
        }

        private static void ResetButtonClicked()
        {
            preview.ResetPreview();
        }

        private static void ChangeNameButtonClicked()
        {
            if (Prefab.RenamePrefab.ShowWindow(EAUploaderCore.selectedPrefabPath))
            {
                GetModelList();
            }
        }
    }
}