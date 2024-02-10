using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace EAUploader_beta.UI.Upload
{
    internal class Main
    {
        public static void ShowContent(VisualElement root)
        {
            var visualTree = Resources.Load<VisualTreeAsset>("UI/TabContents/Upload/Upload");
            visualTree.CloneTree(root);

            var preview = new Components.Preview();
            preview.ShowContent(root.Q("avatar_preview"));

        }
    }
}
