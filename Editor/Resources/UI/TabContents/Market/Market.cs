using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace EAUploader_beta
{
    public class Market
    {
        public static void ShowContent(VisualElement root)
        {
            var visualTree = Resources.Load<VisualTreeAsset>("UI/TabContents/Market/Market");
            visualTree.CloneTree(root);
        }
    }
}