using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace EAUploader_beta.UI.Market
{
    internal class Main
    {
        public static void ShowContent(VisualElement root)
        {
            var visualTree = Resources.Load<VisualTreeAsset>("UI/TabContents/Market/Market");
            visualTree.CloneTree(root);


        }
    }
}