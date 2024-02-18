using Cysharp.Threading.Tasks;
using EAUploader.CustomPrefabUtility;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace EAUploader.UI.Setup
{
    internal class Main
    {
        private static List<PrefabInfo> prefabsWithPreview = new List<PrefabInfo>();
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
            var item = new Button(() =>
            {
                EAUploaderCore.selectedPrefabPath = prefab.Path;
                UpdatePrefabInto(prefab.Path);
                preview.UpdatePreview(prefab.Path);
            })
            {
                style =
                            {
                                flexDirection = FlexDirection.Row,
                                alignItems = Align.Center,
                                marginTop = 5
                            }
            };

            var previewImage = new Image { image = prefab.Preview, scaleMode = ScaleMode.ScaleToFit, style = { width = 100, height = 100 } };
            item.Add(previewImage);

            var label = new Label(Path.GetFileNameWithoutExtension(prefab.Path));
            item.Add(label);

            return item;
        }

        private static void UpdatePrefabInto(string prefabPath)
        {
            var prefabInfo = root.Q<VisualElement>("prefab_info");
            prefabInfo.Clear();

            var prefabName = new Label(Path.GetFileNameWithoutExtension(prefabPath));
            prefabInfo.Add(prefabName);

            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
            var prefabHeight = new Label("Height: " + CustomPrefabUtility.Utility.GetAvatarHeight(prefab));
            prefabInfo.Add(prefabHeight);
        }

        private static void ButtonClickHandler()
        {
            var resetButton = root.Q<Button>("reset_view");
            resetButton.clicked += ResetButtonClicked; 
            var changeNameButton = root.Q<Button>("change_name");
            changeNameButton.clicked += ChangeNameButtonClicked;
            var pinButton = root.Q<Button>("pin_model");
            pinButton.clicked += PinButtonClicked;
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
    }
}