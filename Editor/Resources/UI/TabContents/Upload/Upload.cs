using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace EAUploader.UI.Upload
{
    internal class Main
    {
        public static void ShowContent(VisualElement root)
        {
            var visualTree = Resources.Load<VisualTreeAsset>("UI/TabContents/Upload/Upload");
            visualTree.CloneTree(root);

            var preview = new Components.Preview(root.Q("avatar_preview"), EAUploaderCore.selectedPrefabPath);
            preview.ShowContent();

            // This is VRC check 2024/02/13
            var vrcSdkControlPanel = new VRCSdkControlPanel();
            vrcSdkControlPanel.Show();
        }
    }
}
