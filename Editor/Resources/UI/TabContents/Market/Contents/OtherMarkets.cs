using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace EAUploader.UI.Market
{
    internal class OtherMarkets
    {
        public static void ShowContent(VisualElement root)
        {
            var visualTree = Resources.Load<VisualTreeAsset>("UI/TabContents/Market/Contents/OtherMarkets");
            visualTree.CloneTree(root);
        }
    }
}