using EAUploader.CustomPrefabUtility;
using EAUploader.UI.Components;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
        internal static Components.Preview preview;
        private static bool isEditorInfoLoaded = false;
        private static List<EditorRegistration> editorRegistrations;
        private static SortOrder sortOrder = SortOrder.LastModifiedDescending;
        internal static Dictionary<string, bool> editorFoldouts = new Dictionary<string, bool>();

        public static void ShowContent(VisualElement rootElement)
        {
            root = rootElement;

            var visualTree = Resources.Load<VisualTreeAsset>("UI/TabContents/Setup/Setup");
            visualTree.CloneTree(root);

            modelList = root.Q<ScrollView>("model_list");

            GetModelList();

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

            UpdateModelList();

            preview = new Preview(root.Q("avatar_preview"), EAUploaderCore.selectedPrefabPath);

            preview.ShowContent();

            if (EAUploaderCore.selectedPrefabPath != null)
            {
                UpdatePrefabInto(EAUploaderCore.selectedPrefabPath);
                preview.UpdatePreview(EAUploaderCore.selectedPrefabPath);
            }

            BuildEditor();

            ButtonClickHandler();

            root.Q<ShadowButton>("find_extentions").clicked += () =>
            {
                Application.OpenURL("https://www.uslog.tech/eauploader-plug-ins");
            };
        }

        private static void GetModelList()
        {
            prefabsWithPreview = PrefabManager.GetAllPrefabsWithPreview();
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
            var item = new PrefabItemButton(prefab, () =>
            {
                EAUploaderCore.selectedPrefabPath = prefab.Path;
                UpdatePrefabInto(prefab.Path);
                preview.UpdatePreview(prefab.Path);
            });
            return item;
        }

        private static void UpdateModelList()
        {
            var searchQuery = root.Q<TextField>("searchQuery").value;
            GetModelList(searchQuery);
        }

        private static void GetModelList(string searchValue = "")
        {
            prefabsWithPreview = PrefabManager.GetAllPrefabsWithPreview();
            if (!string.IsNullOrEmpty(searchValue))
            {
                prefabsWithPreview = prefabsWithPreview.Where(prefab => prefab.Name.Contains(searchValue)).ToList();
            }

            var pinnedPrefabs = prefabsWithPreview.Where(p => p.Status == PrefabStatus.Pinned).ToList();
            var unpinnedPrefabs = prefabsWithPreview.Where(p => p.Status != PrefabStatus.Pinned).ToList();

            switch (sortOrder)
            {
                case SortOrder.LastModifiedDescending:
                    pinnedPrefabs = pinnedPrefabs.OrderByDescending(p => p.LastModified).ToList();
                    unpinnedPrefabs = unpinnedPrefabs.OrderByDescending(p => p.LastModified).ToList();
                    break;
                case SortOrder.LastModifiedAscending:
                    pinnedPrefabs = pinnedPrefabs.OrderBy(p => p.LastModified).ToList();
                    unpinnedPrefabs = unpinnedPrefabs.OrderBy(p => p.LastModified).ToList();
                    break;
                case SortOrder.NameDescending:
                    pinnedPrefabs = pinnedPrefabs.OrderByDescending(p => p.Name).ToList();
                    unpinnedPrefabs = unpinnedPrefabs.OrderByDescending(p => p.Name).ToList();
                    break;
                case SortOrder.NameAscending:
                    pinnedPrefabs = pinnedPrefabs.OrderBy(p => p.Name).ToList();
                    unpinnedPrefabs = unpinnedPrefabs.OrderBy(p => p.Name).ToList();
                    break;
            }

            prefabsWithPreview = pinnedPrefabs.Concat(unpinnedPrefabs).ToList();

            if (modelList != null)
            {
                modelList.Clear();
                AddPrefabsToModelList();
            }
        }

        internal static void UpdatePrefabInto(string prefabPath)
        {
            var prefabInfo = root.Q<VisualElement>("prefab_info");
            prefabInfo.Clear();

            var prefabName = new Label(T7e.Get("Now selecting: ") + Path.GetFileNameWithoutExtension(prefabPath))
            {
                style =
                {
                    fontSize = 20,
                    unityFontStyleAndWeight = FontStyle.Bold
                }
            };
            prefabInfo.Add(prefabName);

            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
            var prefabHeight = new Label(T7e.Get("Height: ") + Utility.GetAvatarHeight(prefab) + "m")
            {
                style =
                {
                    fontSize = 15,
                    unityFontStyleAndWeight = FontStyle.Normal
                }
            };
            prefabInfo.Add(prefabHeight);
        }

        private static void ButtonClickHandler()
        {
            var changeNameButton = root.Q<ShadowButton>("change_name");
            changeNameButton.clicked += ChangeNameButtonClicked;
            var pinButton = root.Q<ShadowButton>("pin_model");
            pinButton.clicked += PinButtonClicked;
            var deleteButton = root.Q<ShadowButton>("delete_model");
            deleteButton.clicked += DeleteButtonClicked;
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
            PrefabManager.PinPrefab(EAUploaderCore.selectedPrefabPath);
            GetModelList();
        }

        private static void DeleteButtonClicked()
        {
            if (PrefabManager.ShowDeletePrefabDialog(EAUploaderCore.selectedPrefabPath))
            {
                GetModelList();
            }
        }

        internal static void BuildEditor()
        {
            if (!isEditorInfoLoaded)
            {
                editorRegistrations = new List<EditorRegistration>(EAUploaderEditorManager.GetRegisteredEditors());
                foreach (var editor in editorRegistrations)
                {
                    editorFoldouts[editor.EditorName] = false;
                }
                isEditorInfoLoaded = true;
            }

            var editorList = root.Q<VisualElement>("avatar_editor_list");
            editorList.Clear();

            foreach (var editor in editorRegistrations)
            {
                var editorItem = CreateEditorItem(editor);
                editorList.Add(editorItem);
            }
        }

        private static VisualElement CreateEditorItem(EditorRegistration editor)
        {
            var item = new EditorItem(editor);

            return item;
        }

        public enum SortOrder
        {
            LastModifiedDescending,
            LastModifiedAscending,
            NameDescending,
            NameAscending
        }
    }

    internal class EditorItem : VisualElement
    {
        public EditorItem(EditorRegistration editor)
        {
            var buttonGroup = new VisualElement()
            {
                style =
                {
                    flexDirection = FlexDirection.Row,
                }
            };

            var item_button = new Button(() =>
            {
                EditorApplication.ExecuteMenuItem(editor.MenuName);
            })
            {
                text = editor.EditorName,
                style =
                {
                    flexGrow = 1,
                    borderBottomRightRadius = 0,
                    borderTopRightRadius = 0,
                    borderRightColor = new StyleColor(new Color(0.0784313725f , 0.3921568627f, 0.7058823529f,1)),
                    borderRightWidth = 1,
                }
            };

            buttonGroup.Add(item_button);

            var foldoutButton = new Button(() =>
            {
                Main.editorFoldouts[editor.EditorName] = !Main.editorFoldouts[editor.EditorName];
                Main.BuildEditor();
            })
            {
                style =
                {
                    width = 50,
                    justifyContent = Justify.Center,
                    paddingLeft = 2,
                    paddingRight = 2,
                    borderBottomLeftRadius = 0,
                    borderTopLeftRadius = 0,
                }
            };

            buttonGroup.Add(foldoutButton);

            Add(buttonGroup);

            var expandMore = new MaterialIcon { icon = "expand_more" };
            expandMore.style.fontSize = 20;
            var expandLess = new MaterialIcon { icon = "expand_less" };
            expandLess.style.fontSize = 20;

            if (Main.editorFoldouts[editor.EditorName])
            {
                foldoutButton.Add(expandLess);
                var editorContent = new VisualElement()
                {
                    style =
                    {
                        paddingBottom = 8,
                        paddingLeft = 8,
                        paddingRight = 8,
                        paddingTop = 8,
                    }
                };
                editorContent.Add(new Label(T7e.Get("Description: ") + editor.EditorName));
                editorContent.Add(new Label(T7e.Get("Version: ") + editor.MenuName));
                editorContent.Add(new Label(T7e.Get("Author: ") + editor.Author));

                if (!string.IsNullOrEmpty(editor.Url))
                {
                    var openButton = new Button(() =>
                    {
                        Application.OpenURL(editor.Url);
                    })
                    {
                        text = T7e.Get("Open the link")
                    };

                    openButton.AddToClassList("link");
                    editorContent.Add(openButton);
                }

                Add(editorContent);
            }
            else
            {
                foldoutButton.Add(expandMore);
            }
        }
    }
}