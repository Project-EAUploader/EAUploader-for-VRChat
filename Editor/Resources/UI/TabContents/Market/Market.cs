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

            root.style.flexDirection = FlexDirection.ColumnReverse;

            var VRCWButton = root.Q<Button>("Show_VRCWMarket");
            var OtherMarketsButton = root.Q<Button>("Show_OtherMarkets");


            VRCWButton.EnableInClassList("tab-button__selected", true);
            VRCWMarket.ShowContent(root.Q("market_container"));

            

            VRCWButton.clicked += () =>
            {
                root.Q("market_container").Clear();
                VRCWMarket.ShowContent(root.Q("market_container"));
                VRCWButton.EnableInClassList("tab-button__selected", true);
                OtherMarketsButton.EnableInClassList("tab-button__selected", false);
            };

            
            OtherMarketsButton.clicked += () =>
            {
                root.Q("market_container").Clear();
                OtherMarkets.ShowContent(root.Q("market_container"));
                VRCWButton.EnableInClassList("tab-button__selected", false);
                OtherMarketsButton.EnableInClassList("tab-button__selected", true);
            };
        }
    }
}