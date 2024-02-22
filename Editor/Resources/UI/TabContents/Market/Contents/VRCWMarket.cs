using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UIElements;
using VRCWMarketPlace;

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

        private static VisualElement root;
        private static string STARED_ITEMS_PATH = "Assets/EAUploader/StaredItems.json";
        private static Vector2 scrollPosition;
        private static List<Product> products = new List<Product>();
        private static List<Product> markedProducts = new List<Product>();
        private static UnityWebRequest currentRequest = null;
        private static string markFilePath = "Assets/EAUploader/mark.json";
        private static Dictionary<string, UnityWebRequest> downloadRequests = new Dictionary<string, UnityWebRequest>();
        private static string searchString = "";
        private static int currentPage = 1;
        private static bool isLoading = false;
        private static bool isSearching = false;
        private static Rect _dropDownButtonRect;
        private static bool isMyListPage = false;

        public static void ShowContent(VisualElement rootContainer)
        {
            root = rootContainer;
            var visualTree = Resources.Load<VisualTreeAsset>("UI/TabContents/Market/Contents/VRCWMarket");
            visualTree.CloneTree(root);

            Initialize();
            FetchProducts();
            Products();
        }

        private static void Initialize()
        {
            // Create json file to store stared items
            if (!File.Exists(STARED_ITEMS_PATH))
            {
                File.WriteAllText(STARED_ITEMS_PATH, "[]");
            }
        }

        private static async void Products()
        {
            // Draw the items
            var items = root.Q<VisualElement>("shop_items_container");
            items.Clear();

            int itemsPerRow = 4; // Number of items to display per row
            int rowCount = Mathf.CeilToInt((float)products.Count / itemsPerRow); // Calculate the number of rows needed

            for (int row = 0; row < rowCount; row++)
            {
                var rowContainer = new VisualElement();
                rowContainer.style.flexDirection = FlexDirection.Row;
                rowContainer.style.justifyContent = Justify.SpaceBetween;
                rowContainer.style.marginBottom = 10; // Adjust the margin between rows if needed

                for (int col = 0; col < itemsPerRow; col++)
                {
                    int index = row * itemsPerRow + col;
                    if (index >= products.Count)
                    {
                        break; // Break if there are no more products to display
                    }

                    var product = products[index];
                    var productItem = Resources.Load<VisualTreeAsset>("UI/TabContents/Market/Contents/VRCWMarketItem");
                    var item = productItem.CloneTree();
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

                    var image = await DownloadProductImage(product);
                    var thumbnail = item.Q<VisualElement>("Thumbnail");
                    if (image != null)
                    {
                        var imageElement = new Image();
                        imageElement.image = image;
                        thumbnail.Add(imageElement);
                    }

                    rowContainer.Add(item);
                }

                items.Add(rowContainer);
            }
        }

        private static async Task<Texture2D> DownloadProductImage(Product product)
        {
            using (HttpClient client = new HttpClient())
            {
                try
                {
                    HttpResponseMessage response = await client.GetAsync(product.image_url);
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

        private static async void FetchProducts(string searchQuery = "", int page = 1)
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
                        ProcessJsonData(jsonText);
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

        private static void ProcessJsonData(string jsonText)
        {
            // Parse the new products
            ProductList newProducts = JsonUtility.FromJson<ProductList>(jsonText);
            if (newProducts.products != null)
            {
                foreach (var newProduct in newProducts.products)
                {
                    // Add the new product to the existing list
                    products.Add(new Product
                    {
                        id = newProduct.id,
                        name = newProduct.name,
                        price = newProduct.price,
                        price_display_mode = newProduct.price_display_mode,
                        format = newProduct.format,
                        polygon = newProduct.polygon,
                        url = newProduct.url,
                        creator_name = newProduct.creator_name,
                        creator_twitter = newProduct.creator_twitter,
                        creator_vrcname = newProduct.creator_vrcname,
                        creator_hide_vrcname = newProduct.creator_hide_vrcname,
                        rule_outofvrc = newProduct.rule_outofvrc,
                        rule_commerce = newProduct.rule_commerce,
                        rule_modify = newProduct.rule_modify,
                        rule_reuseparts = newProduct.rule_reuseparts,
                        description = newProduct.description,
                        status = newProduct.status,
                        deleted = newProduct.deleted,
                        updated_at = newProduct.updated_at,
                        created_at = newProduct.created_at,
                        image_url = newProduct.image_url,
                        product_type = newProduct.product_type,
                        member_displayname_now = newProduct.member_displayname_now,
                        member_displayname_first = newProduct.member_displayname_first,
                        category = newProduct.category,
                        keyword = newProduct.keyword,
                    });
                }
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
            // Load the stared items
            string jsonText = System.IO.File.ReadAllText(STARED_ITEMS_PATH);
            List<Product> staredItems = JsonUtility.FromJson<List<Product>>(jsonText);

            // Check if the product is already stared
            bool isStared = false;
            foreach (var item in staredItems)
            {
                if (item.id == product.id)
                {
                    isStared = true;
                    break;
                }
            }

            // Toggle the stared status
            if (isStared)
            {
                // Remove the product from the list
                staredItems.RemoveAll(item => item.id == product.id);
            }
            else
            {
                // Add the product to the list
                staredItems.Add(product);
            }

            // Save the list
            string newJsonText = JsonUtility.ToJson(staredItems);
            System.IO.File.WriteAllText(STARED_ITEMS_PATH, newJsonText);
        }
    }
}