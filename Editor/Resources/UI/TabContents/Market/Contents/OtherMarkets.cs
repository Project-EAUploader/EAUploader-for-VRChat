using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using EAUploader.UI.Components;

namespace EAUploader.UI.Market
{
    internal class OtherMarkets
    {
        public static void ShowContent(VisualElement root)
        {
            var visualTree = Resources.Load<VisualTreeAsset>("UI/TabContents/Market/Contents/OtherMarkets");
            visualTree.CloneTree(root);

            string language = LanguageUtility.GetCurrentLanguage();

            // Get the texts from the files
            var marketsFilesPath = $"Packages/tech.uslog.eauploader/Editor/Resources/UI/TabContents/Market/Contents/OtherMarkets/{language}";
            var marketsFiles = Directory.GetFiles(marketsFilesPath, "*.txt", SearchOption.AllDirectories);

            var otherMarketsContainer = root.Q<ScrollView>("other_markets_container");
            foreach (var file in marketsFiles)
            {
                var marketItem = new VisualElement()
                {
                    name = "market_item",
                };

                // Get thumbnail path eg. "Packages/tech.uslog.eauploader/Editor/Resources/UI/TabContents/Market/Contents/OtherMarkets/thumbnail/Booth.png"
                var thumbnailPath = $"Packages/tech.uslog.eauploader/Editor/Resources/UI/TabContents/Market/Contents/OtherMarkets/thumbnail/{Path.GetFileNameWithoutExtension(file)}.png";
                var thumbnail = AssetDatabase.LoadAssetAtPath<Texture2D>(thumbnailPath);

                // Create image button
                var marketButton = new ImageButton(thumbnail);
                marketButton.AddToClassList("market");
                marketButton.clicked += () =>
                {
                    Application.OpenURL($"https://booth.pm/ja/items?tags%5B%5D=VRChat");
                };

                marketItem.Add(marketButton);

                var marketElementContainer = new ScrollView()
                {
                    name = "market_element_container",
                };

                var marketElement = new ArticleRenderer(file);
                marketElement.AddToClassList("market");
                marketElementContainer.Add(marketElement);

                marketItem.Add(marketElementContainer);

                otherMarketsContainer.Add(marketItem);
            }
        }
    }

    internal class ImageButton : Button
    {
        public ImageButton(Texture2D texture)
        {
            var image = new Image { image = texture };
            Add(image);
        }
    }
}