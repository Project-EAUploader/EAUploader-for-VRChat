using Cysharp.Threading.Tasks;
using EAUploader.UI.Components;
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
            public StaredProduct[] staredProducts;
        }

        [Serializable]
        public struct StaredProduct
        {
            public int id;
            public string name;
            public int price;
            public string price_display_mode;
            public string creator_name;
            public string created_at;
            public string url;
            public string image_url;
            public string creator_twitter_url;
        }

        private static VisualElement root;
        private static string STARED_ITEMS_PATH = "Assets/EAUploader/StaredItems.json";
        private static List<List<Product>> productsPage = new List<List<Product>>();

        public static async void ShowContent(VisualElement rootContainer)
        {
            root = rootContainer;
            var visualTree = Resources.Load<VisualTreeAsset>("UI/TabContents/Market/Contents/VRCWMarket");
            visualTree.CloneTree(root);

            Initialize();

            root.Q<Button>("my_list").clicked += async () =>
            {
                // Open the stared items page
                var staredItemsPage = root.Q<VisualElement>("shop_items_container");

                var items = new VisualElement();
                items.AddToClassList("stared_items_container");

                var back_button = new Button();
                back_button.text = T7e.Get("Back");
                back_button.clicked += async () =>
                {
                    await ShowProducts();
                };

                items.Add(back_button);

                var staredItems = new List<StaredProduct>();

                if (File.Exists(STARED_ITEMS_PATH))
                {
                    string jsonTextFromFile = File.ReadAllText(STARED_ITEMS_PATH);
                    if (!string.IsNullOrEmpty(jsonTextFromFile))
                    {
                        StaredProductList staredProductList = JsonUtility.FromJson<StaredProductList>(jsonTextFromFile);
                        if (staredProductList.staredProducts != null)
                        {
                            staredItems.AddRange(staredProductList.staredProducts);
                        }
                    }
                }

                foreach (var product in staredItems)
                {
                    var item = new VisualElement();
                    item.AddToClassList("product_item");
                    var itemUxml = Resources.Load<VisualTreeAsset>("UI/TabContents/Market/Contents/VRCWMarketItem");

                    itemUxml.CloneTree(item);

                    item.Q<Label>("Name").text = product.name;
                    item.Q<Label>("Price").text = product.price.ToString();
                    item.Q<Label>("Creator").text = product.creator_name;
                    item.Q<Label>("CreatedAt").text = product.created_at;
                    item.Q<Button>("OpenProduct").clicked += () =>
                    {
                        Application.OpenURL(product.url);
                    };
                    item.Q<Button>("OpenSNS").clicked += () =>
                    {
                        Application.OpenURL(product.creator_twitter_url);
                    };
                    item.Q<Button>("StarButton").clicked += () =>
                    {
                        // Remove the item from the list
                        var newStaredItems = new List<StaredProduct>(staredItems);
                        newStaredItems.Remove(product);
                        StaredProductList newStaredProductList = new StaredProductList { staredProducts = newStaredItems.ToArray() };
                        string newJson = JsonUtility.ToJson(newStaredProductList);
                        File.WriteAllText(STARED_ITEMS_PATH, newJson);

                        item.Q<Button>("StarButton").SetEnabled(true);
                    };

                    var image = await DownloadProductImage(product.image_url);
                    var thumbnail = item.Q<VisualElement>("product_thumbnail");
                    if (image != null)
                    {
                        var imageElement = new Image();
                        imageElement.image = image;
                        thumbnail.Add(imageElement);
                    }

                    items.Add(item);

                    await UniTask.Delay(100);
                    item.AddToClassList("product_item__show");
                }

                staredItemsPage.Clear();

                staredItemsPage.Add(items);
            };

            await FetchProducts();
            await ShowProducts();
        }

        private static void Initialize()
        {
            // Create json file to store stared items
            if (!File.Exists(STARED_ITEMS_PATH))
            {
                File.WriteAllText(STARED_ITEMS_PATH, "{}");
            }
        }

        private static async Task ShowProducts()
        {
            // Draw the items
            var items = root.Q<VisualElement>("shop_items_container");
            items.Clear();

            int itemsPerRow = 4;

            List<Product> allProducts = new List<Product>();
            foreach (var page in productsPage)
            {
                allProducts.AddRange(page);
            }


            int rowCount = Mathf.CeilToInt((float)allProducts.Count / itemsPerRow); // Calculate the number of rows needed

            for (int row = 0; row < rowCount; row++)
            {
                var rowContainer = new VisualElement()
                {
                    name = "product_row"
                };

                items.Add(rowContainer);

                for (int col = 0; col < itemsPerRow; col++)
                {
                    int index = row * itemsPerRow + col;
                    if (index >= allProducts.Count)
                    {
                        break; // Break if there are no more products to display
                    }

                    var product = allProducts[index];
                    var item = new VisualElement();
                    item.AddToClassList("product_item");
                    var itemUxml = Resources.Load<VisualTreeAsset>("UI/TabContents/Market/Contents/VRCWMarketItem");

                    itemUxml.CloneTree(item);

                    item.Q<Label>("Name").text = product.name;
                    item.Q<Label>("Price").text = product.price.ToString();
                    item.Q<Label>("Creator").text = product.creator_name;
                    item.Q<Label>("CreatedAt").text = product.created_at;
                    item.Q<Button>("OpenProduct").clicked += () =>
                    {
                        OpenProductPage(product);
                    };
                    item.Q<Button>("OpenSNS").clicked += () =>
                    {
                        OpenSNSPage(product);
                    };

                    bool isMarked = IsProductMarked(product.id);

                    if (isMarked)
                    {
                        item.Q<Button>("StarButton").Q<Label>().text = T7e.Get("Stared");
                        item.Q<Button>("StarButton").SetEnabled(false);
                    }
                    else
                    {

                        item.Q<Button>("StarButton").clicked += () =>
                        {
                            ToggleStared(product);
                        };
                    }

                    var image = await DownloadProductImage(product.image_url);
                    var thumbnail = item.Q<VisualElement>("product_thumbnail");
                    if (image != null)
                    {
                        var imageElement = new Image();
                        imageElement.image = image;
                        thumbnail.Add(imageElement);
                    }

                    rowContainer.Add(item);

                    await UniTask.Delay(100);
                    item.AddToClassList("product_item__show");
                }
            }

            // Create load more button
            var loadMoreButton = new ShadowButton()
            {
                name = "LoadMore"
            };
            loadMoreButton.text = T7e.Get("Load More");
            loadMoreButton.clicked += async () =>
            {
                int nextPage = productsPage.Count + 1;
                await MoreProducts(nextPage);
            };

            items.Add(loadMoreButton);
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
                        if (staredProduct.id == productId)
                        {
                            return true;
                        }
                    }
                }
            }
            return false;
        }

        private static async Task MoreProducts(int page)
        {
            await FetchProducts(page: page);

            // Show the new products

            var items = root.Q<VisualElement>("shop_items_container");
            items.Remove(items.Q<ShadowButton>("LoadMore"));

            int itemsPerRow = 4;

            List<Product> products = productsPage[page - 1];

            int rowCount = Mathf.CeilToInt((float)products.Count / itemsPerRow); // Calculate the number of rows needed

            for (int row = 0; row < rowCount; row++)
            {
                var rowContainer = new VisualElement()
                {
                    name = "product_row"
                };

                items.Add(rowContainer);

                for (int col = 0; col < itemsPerRow; col++)
                {
                    int index = row * itemsPerRow + col;
                    if (index >= products.Count)
                    {
                        break; // Break if there are no more products to display
                    }

                    var product = products[index];
                    var item = new VisualElement();
                    item.AddToClassList("product_item");
                    var itemUxml = Resources.Load<VisualTreeAsset>("UI/TabContents/Market/Contents/VRCWMarketItem");

                    itemUxml.CloneTree(item);

                    item.Q<Label>("Name").text = product.name;
                    item.Q<Label>("Price").text = product.price.ToString();
                    item.Q<Label>("Creator").text = product.creator_name;
                    item.Q<Label>("CreatedAt").text = product.created_at;
                    item.Q<Button>("OpenProduct").clicked += () =>
                    {
                        OpenProductPage(product);
                    };
                    item.Q<Button>("OpenSNS").clicked += () =>
                    {
                        OpenSNSPage(product);
                    };
                    item.Q<Button>("StarButton").clicked += () =>
                    {
                        ToggleStared(product);
                    };

                    var image = await DownloadProductImage(product.image_url);
                    var thumbnail = item.Q<VisualElement>("product_thumbnail");
                    if (image != null)
                    {
                        var imageElement = new Image();
                        imageElement.image = image;
                        thumbnail.Add(imageElement);
                    }

                    rowContainer.Add(item);

                    await UniTask.Delay(100);
                    item.AddToClassList("product_item__show");
                }
            }

            // Create load more button
            var loadMoreButton = new ShadowButton()
            {
                name = "LoadMore"
            };

            loadMoreButton.text = T7e.Get("Load More");
            loadMoreButton.clicked += async () =>
            {
                int nextPage = productsPage.Count + 1;
                await MoreProducts(nextPage);
            };

            items.Add(loadMoreButton);
        }

        private static async UniTask<Texture2D> DownloadProductImage(string image_url)
        {
            using (HttpClient client = new HttpClient())
            {
                try
                {
                    HttpResponseMessage response = await client.GetAsync(image_url);
                    if (response.IsSuccessStatusCode)
                    {
                        byte[] imageBytes = await response.Content.ReadAsByteArrayAsync();
                        Texture2D texture = new Texture2D(2, 2); // Initialize with arbitrary size
                        texture.LoadImage(imageBytes);
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

        private static async Task FetchProducts(string searchQuery = "", int page = 1)
        {
            string url = $"https://www.vrcw.net/product/latest/json?page={page}&keyword={Uri.EscapeDataString(searchQuery)}"; // Use Uri.EscapeDataString for proper URL encoding

            using (HttpClient client = new HttpClient())
            {
                try
                {
                    HttpResponseMessage response = await client.GetAsync(url);
                    if (response.IsSuccessStatusCode)
                    {
                        string jsonText = await response.Content.ReadAsStringAsync();
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
                if (page >= productsPage.Count)
                {
                    productsPage.Add(new List<Product>());
                }

                productsPage[page - 1] = newProducts.products.ToList();
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
            List<StaredProduct> staredProducts = new List<StaredProduct>();
            if (File.Exists(STARED_ITEMS_PATH))
            {
                string jsonTextFromFile = File.ReadAllText(STARED_ITEMS_PATH);
                if (!string.IsNullOrEmpty(jsonTextFromFile))
                {
                    StaredProductList staredProductList = JsonUtility.FromJson<StaredProductList>(jsonTextFromFile);
                    if (staredProductList.staredProducts != null)
                    {
                        staredProducts.AddRange(staredProductList.staredProducts);
                    }
                }
            }

            StaredProduct staredProduct = new StaredProduct
            {
                id = product.id,
                name = product.name,
                price = product.price,
                price_display_mode = product.price_display_mode,
                creator_name = product.creator_name,
                created_at = product.created_at,
                url = product.url,
                image_url = product.image_url,
                creator_twitter_url = product.creator_twitter.url
            };

            staredProducts.Add(staredProduct);

            StaredProductList newStaredProductList = new StaredProductList { staredProducts = staredProducts.ToArray() };
            string newJson = JsonUtility.ToJson(newStaredProductList);
            File.WriteAllText(STARED_ITEMS_PATH, newJson);
        }
    }
}