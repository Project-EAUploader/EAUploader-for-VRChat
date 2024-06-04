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

            var libraryFoldButoton = root.Q<VisualElement>("library_fold_button");
            var icon = libraryFoldButoton.Q<MaterialIcon>();
            icon.icon = Main.isLibraryOpen ? "chevron_right" : "chevron_left";
            libraryFoldButoton.RegisterCallback<MouseUpEvent>(evt =>
            {
                Main.ToggleLibrary();
                icon.icon = Main.isLibraryOpen ? "chevron_right" : "chevron_left";
            });

            var filterbar = root.Q<VisualElement>("filterbar");
            filterbar.Add(filterDropdown);

            UpdateModelList();

            if (EAUploaderCore.HasVRM)
            {
                root.Q<VisualElement>("drop_model").Q<Label>("drop_model_label").text = T7e.Get("Drop files(.unitypackage, .prefab, .vrm(ver. 0.x)) to import");
            }

            root.RegisterCallback<DragEnterEvent>(OnDragEnter);
            root.RegisterCallback<DragLeaveEvent>(OnDragLeave);
            root.RegisterCallback<DragUpdatedEvent>(OnDragUpdate);
            root.RegisterCallback<DragPerformEvent>(OnDragPerform);
        }

        // This method runs if a user brings the pointer over the target while a drag is in progress.
        static void OnDragEnter(DragEnterEvent _)
        {
            root.Q<VisualElement>("drop_model").EnableInClassList("hidden", false);
        }

        // This method runs if a user makes the pointer leave the bounds of the target while a drag is in progress.
        static void OnDragLeave(DragLeaveEvent _)
        {
            root.Q<VisualElement>("drop_model").EnableInClassList("hidden", true);
        }

        // This method runs every frame while a drag is in progress.
        static void OnDragUpdate(DragUpdatedEvent _)
        {
            DragAndDrop.visualMode = DragAndDropVisualMode.Generic;
        }

        // This method runs when a user drops a dragged object onto the target.
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
            var searchQuery = root.Q<TextField>("searchQuery").value;
            modelList.Clear();
            
            List<PrefabInfo> prefabs = await UpdatePrefabsWithPreviewAsync(searchQuery, prefabInfo =>
            {
                var item = CreatePrefabItem(prefabInfo);
                modelList.Add(item);
            });
        }

        private static async Task<List<PrefabInfo>> UpdatePrefabsWithPreviewAsync(string searchValue = "", System.Action<PrefabInfo> onPrefabInfoAdded = null)
        {
            prefabsWithPreview = await PrefabManager.GetAllPrefabsIncludingHiddenAsync(onPrefabInfoAdded);
            List<PrefabInfo> prefabs = await Task.Run(() =>
            {
                return PrefabManager.GetAllPrefabsIncludingHiddenAsync();
            });

            if (!string.IsNullOrEmpty(searchValue))
            {
                prefabs = prefabs.Where(prefab => prefab.Name.Contains(searchValue)).ToList();
            }

            switch (sortOrder)
            {
                case SortOrder.LastModifiedDescending:
                    prefabs = prefabs.OrderByDescending(p => p.LastModified).ToList();
                    break;
                case SortOrder.LastModifiedAscending:
                    prefabs = prefabs.OrderBy(p => p.LastModified).ToList();
                    break;
                case SortOrder.NameDescending:
                    prefabs = prefabs.OrderByDescending(p => p.Name).ToList();
                    break;
                case SortOrder.NameAscending:
                    prefabs = prefabs.OrderBy(p => p.Name).ToList();
                    break;
            }

            switch (filterOrder)
            {
                case FilterOrder.NotShowHiddenModels:
                    prefabs = prefabs.Where(p => p.Status != EAUploaderMeta.PrefabStatus.Hidden).ToList();
                    break;
                case FilterOrder.ShowHiddenModels:
                    // フィルタリングは不要
                    break;
                case FilterOrder.ShowOnlyHiddenModels:
                    prefabs = prefabs.Where(p => p.Status == EAUploaderMeta.PrefabStatus.Hidden).ToList();
                    break;
            }

            return prefabs;
        }

        private static async Task AddPrefabsToModelListAsync()
        {
            await Task.Run(() =>
            {
                foreach (var prefab in prefabsWithPreview)
                {
                    var item = CreatePrefabItem(prefab);
                    modelList.Add(item);
                }
            });
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

            var miscellaneous = this.Q<VisualElement>("miscellaneous");

            var prefabObject = PrefabManager.GetPrefab(prefab.Path);
            var hasDescriptor = Utility.CheckAvatarHasVRCAvatarDescriptor(prefabObject);
            var hasShader = ShaderChecker.CheckAvatarHasShader(prefabObject);
            var isVRM = Utility.CheckAvatarIsVRM(prefabObject);

            if (!hasDescriptor)
            {
                if (isVRM)
                {
                    var warning = new VisualElement()
                    {
                        style =
                            {
                                flexDirection = FlexDirection.Row,
                                alignItems = Align.Center,
                                marginBottom = 4,
                            }
                    };
                    warning.AddToClassList("warning");
                    var warningIcon = new MaterialIcon { icon = "warning" };
                    var warningLabel = new Label(T7e.Get("VRM Avatar needs to convert to VRChat Avatar"));
                    warning.Add(warningIcon);
                    warning.Add(warningLabel);
                    miscellaneous.Add(warning);
                }
                else
                {
                    var warning = new VisualElement()
                    {
                        style =
                            {
                                flexDirection = FlexDirection.Row,
                                alignItems = Align.Center,
                                marginBottom = 4,
                            }
                    };
                    warning.AddToClassList("warning");
                    var warningIcon = new MaterialIcon { icon = "warning" };
                    var warningLabel = new Label(T7e.Get("Can't be uploaded"));
                    warning.Add(warningIcon);
                    warning.Add(warningLabel);
                    miscellaneous.Add(warning);
                }
            }

            if (!hasShader)
            {
                var warning = new VisualElement()
                {
                    style =
                        {
                            flexDirection = FlexDirection.Row,
                            alignItems = Align.Center,
                            marginBottom = 4,
                        }
                };
                warning.AddToClassList("warning");
                var warningIcon = new MaterialIcon { icon = "warning" };
                var warningLabel = new Label(T7e.Get("Cannot find the configured shader."));
                warning.Add(warningIcon);
                warning.Add(warningLabel);
                miscellaneous.Add(warning);
            }

            if (!hasDescriptor || !hasShader)
            {
                if (prefab.Status == EAUploaderMeta.PrefabStatus.Hidden)
                {
                    var unhideButton = new Button(() => ManageModels.ShowPrefab(prefab.Path))
                    {
                        text = T7e.Get("Show"),
                        style =
                        {
                            marginBottom = 4,
                            fontSize = 10,
                        }
                    };
                    miscellaneous.Add(unhideButton);
                }
                else
                {
                    var hideButton = new Button(() => ManageModels.HidePrefab(prefab.Path))
                    {
                        text = T7e.Get("Hide"),
                        style =
                        {
                            marginBottom = 4,
                            fontSize = 10,
                        }
                    };
                    miscellaneous.Add(hideButton);
                }
            }

            var controls = this.Q<VisualElement>("controls");
            var changeNameButton = this.Q<Button>("changeNameButton");
            changeNameButton.clicked += () => ChangePrefabName(prefab.Path);

            var copyAsNewNameButton = this.Q<Button>("copyAsNewNameButton");
            copyAsNewNameButton.clicked += () => CopyPrefabAsNewName(prefab.Path);

            var deleteButton = this.Q<Button>("deleteButton");
            deleteButton.clicked += () => DeletePrefab(prefab.Path);
        }

        private static void ShowLargeImage(PrefabInfo prefab)
        {
            PrefabPreviewer.ShowLargeImage(prefab.Path, prefab.Preview);
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