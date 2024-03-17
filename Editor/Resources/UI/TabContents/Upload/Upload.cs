using EAUploader.CustomPrefabUtility;
using EAUploader.UI.Components;
using System.IO;
using UnityEngine;
using UnityEngine.UIElements;
using VRC.SDK3A.Editor;

namespace EAUploader.UI.Upload
{
    internal class Main
    {
        public static VisualElement root;
        public static Components.Preview preview;
        public static VRCSdkControlPanelAvatarBuilder _builder;

        public static void ShowContent(VisualElement rootContainer)
        {
            root = rootContainer;
            var visualTree = Resources.Load<VisualTreeAsset>("UI/TabContents/Upload/Upload");
            visualTree.CloneTree(root);

            UploadForm.ShowContent(root.Q("upload_form"));

            preview = new Components.Preview(root.Q("avatar_preview"), EAUploaderCore.selectedPrefabPath);
            preview.ShowContent();

            CreatePrefabList();
        }

        internal static void CreatePrefabList()
        {
            var prefabList = PrefabManager.GetAllPrefabsWithPreview();
            var prefabListContainer = root.Q("prefab_list_container");
            prefabListContainer.Clear();

            foreach (var prefab in prefabList)
            {
                var prefabButton = new PrefabItemButton(prefab, () =>
                {
                    EAUploaderCore.selectedPrefabPath = prefab.Path;
                    preview.UpdatePreview(prefab.Path);
                }, true);
                prefabListContainer.Add(prefabButton);
            }
        }
    }
}