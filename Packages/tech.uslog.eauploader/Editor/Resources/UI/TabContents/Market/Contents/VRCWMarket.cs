using Cysharp.Threading.Tasks;
using EAUploader.UI.Components;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UIElements;

namespace EAUploader.UI.Market
{
    internal class VRCWMarket
    {
        [Serializable]
        private struct ProductList
        {
            public Product[] products;
        }

        [Serializable]
        private struct Product
        {
            public int id;
            public string name;
            public int price;
            public string price_display_mode;
            public Format[] format;
            public int polygon;
            public string url;
            public string creator_name;
            public CreatorTwitter creator_twitter;
            public string creator_vrcname;
            public int creator_hide_vrcname;
            public string rule_outofvrc;
            public string rule_commerce;
            public string rule_modify;
            public string rule_reuseparts;
            public string description;
            public string status;
            public string deleted;
            public string updated_at;
            public string created_at;
            public string image_url;
            public ProductType product_type;
            public string member_displayname_now;
            public string member_displayname_first;
            public Category[] category;
            public string keyword;
        }

        [Serializable]
        public struct Format
        {
            public string format;
            public string name;
        }

        [Serializable]
        public struct CreatorTwitter
        {
            public string account;
            public string url;
        }

        [Serializable]
        public struct ProductType
        {
            public int id;
            public string name;
            public string filename;
            public string url;
        }

        [Serializable]
        public struct Category
        {
            public int id;
            public string name;
            public string filename;
            public string url;
        }

        public struct StaredProductList
        {
            public int[] staredProducts;
        }

        private static VisualElement root;
        private static string STARED_ITEMS_PATH = "Assets/EAUploader/StaredItems.json";
        private static int cachedPage = 0;
        private static List<Product> allProducts = new List<Product>();
        private static bool isMyPage = false;
        private static string searchQuery = "";

        public static void ShowContent(VisualElement rootContainer)
        {
            root = rootContainer;
            var visualTree = Resources.Load<VisualTreeAsset>("UI/TabContents/Market/Contents/VRCWMarket");
            root.styleSheets.Add(Resources.Load<StyleSheet>("UI/TabContents/Market/Contents/VRCWMarketStyle"));
            visualTree.CloneTree(root);

            Initialize();
        }

        private static async void Initialize()
        {
            if (!File.Exists(STARED_ITEMS_PATH))
            {
                File.WriteAllText(STARED_ITEMS_PATH, "{}");
            }

            var productDetailContainer = root.Q<VisualElement>("product_detail_container");
            productDetailContainer.Clear();

            var label = new Label(T7e.Get("商品を選択して詳細を見る"));
            productDetailContainer.Add(label);

            await FetchProducts();

            var storeContainer = root.Q<VisualElement>("shop_container");
            var productList = storeContainer.Q<ListView>("shop_item_list");

            productList.makeItem = MakeItem;
            productList.bindItem = BindItem;
            productList.fixedItemHeight = 128;
            productList.itemsSource = GetProductsIndex();
            productList.selectionType = SelectionType.Single;
            productList.selectionChanged += OnSelectionChanged;

            var searchButton = root.Q<Button>("search_button");
            var refreshButton = root.Q<Button>("refresh_button");
            var myListButton = root.Q<Button>("my_list");

            searchButton.clicked -= SearchButtonClicked;
            refreshButton.clicked -= RefreshButtonClicked;
            myListButton.clicked -= MyListButtonClicked;

            searchButton.clicked += SearchButtonClicked;
            refreshButton.clicked += RefreshButtonClicked;
            myListButton.clicked += MyListButtonClicked;

            var scrollView = productList.Q<ScrollView>();
            scrollView.verticalScroller.valueChanged += async (value) =>
            {
                if (scrollView.verticalScroller.highValue == value)
                {
                    await FetchProducts(cachedPage + 1);
                    productList.itemsSource = GetProductsIndex();
                    productList.Rebuild();
                }
            };
        }

        private static async void SearchButtonClicked()
        {
            searchQuery = root.Q<TextFieldPro>("search_input").GetValue();
            allProducts.Clear();
            cachedPage = 0;
            await FetchProducts();
            UpdateProductList();
        }

        private static async void RefreshButtonClicked()
        {
            allProducts.Clear();
            cachedPage = 0;
            await FetchProducts();
            UpdateProductList();
        }

        private static void MyListButtonClicked()
        {
            isMyPage = !isMyPage;
            root.Q<Button>("my_list").EnableInClassList("active", isMyPage);
            UpdateProductList();
        }

        private static void UpdateProductList()
        {
            var productList = root.Q<ListView>("shop_item_list");
            productList.itemsSource = GetProductsIndex();
            productList.Rebuild();
            var scrollView = productList.Q<ScrollView>();
            scrollView.verticalScroller.value = 0;

            var productDetailContainer = root.Q<VisualElement>("product_detail_container");
            productDetailContainer.Clear();

            var label = new Label(T7e.Get("商品を選択して詳細を見る"));
            productDetailContainer.Add(label);
        }


        private static VisualElement MakeItem()
        {
            var item = Resources.Load<VisualTreeAsset>("UI/TabContents/Market/Contents/VRCWMarketItem");
            return item.CloneTree();
        }

        private static async void BindItem(VisualElement element, int index)
        {
            var product = GetProductsIndex()[index];
            element.Q<Label>("Name").text = product.name;

            string priceText = product.price_display_mode switch
            {
                "min" => $"{product.price}円 ~",
                "max" => $"~ {product.price}円",
                _ => $"{product.price}円"
            };

            element.Q<Label>("Price").text = priceText;
            element.Q<Label>("Creator").text = product.creator_name;

            var image = element.Q<Image>("Image");
            image.scaleMode = ScaleMode.ScaleAndCrop;

            if (product.image_url != null)
            {
                var texture = await GetProductImage(product.image_url);
                image.image = texture;
            }

            element.Q<MaterialIcon>("star").EnableInClassList("hidden", !IsProductMarked(product.id));
        }

        private static List<Product> GetProductsIndex()
        {
            var products = new List<Product>();
            
            if (isMyPage)
            {
                if (File.Exists(STARED_ITEMS_PATH))
                {
                    string jsonTextFromFile = File.ReadAllText(STARED_ITEMS_PATH);
                    StaredProductList staredProductList = JsonUtility.FromJson<StaredProductList>(jsonTextFromFile);

                    if (staredProductList.staredProducts != null)
                    {
                        foreach (var staredProductId in staredProductList.staredProducts)
                        {
                            var product = allProducts.Find(p => p.id == staredProductId);
                            products.Add(product);
                        }
                    }
                }
            }
            else
            {
                products = allProducts;
            }

            return products;
        }

        private static async void OnSelectionChanged(IEnumerable<object> selected)
        {
            // Clear the product detail container
            var productDetailContainer = root.Q<VisualElement>("product_detail_container");
            productDetailContainer.Clear();

            var productDetail = Resources.Load<VisualTreeAsset>("UI/TabContents/Market/Contents/VRCWMarketDetail");
            productDetail.CloneTree(productDetailContainer);

            var item = selected.First();

            var product = (Product)item;

            productDetailContainer.Q<Label>("name").text = product.name;

            if (!string.IsNullOrEmpty(product.image_url))
            {
                var texture = await GetProductImage(product.image_url);
                var imageElement = productDetailContainer.Q<Image>("image");
                imageElement.scaleMode = ScaleMode.ScaleToFit;
                imageElement.image = texture;
            }

            var detailInfo = productDetailContainer.Q<ScrollView>("product-detail__info");
            detailInfo.Clear();

            string priceText = product.price_display_mode switch
            {
                "min" => $"{product.price}円 ~",
                "max" => $"~ {product.price}円",
                _ => $"{product.price}円"
            };

            List<string> ruleTexts = new List<string>();

            if (product.rule_outofvrc == "1")
            {
                ruleTexts.Add("VRChat以外で利用可能: 〇");
            }

            if (product.rule_commerce == "1")
            {
                ruleTexts.Add("商用利用可能: 〇");
            }

            if (product.rule_modify == "1")
            {
                ruleTexts.Add("任意に改変可能: 〇");
            }

            if (product.rule_reuseparts == "1")
            {
                ruleTexts.Add("パーツの流用可能: 〇");
            }

            detailInfo.Add(new ProductInfoItem("区分", new Link(product.product_type.name)
            {
                href = product.product_type.url
            }));

            detailInfo.Add(new ProductInfoItem("価格", new Label(priceText)));

            var formatContainer = new VisualElement()
            {
                style = { flexDirection = FlexDirection.Row, flexWrap = Wrap.Wrap }
            };
            foreach (var format in product.format)
            {
                formatContainer.Add(new Link(format.name)
                {
                    href = $"https://www.vrcw.net/search?keyword={Uri.EscapeDataString(format.name)}&mode=product",
                    style = { marginRight = 8 }
                });
            }

            detailInfo.Add(new ProductInfoItem("配布形式", formatContainer));

            if (product.polygon != -1)
            {
                detailInfo.Add(new ProductInfoItem("ポリゴン数", new Label($"△{product.polygon}")));
            }

            detailInfo.Add(new ProductInfoItem(
                "販売・配布ページ",
                new Link(product.url)
                {
                    href = product.url
                }));
            detailInfo.Add(new ProductInfoItem("制作者名", new Label(product.creator_name)));
            detailInfo.Add(new ProductInfoItem(
                "制作者Twitter",
                new Link($"@{product.creator_twitter.account}")
                {
                    href = product.creator_twitter.url
                }));
            if (product.creator_hide_vrcname == 0)
            {
                detailInfo.Add(new ProductInfoItem("制作者VRC名", new Label(product.creator_vrcname)));
            }
            if (ruleTexts.Count > 0)
            {
                detailInfo.Add(new ProductInfoItem("規約", new Label(string.Join("\n", ruleTexts))));
            }

            var keywordsContainer = new VisualElement()
            {
                style = { flexDirection = FlexDirection.Row, flexWrap = Wrap.Wrap }
            };

            foreach (var keyword in product.keyword.Split(' '))
            {
                keywordsContainer.Add(new Link(keyword)
                {
                    href = $"https://www.vrcw.net/search?keyword={Uri.EscapeDataString(keyword)}&mode=product",
                    style = { marginRight = 8 }
                });
            }

            detailInfo.Add(new ProductInfoItem("キーワード", keywordsContainer));

            var categoryContainer = new VisualElement()
            {
                style = { flexDirection = FlexDirection.Row, flexWrap = Wrap.Wrap }
            };

            foreach (var category in product.category)
            {
                categoryContainer.Add(new Link(category.name)
                {
                    href = category.url,
                    style = { marginRight = 8 }
                });
            }

            detailInfo.Add(new ProductInfoItem("カテゴリ", categoryContainer));

            var likeButton = productDetailContainer.Q<ShadowButton>("like_button");
            likeButton.text = IsProductMarked(product.id) ? "マイリストから消去する" : "マイリストに追加する";
            likeButton.clicked += () =>
                {
                    ToggleStared(product);
                    likeButton.text = IsProductMarked(product.id) ? "マイリストから消去する" : "マイリストに追加する";
                    var productList = root.Q<ListView>("shop_item_list");
                    productList.itemsSource = GetProductsIndex();
                    productList.Rebuild();
                };
        }

        private static bool IsProductMarked(int productId)
        {
            if (File.Exists(STARED_ITEMS_PATH))
            {
                string jsonTextFromFile = File.ReadAllText(STARED_ITEMS_PATH);
                StaredProductList staredProductList = JsonUtility.FromJson<StaredProductList>(jsonTextFromFile);

                if (staredProductList.staredProducts != null)
                {
                    foreach (var staredProduct in staredProductList.staredProducts)
                    {
                        if (staredProduct == productId)
                        {
                            return true;
                        }
                    }
                }
            }
            return false;
        }

        private static async Task<Texture2D> GetProductImage(string image_url)
        {
            string cachePath = Path.Combine("Assets/EAUploader/Cache");

            if (!Directory.Exists(cachePath))
            {
                Directory.CreateDirectory(cachePath);
            }

            string fileName = Path.GetFileName(image_url);
            string filePath = Path.Combine(cachePath, fileName);

            if (File.Exists(filePath))
            {
                byte[] fileData = File.ReadAllBytes(filePath);
                Texture2D texture = new Texture2D(2, 2);
                texture.LoadImage(fileData);
                return texture;
            }

            using (HttpClient client = new HttpClient())
            {
                try
                {
                    HttpResponseMessage response = await client.GetAsync(image_url);
                    if (response.IsSuccessStatusCode)
                    {
                        byte[] imageBytes = await response.Content.ReadAsByteArrayAsync();
                        Texture2D texture = new Texture2D(2, 2);
                        texture.LoadImage(imageBytes);

                        byte[] resizedImageBytes = texture.EncodeToPNG();
                        File.WriteAllBytes(filePath, resizedImageBytes);

                        return texture;
                    }
                    else
                    {
                        Debug.LogError("Image Download Error: " + response.StatusCode);
                    }
                }
                catch (HttpRequestException ex)
                {
                    Debug.LogError("Image Download Exception: " + ex.Message);
                }
            }
            return null;
        }

        private static async Task FetchProducts(int page = 1)
        {
            root.Q<VisualElement>("fetching_data_container").EnableInClassList("fetching_data__hidden", false);

            if (page <= cachedPage)
            {
                return;
            }

            cachedPage = page;

            string url = $"https://www.vrcw.net/product/latest/json?page={page}{(searchQuery != "" ? $"&keyword={searchQuery}" : "")}";

            using (HttpClient client = new HttpClient())
            {
                try
                {
                    HttpResponseMessage response = await client.GetAsync(url);
                    if (response.IsSuccessStatusCode)
                    {
                        string jsonText = await response.Content.ReadAsStringAsync();
                        root.Q<VisualElement>("fetching_data_container").EnableInClassList("fetching_data__hidden", true);
                        ProcessJsonData(jsonText, page);
                    }
                    else
                    {
                        Debug.LogError("Network Error: " + response.StatusCode);
                    }
                }
                catch (HttpRequestException ex)
                {
                    Debug.LogError("Network Exception: " + ex.Message);
                }
            }
        }

        private static void ProcessJsonData(string jsonText, int page)
        {
            ProductList newProducts = JsonUtility.FromJson<ProductList>(jsonText);
            if (newProducts.products != null)
            {
                allProducts.AddRange(newProducts.products);
            }
            else
            {
                Debug.LogError("Failed to parse new products from JSON.");
            }
        }

        private static void OpenProductPage(Product product)
        {
            Application.OpenURL(product.url);
        }

        private static void OpenSNSPage(Product product)
        {
            Application.OpenURL(product.creator_twitter.url);
        }

        private static void ToggleStared(Product product)
        {
            if (!File.Exists(STARED_ITEMS_PATH)) return;

            string jsonTextFromFile = File.ReadAllText(STARED_ITEMS_PATH);
            if (string.IsNullOrEmpty(jsonTextFromFile)) return;

            StaredProductList staredProductList = JsonUtility.FromJson<StaredProductList>(jsonTextFromFile);
            if (staredProductList.staredProducts == null)
            {
                staredProductList.staredProducts = new int[] { product.id };
            }
            else
            {
                var staredProductsSet = new HashSet<int>(staredProductList.staredProducts);
                if (!staredProductsSet.Add(product.id))
                {
                    staredProductsSet.Remove(product.id);
                }
                staredProductList.staredProducts = staredProductsSet.ToArray();
            }

            string jsonText = JsonUtility.ToJson(staredProductList);
            File.WriteAllText(STARED_ITEMS_PATH, jsonText);
        }
    }

    public class ProductInfoItem : VisualElement
    {
        public ProductInfoItem(string label, VisualElement value)
        {
            var labelElement = new Label(label)
            {
                name = "product-detail__info__label"
            };
            var valueElement = new VisualElement();
            valueElement.Add(value);
            Add(labelElement);
            Add(valueElement);
        }
    }
}