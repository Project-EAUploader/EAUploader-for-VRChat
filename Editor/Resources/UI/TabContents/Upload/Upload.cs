using EAUploader.CustomPrefabUtility;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using VRC.SDKBase.Editor;

namespace EAUploader.UI.Upload
{
    internal class Main
    {
        public static VisualElement root;
        public static Components.Preview preview;

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

        private static void CreatePrefabList()
        {
            var prefabList = PrefabManager.GetAllPrefabsWithPreview();
            var prefabListContainer = root.Q("prefab_list_container");

            foreach (var prefab in prefabList)
            {
                var prefabButton = new Button(() =>
                {
                    EAUploaderCore.selectedPrefabPath = prefab.Path;
                    preview.UpdatePreview(prefab.Path);
                })
                {
                    text = Path.GetFileNameWithoutExtension(prefab.Path)
                };
                prefabListContainer.Add(prefabButton);
            }
        }
    }
}