using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using System.Collections.Generic;
using System.Linq;
using EAUploader.Components;
using EAUploader.CustomPrefabUtility;
using EAUploader.UI.Components;

namespace EAUploader.UI.Windows
{
    public class PrefabListManagerWindow : EditorWindow
    {
        private ListView prefabListView;
        private List<string> prefabLists;
        private ShadowButton applyButton;

        public static void ShowWindow()
        {
            var window = GetWindow<PrefabListManagerWindow>();
            window.titleContent = new GUIContent("Prefab List Manager");
            window.minSize = new Vector2(400, 300);
            window.Show();
        }

        private void OnEnable()
        {
            var visualTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Packages/tech.uslog.eauploader/Editor/Resources/UI/TabContents/ImportSettings/Contents/PrefabListManagerWindow.uxml");
            rootVisualElement.Add(visualTree.Instantiate());

            prefabListView = rootVisualElement.Q<ListView>("prefabListView");
            prefabListView.makeItem = () => new PrefabListItem();
            prefabListView.bindItem = (element, i) =>
            {
                var item = (PrefabListItem)element;
                item.SetupItem(prefabLists[i], i, MoveListItem, RemoveList);
            };

            rootVisualElement.Q<ShadowButton>("addListButton").clicked += AddList;

            applyButton = rootVisualElement.Q<ShadowButton>("applyButton");
            if (applyButton != null)
            {
                applyButton.clicked += SaveLists;
                applyButton.style.display = DisplayStyle.None;
            }

            rootVisualElement.Q<ShadowButton>("cancelButton").clicked += Close;

            LoadLists();
        }

        private void LoadLists()
        {
            prefabLists = PrefabManager.prefabLists?.Keys.ToList() ?? new List<string>();
            prefabListView.itemsSource = prefabLists;

            if (prefabListView != null)
            {
                prefabListView.Rebuild();
            }

            applyButton.style.display = DisplayStyle.None;
        }

        private void AddList()
        {
            prefabLists.Add($"New List {prefabLists.Count}");
            prefabListView.Rebuild();
            ShowApplyButton();
        }

        private void RemoveList(int index)
        {
            prefabLists.RemoveAt(index);
            prefabListView.Rebuild();
            ShowApplyButton();
        }

        private void MoveListItem(int index, int direction)
        {
            if (index + direction >= 0 && index + direction < prefabLists.Count)
            {
                var temp = prefabLists[index];
                prefabLists[index] = prefabLists[index + direction];
                prefabLists[index + direction] = temp;
                prefabListView.Rebuild();
                ShowApplyButton();
            }
        }

        private void SaveLists()
        {
            var newPrefabLists = new Dictionary<string, List<PrefabInfo>>();
            foreach (var list in prefabLists)
            {
                if (PrefabManager.prefabLists.ContainsKey(list))
                {
                    newPrefabLists[list] = PrefabManager.prefabLists[list];
                }
                else
                {
                    newPrefabLists[list] = new List<PrefabInfo>();
                }
            }
            PrefabManager.prefabLists = newPrefabLists;
            PrefabManager.SavePrefabLists();
            applyButton.style.display = DisplayStyle.None;
        }

        internal void ShowApplyButton()
        {
            applyButton.style.display = DisplayStyle.Flex;
        }
    }

    public class PrefabListItem : VisualElement
    {
        private TextFieldPro textField;
        private ShadowButton moveUpButton;
        private ShadowButton moveDownButton;
        private ShadowButton removeButton;

        public PrefabListItem()
        {
            var visualTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Packages/tech.uslog.eauploader/Editor/Resources/UI/TabContents/ImportSettings/Contents/PrefabListItem.uxml");
            visualTree.CloneTree(this);

            textField = this.Q<TextFieldPro>("listNameTextField");
            moveUpButton = this.Q<ShadowButton>("moveUpButton");
            moveDownButton = this.Q<ShadowButton>("moveDownButton");
            removeButton = this.Q<ShadowButton>("removeButton");
        }

        public void SetupItem(string listName, int index, System.Action<int, int> moveListItem, System.Action<int> removeList)
        {
            textField.value = listName;
            textField.RegisterValueChangedCallback(evt =>
            {
                if (PrefabManager.prefabLists.ContainsKey(evt.previousValue))
                {
                    PrefabManager.prefabLists[evt.newValue] = PrefabManager.prefabLists[evt.previousValue];
                    PrefabManager.prefabLists.Remove(evt.previousValue);
                }
                else
                {
                    PrefabManager.prefabLists[evt.newValue] = new List<PrefabInfo>();
                }
                var window = PrefabListManagerWindow.GetWindow<PrefabListManagerWindow>();
                window.ShowApplyButton();
            });

            moveUpButton.clicked += () => moveListItem(index, -1);
            moveDownButton.clicked += () => moveListItem(index, 1);
            removeButton.clicked += () => removeList(index);
        }
    }
}