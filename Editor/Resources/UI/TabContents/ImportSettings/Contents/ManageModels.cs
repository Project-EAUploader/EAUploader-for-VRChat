using EAUploader.Components;
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

            var filterbar = root.Q<VisualElement>("filterbar");
            filterbar.Add(filterDropdown);

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
                    // フィルタリングは不要
                    break;
                case FilterOrder.ShowOnlyHiddenModels:
                    prefabsWithPreview = prefabsWithPreview.Where(p => p.Status == EAUploaderMeta.PrefabStatus.Hidden).ToList();
                    break;
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

        internal static void HidePrefab(string prefabPath)
        {
            var allPrefabs = PrefabManager.LoadPrefabsInfo();
            var prefab = allPrefabs.FirstOrDefault(p => p.Path == prefabPath);
            if (prefab != null)
            {
                prefab.Status = EAUploaderMeta.PrefabStatus.Hidden;
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

            previewImage.RegisterCallback<MouseUpEvent>(evt => ShowLargeImage(prefab.Preview));

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