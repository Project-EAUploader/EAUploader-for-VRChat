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
            var visualTree = Resources.Load<VisualTreeAsset>("UI/TabContents/ImportSettings/Contents/PrefabItem");
            visualTree.CloneTree(this);

            var previewImage = this.Q<Image>("previewImage");
            previewImage.image = prefab.Preview;
            previewImage.RegisterCallback<MouseUpEvent>(evt => ShowLargeImage(prefab.Preview));

            var name = this.Q<Label>("nameLabel");
            name.text = prefab.Name;

            var lastModified = this.Q<Label>("lastModifiedLabel");
            lastModified.text = prefab.LastModified.ToString("yyyy/MM/dd HH:mm:ss");

            var controls = this.Q<VisualElement>("controls");
            var changeNameButton = this.Q<Button>("changeNameButton");
            changeNameButton.clicked += () => ChangePrefabName(prefab.Path);

            var copyAsNewNameButton = this.Q<Button>("copyAsNewNameButton");
            copyAsNewNameButton.clicked += () => CopyPrefabAsNewName(prefab.Path);

            var deleteButton = this.Q<Button>("deleteButton");
            deleteButton.clicked += () => DeletePrefab(prefab.Path);
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

                var renameWindow = ScriptableObject.CreateInstance<RenamePrefabWindow>();
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
