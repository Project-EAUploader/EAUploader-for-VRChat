using EAUploader.Components;
using EAUploader.CustomPrefabUtility;
using EAUploader.UI.Components;
using EAUploader.UI.Windows;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
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
        private static SortOrder sortOrder = SortOrder.LastModifiedDescending;
        private static FilterOrder filterOrder = FilterOrder.NotShowHiddenModels;

        public enum SortOrder
        {
            LastModifiedDescending,
            LastModifiedAscending,
            NameDescending,
            NameAscending
        }

        public enum FilterOrder
        {
            NotShowHiddenModels,
            ShowHiddenModels,
            ShowOnlyHiddenModels
        }

        public static void ShowContent(VisualElement rootElement)
        {
            root = rootElement;
            var visualTree = Resources.Load<VisualTreeAsset>("UI/TabContents/ImportSettings/Contents/ManageModels");
            visualTree.CloneTree(root);

            modelList = root.Q<ScrollView>("model_list");

            BuildUI();
        }

        internal static async void BuildUI()
        {
            await Task.Yield();
            var searchButton = root.Q<ShadowButton>("searchButton");
            searchButton.clicked += UpdateModelList;

            var sortDropdown = new DropdownField("", new List<string>
            {
                T7e.Get("Last Modified Descending"),
                T7e.Get("Last Modified Ascending"),
                T7e.Get("Name Descending"),
                T7e.Get("Name Ascending")
            }, 0);
            sortDropdown.RegisterValueChangedCallback(evt =>
            {
                sortOrder = (SortOrder)sortDropdown.index;
                UpdateModelList();
            });

            var sortbar = root.Q<VisualElement>("sortbar");
            sortbar.Add(sortDropdown);

            var filterDropdown = new DropdownField("", new List<string>
            {
                T7e.Get("Do not show hidden models"),
                T7e.Get("Show hidden models"),
                T7e.Get("Show only hidden models")
            }, 0);
            filterDropdown.RegisterValueChangedCallback(evt =>
            {
                filterOrder = (FilterOrder)filterDropdown.index;
                UpdateModelList();
            });

            var libraryFoldButton = root.Q<VisualElement>("library_fold_button");
            var icon = libraryFoldButton.Q<MaterialIcon>();
            icon.icon = Main.isLibraryOpen ? "chevron_right" : "chevron_left";
            libraryFoldButton.RegisterCallback<MouseUpEvent>(evt =>
            {
                Main.ToggleLibrary();
                icon.icon = Main.isLibraryOpen ? "chevron_right" : "chevron_left";
            });

            var filterbar = root.Q<VisualElement>("filterbar");
            filterbar.Add(filterDropdown);

            if (EAUploaderCore.HasVRM)
            {
                root.Q<VisualElement>("drop_model").Q<Label>("drop_model_label").text = T7e.Get("Drop files(.unitypackage, .prefab, .vrm(ver. 0.x)) to import");
            }

            root.RegisterCallback<DragEnterEvent>(OnDragEnter);
            root.RegisterCallback<DragLeaveEvent>(OnDragLeave);
            root.RegisterCallback<DragUpdatedEvent>(OnDragUpdate);
            root.RegisterCallback<DragPerformEvent>(OnDragPerform);

            UpdateModelList();
        }

        static void OnDragEnter(DragEnterEvent _)
        {
            root.Q<VisualElement>("drop_model").EnableInClassList("hidden", false);
        }

        static void OnDragLeave(DragLeaveEvent _)
        {
            root.Q<VisualElement>("drop_model").EnableInClassList("hidden", true);
        }

        static void OnDragUpdate(DragUpdatedEvent _)
        {
            DragAndDrop.visualMode = DragAndDropVisualMode.Generic;
        }

        static void OnDragPerform(DragPerformEvent _)
        {
            root.Q<VisualElement>("drop_model").EnableInClassList("hidden", true);
            if (DragAndDrop.paths.Length > 0 && DragAndDrop.objectReferences.Length == 0)
            {
                foreach (string path in DragAndDrop.paths)
                {
                    var fileExtension = Path.GetExtension(path)?.ToLower();

                    switch (fileExtension)
                    {
                        case ".prefab":
                            AssetDatabase.ImportAsset(path, ImportAssetOptions.Default);
                            break;
                        case ".unitypackage":
                            AssetDatabase.ImportPackage(path, false);
                            break;
#if HAS_VRM
                        case ".vrm":
                            VRMImporter.ImportVRM(path);
                            break;
#endif
                    }
                }
            }
            else
            {
                Debug.Log("Out of reach");
                Debug.Log("Paths:");
                foreach (string path in DragAndDrop.paths)
                {
                    Debug.Log("- " + path);
                }

                Debug.Log("ObjectReferences:");
                foreach (Object obj in DragAndDrop.objectReferences)
                {
                    Debug.Log("- " + obj);
                }
            }
        }

        internal static async void UpdateModelList()
        {
            await Task.Yield();
            var searchQuery = root.Q<TextField>("searchQuery").value;
            UpdatePrefabsWithPreview(searchQuery);

            modelList.Clear();
            AddPrefabsToModelListAsync();
        }

        private static void UpdatePrefabsWithPreview(string searchValue = "")
        {
            prefabsWithPreview = PrefabManager.GetAllPrefabsIncludingHidden();

            if (!string.IsNullOrEmpty(searchValue))
            {
                prefabsWithPreview = prefabsWithPreview.Where(prefab => prefab.Name.Contains(searchValue)).ToList();
            }

            switch (sortOrder)
            {
                case SortOrder.LastModifiedDescending:
                    prefabsWithPreview = prefabsWithPreview.OrderByDescending(p => p.LastModified).ToList();
                    break;
                case SortOrder.LastModifiedAscending:
                    prefabsWithPreview = prefabsWithPreview.OrderBy(p => p.LastModified).ToList();
                    break;
                case SortOrder.NameDescending:
                    prefabsWithPreview = prefabsWithPreview.OrderByDescending(p => p.Name).ToList();
                    break;
                case SortOrder.NameAscending:
                    prefabsWithPreview = prefabsWithPreview.OrderBy(p => p.Name).ToList();
                    break;
            }

            switch (filterOrder)
            {
                case FilterOrder.NotShowHiddenModels:
                    prefabsWithPreview = prefabsWithPreview.Where(p => p.Status != EAUploaderMeta.PrefabStatus.Hidden).ToList();
                    break;
                case FilterOrder.ShowHiddenModels:
                    break;
                case FilterOrder.ShowOnlyHiddenModels:
                    prefabsWithPreview = prefabsWithPreview.Where(p => p.Status == EAUploaderMeta.PrefabStatus.Hidden).ToList();
                    break;
            }
        }

        private static async void AddPrefabsToModelListAsync()
        {
            foreach (var prefab in prefabsWithPreview)
            {
                var item = CreatePrefabItem(prefab);

                modelList.Add(item);
                await Task.Yield();
            }
        }

        private static VisualElement CreatePrefabItem(PrefabInfo prefab)
        {
            var item = new PrefabItem(prefab);
            return item;
        }

        internal static void HidePrefab(string prefabPath)
        {
            var allPrefabs = PrefabManager.LoadPrefabsInfo();
            var prefab = allPrefabs.FirstOrDefault(p => p.Path == prefabPath);
            if (prefab != null)
            {
                prefab.Status = EAUploaderMeta.PrefabStatus.Hidden;
                Debug.Log($"Hide prefab to {prefab.Status}");
                PrefabManager.SavePrefabsInfo(allPrefabs);
                ManageModels.UpdateModelList();
            }
        }

        internal static void ShowPrefab(string prefabPath)
        {
            var allPrefabs = PrefabManager.LoadPrefabsInfo();
            var prefab = allPrefabs.FirstOrDefault(p => p.Path == prefabPath);
            if (prefab != null)
            {
                prefab.Status = EAUploaderMeta.PrefabStatus.Show;
                PrefabManager.SavePrefabsInfo(allPrefabs);
                ManageModels.UpdateModelList();
            }
        }
    }

    internal class PrefabItem : VisualElement
    {
        public PrefabItem(PrefabInfo prefab)
        {
            var visualTree = Resources.Load<VisualTreeAsset>("UI/TabContents/ImportSettings/Contents/PrefabItem");
            visualTree.CloneTree(this);

            var previewImage = this.Q<Image>("previewImage");
            if (prefab.Preview != null)
            {
                previewImage.image = prefab.Preview;
            }

            previewImage.RegisterCallback<MouseUpEvent>(evt => ShowLargeImage(prefab));

            var name = this.Q<Label>("nameLabel");
            name.text = prefab.Name;

            var lastModified = this.Q<Label>("lastModifiedLabel");
            lastModified.text = prefab.LastModified.ToString("yyyy/MM/dd HH:mm:ss");

            var settingsButton = this.Q<Button>("settingsButton");
            settingsButton.clicked += () => OpenSettings(prefab.Path, prefab.Preview);

            var deleteButton = this.Q<Button>("deleteButton");
            deleteButton.clicked += () => DeletePrefab(prefab.Path);
        }

        private static void ShowLargeImage(PrefabInfo prefab)
        {
            PrefabPreviewer.ShowLargeImage(prefab.Path, prefab.Preview);
        }

        private static void OpenSettings(string prefabPath, Texture2D preview)
        {
            var settingsWindow = AvatarSettingsWindow.ShowWindow();
            settingsWindow.SetPrefabPath(prefabPath, preview);
        }

        private static void DeletePrefab(string prefabPath)
        {
            if (PrefabManager.ShowDeletePrefabDialog(prefabPath))
            {
                ManageModels.UpdateModelList();
            }
        }
    }
}
