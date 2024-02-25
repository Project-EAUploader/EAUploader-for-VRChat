using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace EAUploader.UI.Market
{
    internal class Main
    {
        public static void ShowContent(VisualElement root)
        {
            var visualTree = Resources.Load<VisualTreeAsset>("UI/TabContents/Market/Market");
            visualTree.CloneTree(root);

            VRCWMarket.ShowContent(root.Q("market_container"));

            var VRCWButton = root.Q<Button>("Show_VRCWMarket");
            VRCWButton.clicked += () =>
            {
                root.Q("market_container").Clear();
                VRCWMarket.ShowContent(root.Q("market_container"));
            };

            var OtherMarketsButton = root.Q<Button>("Show_OtherMarkets");
            OtherMarketsButton.clicked += () =>
            {
                root.Q("market_container").Clear();
                OtherMarkets.ShowContent(root.Q("market_container"));
            };
        }
    }
}