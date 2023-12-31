using UnityEngine;
using UnityEditor;
using UnityEngine.Networking;
using System.Collections.Generic;
using System.Collections;
using System.Threading.Tasks;
using System.IO;
using System.Linq; 
using System;
using static styles;
using static labels;
using static Texture;
using static MarketAnimations;

namespace VRCWMarketPlace
{
    public static class VRCWMarket
    {
        private static Vector2 scrollPosition;
        private static List<Product> products = new List<Product>();
        private static List<Product> markedProducts = new List<Product>();
        private static UnityWebRequest currentRequest = null;
        private static string jsonFilePath = "Packages/com.sabuworks.eauploader/Editor/Scripts/MainWindow/TabContents/VRCWMarket/index.json";
        private const string ThumbnailDirectory = "Packages/com.sabuworks.eauploader/Editor/Scripts/MainWindow/TabContents/VRCWMarket/Thumbnails/";
        private const string MyListThumbnailDirectory = "Packages/com.sabuworks.eauploader/Editor/Scripts/MainWindow/TabContents/VRCWMarket/MyList/";
        private static Dictionary<string, UnityWebRequest> downloadRequests = new Dictionary<string, UnityWebRequest>();
        private static Dictionary<string, Texture2D> imageCache = new Dictionary<string, Texture2D>();
        private static string searchString = "";
        private static int currentPage = 1;
        private static bool isLoading = false;
        private static bool isSearching = false;
        private static Rect _dropDownButtonRect;
        private static bool isMyListPage = false;

        public static void Draw(Rect position)
        {

            EditorGUI.DrawRect(new Rect(0, 0, position.width, position.height), new Color(0, 0, 0, 0));


            float searchBarHeight = position.height * 0.05f;
            float productListHeight = position.height * 0.85f;
            float operationAreaHeight = position.height * 0.05f;


            GUILayout.BeginArea(new Rect(0, 0, position.width, searchBarHeight), GUI.skin.box);
            if (!isMyListPage)
            {
                DrawSearchBar(position.width, searchBarHeight);
            }
            else
            {
                DrawMyListSearchBar(position.width, searchBarHeight);
            }
            GUILayout.EndArea();

            GUILayout.BeginArea(new Rect(0, searchBarHeight, position.width, productListHeight), GUI.skin.box);
            if (!isMyListPage)
            {
                DrawProductList(position.width, productListHeight);
            }
            else
            {
                GUILayout.Label(Get(815), h1LabelStyle);
                DrawMyList(position.width, productListHeight);
            }

            GUILayout.EndArea();

            GUILayout.BeginArea(new Rect(0, searchBarHeight + productListHeight, position.width, operationAreaHeight), GUI.skin.box);
            DrawOperationArea(position.width);
            GUILayout.EndArea();
        }

        private static void DrawSearchBar(float width, float height)
        {
            var SearchBarBoxStyle = new GUIStyle(GUI.skin.box);
            string colorCode = "#1C304A";
            Color color;
            if (ColorUtility.TryParseHtmlString(colorCode, out color))
            {

            }
            GUILayout.BeginHorizontal(SearchBarBoxStyle, GUILayout.Width(width), GUILayout.Height(height));


            searchString = EditorGUILayout.TextField(searchString, TextFieldStyle, GUILayout.Width(width * 0.4f), GUILayout.Height(height * 0.8f));


            if (GUILayout.Button(Getc("search", 144), ClearButtonStyle, GUILayout.Width(width * 0.1f), GUILayout.Height(height * 0.8f)))
            {
                isSearching = true;

            }
            if (searchString != "")
            {

                if (GUILayout.Button(Getc("close", 179), ClearButtonStyle, GUILayout.Width(width * 0.05f), GUILayout.Height(height * 0.8f)))
                {


                    isSearching = false;
                }
            }

            if (GUILayout.Button(Getc("refresh", 179), ClearButtonStyle, GUILayout.Width(width * 0.05f), GUILayout.Height(height * 0.8f)))
            {
                products.Clear();
                FetchProducts(searchString);
            }


            if (GUILayout.Button(Get(813), ClearButtonStyle, GUILayout.Width(width * 0.1f), GUILayout.Height(height * 0.8f)))
            {
                SortOptionsWindow.ShowWindow(isMyListPage);
            }
            if (Event.current.type == EventType.Repaint) _dropDownButtonRect = GUILayoutUtility.GetLastRect();

            if (GUILayout.Button(Get(815), ClearButtonStyle, GUILayout.Width(width * 0.2f), GUILayout.Height(height * 0.8f)))
            {
                AssetDatabase.Refresh();
                isMyListPage = true;
            }
            GUILayout.EndHorizontal();
        }

        private static void DrawMyListSearchBar(float width, float height)
        {
            var SearchBarBoxStyle = new GUIStyle(GUI.skin.box);
            string colorCode = "#1C304A";
            Color color;
            if (ColorUtility.TryParseHtmlString(colorCode, out color))
            {

            }
            GUILayout.BeginHorizontal(SearchBarBoxStyle, GUILayout.Width(width), GUILayout.Height(height));

            
            
            if (GUILayout.Button(Get(816), ClearButtonStyle, GUILayout.Width(width), GUILayout.Height(height * 0.8f)))
            {
                isMyListPage = false;
                isLoading = true;
                products.Clear();
                FetchProducts(searchString);
                isSearching = false;

            }

            GUILayout.EndHorizontal();
        }

        private static void SearchInMyList(string searchQuery)
        {

            if (!string.IsNullOrEmpty(searchQuery))
            {
                searchQuery = searchQuery.ToLower();
                markedProducts = markedProducts.FindAll(p => p.name.ToLower().Contains(searchQuery) || p.creator_name.ToLower().Contains(searchQuery));
            }
        }

        public static void SortProductsByDate()
        {
            products.Sort((p1, p2) => DateTime.Compare(DateTime.Parse(p2.created_at), DateTime.Parse(p1.created_at)));
            ResetScrollPosition();
        }


        public static void SortProductsByPriceAscending()
        {
            products.Sort((p1, p2) => p1.price.CompareTo(p2.price));
            ResetScrollPosition();
        }


        public static void SortProductsByPriceDescending()
        {
            products.Sort((p1, p2) => p2.price.CompareTo(p1.price));
            ResetScrollPosition();
        }


        public static void SortMyListByDate()
        {
            markedProducts.Sort((p1, p2) => DateTime.Compare(DateTime.Parse(p2.created_at), DateTime.Parse(p1.created_at)));
            ResetScrollPosition();
        }

        public static void SortMyListByPriceAscending()
        {
            markedProducts.Sort((p1, p2) => p1.price.CompareTo(p2.price));
            ResetScrollPosition();
        }

        public static void SortMyListByPriceDescending()
        {
            markedProducts.Sort((p1, p2) => p2.price.CompareTo(p1.price));
            ResetScrollPosition();
        }


        public static void DeleteAllImagesInFolder()
        {

            HashSet<string> protectedImages = new HashSet<string>();


            foreach (var product in products.Concat(markedProducts))
            {
                if (!string.IsNullOrEmpty(product.imagePath))
                {
                    protectedImages.Add(product.imagePath);
                }
            }


            DirectoryInfo di = new DirectoryInfo(ThumbnailDirectory);
            foreach (FileInfo file in di.GetFiles())
            {
                if (!protectedImages.Contains(file.FullName))
                {
                    file.Delete();
                }
            }
        }


        public static void ResetScrollPosition()
        {
            scrollPosition = Vector2.zero;
        }

        private static void DrawProductList(float width, float height)
        {
            Rect ProductlistArea = new Rect(0, 0, width, height);
            EditorGUI.DrawRect((ProductlistArea), Color.white);

            GUILayout.BeginArea(ProductlistArea);
            scrollPosition = GUILayout.BeginScrollView(scrollPosition, GUILayout.Width(width), GUILayout.Height(height));


            {
                for (int i = 0; i < products.Count; i++)
                {
                    if (i % 4 == 0) GUILayout.BeginHorizontal();
                    DrawProduct(width / 2, products[i]);
                    if ((i % 4 == 3) || i == products.Count - 1) GUILayout.EndHorizontal();
                }
            }

            if (!isLoading && products.Count== 0 && isSearching)
            {
                GUILayout.Label(Get(821), h1LabelStyle);
            }
            else if (!isLoading && products.Count % 20 == 0)
            {
                if (GUILayout.Button(Getc("more", 805), SubButtonStyle))
                {
                    currentPage++;
                    FetchProducts(searchString, currentPage);
                }
            }

            if (!isLoading)
            {
                GUILayout.Space(20);

                if (GUILayout.Button(Getc("open_in_browser", 812), SubButtonStyle))
                {
                    Application.OpenURL("https://www.vrcw.net");
                }

                GUILayout.Space(20);
            }
            else
            {
                GUILayout.Label(Get(804), h1LabelStyle);
            }

            GUILayout.EndScrollView();
            GUILayout.EndArea();
        }

        private static void DrawOperationArea(float width)
        {
            GUILayout.BeginHorizontal(GUI.skin.box, GUILayout.Width(width), GUILayout.Height(50));

            GUILayout.Label(Get(806), Centerh2LabelStyle);

            GUILayout.EndHorizontal();
        }

        private static void DrawProduct(float width, Product product)
        {



            var productBoxStyle = new GUIStyle(GUI.skin.box);


            GUILayout.BeginHorizontal(productBoxStyle, GUILayout.Width(productWidth), GUILayout.Height(productWidth));


            if (!string.IsNullOrEmpty(product.imagePath))
            {

                Texture2D image;
                if (!imageCache.TryGetValue(product.imagePath, out image))
                {
                    image = AssetDatabase.LoadAssetAtPath<Texture2D>(product.imagePath);
                    if (image != null)
                    {
                        imageCache[product.imagePath] = image;
                    }
                }

                if (image != null)
                {
                    float aspectRatio = (float)image.width / image.height;
                    float adjustedHeight = imageWidth / aspectRatio;
                    GUILayout.Label(new GUIContent(image), GUILayout.Width(imageWidth), GUILayout.Height(adjustedHeight));
                }
            }


            GUILayout.BeginVertical();
            GUILayout.Label(product.name, MarketproductsLabelStyle, GUILayout.Width(productWidth));
            GUILayout.Label(FormatPrice(product.price, product.price_display_mode), MarketproductsLabelStyle, GUILayout.Width(productWidth));
            GUILayout.Label(Get(807) + product.creator_name, MarketproductsLabelStyle, GUILayout.Width(productWidth));
            GUILayout.Label(Get(808) + product.created_at, GUILayout.Width(productWidth));
            if (GUILayout.Button(Getc("shopping_bag", 809), MarketProductsButtonStyle, GUILayout.Width(productWidth)))
            {
                Application.OpenURL(product.url);
            }
            if (product.creator_twitter.url != null)
            {
                if (GUILayout.Button(Getc("open_in_browser", 810), MarketProductsButtonStyle, GUILayout.Width(productWidth)))
                {
                    Application.OpenURL(product.creator_twitter.url);
                }
            }

            bool isMarked = IsProductMarked(product.id);

            if (isMarked)
            {
                GUILayout.Label(Get(817), MarketproductsLabelStyle, GUILayout.Width(productWidth));
            }
            else
            {
                if (GUILayout.Button(Getc("tag", 814), MarketProductsButtonStyle, GUILayout.Width(productWidth)))
                {
                    MarkProduct(product);
                }
            }
            GUILayout.EndVertical();

            GUILayout.EndHorizontal();
        }

        private static string FormatPrice(int price, string priceDisplayMode)
        {
            string money_unit = Get(811);
            switch (priceDisplayMode)
            {
                case "min":
                    return $"{price} {money_unit} ~";
                case "max":
                    return $"~ {price} {money_unit}";

                    return $"{price} {money_unit}";
            }
        }

        private static bool IsProductMarked(int productId)
        {
            string markFilePath = "Packages/com.sabuworks.eauploader/Editor/Scripts/MainWindow/TabContents/VRCWMarket/mark.json";
            if (File.Exists(markFilePath))
            {
                string jsonTextFromFile = File.ReadAllText(markFilePath);
                MarkedProductList markedProductList = JsonUtility.FromJson<MarkedProductList>(jsonTextFromFile);


                if (markedProductList.markedProducts != null)
                {
                    foreach (var markedProduct in markedProductList.markedProducts)
                    {
                        if (markedProduct.id == productId)
                        {
                            return true;
                        }
                    }
                }
            }
            return false;
        }


        private static void FetchProducts(string searchQuery = "", int page = 1)
        {
            isLoading = true;
            string url = $"https://www.vrcw.net/product/latest/json?page={page}";
            if (!string.IsNullOrEmpty(searchQuery))
            {
                url += $"&keyword={UnityWebRequest.EscapeURL(searchQuery)}";
            }

            currentRequest = UnityWebRequest.Get(url);
            currentRequest.SendWebRequest().completed += OnJsonDataDownloaded;

        }

        public static void ClearProductsAndFetchNew(string searchQuery = "")
        {
            isLoading = true;



            ResetScrollPosition();
        }


        private static void OnJsonDataDownloaded(AsyncOperation op)
        {
            var request = (op as UnityWebRequestAsyncOperation).webRequest;
            if (request.result == UnityWebRequest.Result.Success)
            {
                string jsonText = request.downloadHandler.text;

            }
            else
            {
                Debug.LogError("Network Error: " + request.error);
            }
            currentRequest = null;
        }


        private static async void DownloadAllProductImagesAsync()
        {
            isLoading = true;
            List<Task> downloadTasks = new List<Task>();

            for (int i = 0; i < products.Count; i++)
            {
                downloadTasks.Add(DownloadProductImageAsync(i));
            }

            await Task.WhenAll(downloadTasks);
            OnAllImagesDownloaded();
        }


        private static void OnAllImagesDownloaded()
        {
            AssetDatabase.Refresh();
            EditorApplication.QueuePlayerLoopUpdate();
            SceneView.RepaintAll();
            isLoading = false;
        }

        private static void ProcessProductsData(UnityWebRequest request)
        {
            if (request.result == UnityWebRequest.Result.Success)
            {
                try
                {
                    string jsonText = request.downloadHandler.text;


                }
                catch (Exception e)
                {
                    Debug.LogError("JSON Parsing Error: " + e.Message);
                }
            }
            else
            {
                Debug.LogError("Network Error: " + request.error);
            }
            currentRequest = null;
        }

        private static void LoadProductsFromFile()
        {
            if (File.Exists(jsonFilePath))
            {
                try
                {
                    string jsonText = File.ReadAllText(jsonFilePath);
                    ProductList productList = JsonUtility.FromJson<ProductList>(jsonText);
                    if (productList.products != null)
                    {
                        products = new List<Product>(productList.products);
                    }
                    else
                    {
                        Debug.LogError("Failed to parse products from JSON.");
                    }
                }
                catch (Exception e)
                {
                    Debug.LogError("File Read Error: " + e.Message);
                }
            }
        }

        private static void CheckImageRequest(UnityWebRequest imgRequest, Product product)
        {
            if (imgRequest.isDone)
            {
                if (imgRequest.result == UnityWebRequest.Result.Success)
                {
                    Texture2D texture = DownloadHandlerTexture.GetContent(imgRequest);
                    string path = ThumbnailDirectory + Path.GetFileName(imgRequest.url);
                    SaveTextureAsPNG(texture, path);

                }
                else
                {
                    Debug.LogError("Image Download Error: " + imgRequest.error);
                }

                EditorApplication.update -= () => CheckImageRequest(imgRequest, product);
            }
        }

        private static void CheckWebRequest()
        {
            if (currentRequest != null && currentRequest.isDone)
            {
                ProcessProductsData(currentRequest);
                EditorApplication.update -= CheckWebRequest;


                foreach (var product in products)
                {
                    StartImageDownload(product);
                }
            }
        }

        private static void StartImageDownload(Product product)
        {
            if (!string.IsNullOrEmpty(product.image_url) && !downloadRequests.ContainsKey(product.image_url))
            {
                UnityWebRequest imgRequest = UnityWebRequestTexture.GetTexture(product.image_url);
                downloadRequests.Add(product.image_url, imgRequest);
                imgRequest.SendWebRequest().completed += (op) =>
                {
                    if (imgRequest.result == UnityWebRequest.Result.Success)
                    {
                        Texture2D texture = DownloadHandlerTexture.GetContent(imgRequest);
                        string path = ThumbnailDirectory + Path.GetFileName(imgRequest.url);
                        SaveTextureAsPNG(texture, path);
                        product.imagePath = path;
                        imageCache[path] = texture;
                    }
                    else
                    {
                        Debug.LogError("Image Download Error: " + imgRequest.error);
                    }
                };
            }
        }

        private static async Task DownloadProductImageAsync(int index)
        {
            Product product = products[index];
            string imagePath = ThumbnailDirectory + product.id + ".png";

            if (!File.Exists(imagePath))
            {
                if (!string.IsNullOrEmpty(product.image_url))
                {
                    using (UnityWebRequest imgRequest = UnityWebRequestTexture.GetTexture(product.image_url))
                    {
                        await imgRequest.SendWebRequest().ToAwaitable();

                        if (imgRequest.result == UnityWebRequest.Result.Success)
                        {
                            Texture2D texture = DownloadHandlerTexture.GetContent(imgRequest);


                            if (texture.width > 512)
                            {
                                float aspectRatio = (float)texture.height / texture.width;
                                texture = ResizeTexture(texture, 512, (int)(512 * aspectRatio));
                            }

                            SaveTextureAsPNG(texture, imagePath);
                            product.imagePath = imagePath;
                            products[index] = product;
                        }
                        else
                        {
                            Debug.LogError("Image Download Error: " + imgRequest.error);
                        }
                    }
                }
            }
            else
            {
                product.imagePath = imagePath;
                products[index] = product;
            }
        }

        private static Texture2D ResizeTexture(Texture2D texture, int width, int height)
        {
            RenderTexture rt = new RenderTexture(width, height, 24);
            RenderTexture.active = rt;
            Graphics.Blit(texture, rt);
            Texture2D result = new Texture2D(width, height);
            result.ReadPixels(new Rect(0, 0, width, height), 0, 0);
            result.Apply();
            return result;
        }

        private static void SaveTextureAsPNG(Texture2D texture, string path)
        {
            byte[] bytes = texture.EncodeToPNG();
            string directoryPath = Path.GetDirectoryName(path);

            if (!Directory.Exists(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
            }

            File.WriteAllBytes(path, bytes);
        }

        private static void MarkProduct(Product product)
        {
            string markFilePath = "Packages/com.sabuworks.eauploader/Editor/Scripts/MainWindow/TabContents/VRCWMarket/mark.json";

            List<MarkedProduct> markedProducts = new List<MarkedProduct>();
            if (File.Exists(markFilePath))
            {
                string jsonTextFromFile = File.ReadAllText(markFilePath);
                if (!string.IsNullOrEmpty(jsonTextFromFile))
                {
                    MarkedProductList markedProductList = JsonUtility.FromJson<MarkedProductList>(jsonTextFromFile);
                    if (markedProductList.markedProducts != null)
                    {
                        markedProducts.AddRange(markedProductList.markedProducts);
                    }
                }
            }

            MarkedProduct markedProduct = new MarkedProduct
            {
                id = product.id,
                name = product.name,
                price = product.price,
                price_display_mode = product.price_display_mode,
                creator_name = product.creator_name,
                created_at = product.created_at,
                url = product.url,
                imagePath = product.imagePath,
                creator_twitter_url = product.creator_twitter.url
            };

            markedProducts.Add(markedProduct);

            MarkedProductList newMarkedProductList = new MarkedProductList { markedProducts = markedProducts.ToArray() };
            string newJson = JsonUtility.ToJson(newMarkedProductList);
            File.WriteAllText(markFilePath, newJson);

            string sourceImagePath = ThumbnailDirectory + product.id + ".png";
            string myListImagePath = MyListThumbnailDirectory + product.id + ".png";
            if (File.Exists(sourceImagePath) && !File.Exists(myListImagePath))
            {
                File.Copy(sourceImagePath, myListImagePath);
            }
        }

        private static void UnmarkProduct(int productId)
        {
            string markFilePath = "Packages/com.sabuworks.eauploader/Editor/Scripts/MainWindow/TabContents/VRCWMarket/mark.json";

            if (File.Exists(markFilePath))
            {
                string jsonTextFromFile = File.ReadAllText(markFilePath);
                MarkedProductList markedProductList = JsonUtility.FromJson<MarkedProductList>(jsonTextFromFile);


                if (markedProductList.markedProducts != null)
                {
                    var updatedList = new List<MarkedProduct>(markedProductList.markedProducts);


                    MarkedProductList newMarkedProductList = new MarkedProductList { markedProducts = updatedList.ToArray() };
                    string newJson = JsonUtility.ToJson(newMarkedProductList);
                    File.WriteAllText(markFilePath, newJson);
                }
            }

            string imagePath = MyListThumbnailDirectory + productId + ".png";
            if (File.Exists(imagePath))
            {
                File.Delete(imagePath);
            }
        }

        private static void DrawMyList(float width, float height)
        {
            Rect myListArea = new Rect(0, 0, width, height);
            EditorGUI.DrawRect(myListArea, Color.white);

            GUILayout.BeginArea(myListArea);
            scrollPosition = GUILayout.BeginScrollView(scrollPosition, GUILayout.Width(width), GUILayout.Height(height));


            LoadMyListProducts();


            {
                GUILayout.BeginHorizontal();
                if (i < markedProducts.Count)
                {

                }
                if (i + 1 < markedProducts.Count)
                {

                }
                GUILayout.EndHorizontal();
            }

            GUILayout.Space(20);
            GUILayout.EndScrollView();
            GUILayout.EndArea();
        }

        private static void DrawMyListProduct(float width, Product product)
        {



            var productBoxStyle = new GUIStyle(GUI.skin.box);


            GUILayout.BeginHorizontal(productBoxStyle, GUILayout.Width(productWidth), GUILayout.Height(productWidth));

            string imagePath = MyListThumbnailDirectory + product.id + ".png";
            if (File.Exists(imagePath))
            {
                Texture2D image = AssetDatabase.LoadAssetAtPath<Texture2D>(imagePath);
                if (image != null)
                {
                    float aspectRatio = (float)image.width / image.height;
                    float adjustedHeight = imageWidth / aspectRatio;
                    GUILayout.Label(new GUIContent(image), GUILayout.Width(imageWidth), GUILayout.Height(adjustedHeight));
                }
            }

            GUILayout.BeginVertical();


            GUILayout.Label(product.name, NoMargeWhiteh2LabelStyle, GUILayout.Width(productWidth));
            GUILayout.Label(FormatPrice(product.price, product.price_display_mode), NoMargeWhiteh2LabelStyle, GUILayout.Width(productWidth));
            GUILayout.Label(Get(807) + product.creator_name, NoMargeWhiteh2LabelStyle, GUILayout.Width(productWidth));
            GUILayout.Label(Get(808) + product.created_at, MarketproductsLabelStyle, GUILayout.Width(productWidth));
            if (GUILayout.Button(Getc("shopping_bag", 809), MarketProductsButtonStyle, GUILayout.Width(productWidth)))
            {
                Application.OpenURL(product.url);
            }


            if (!string.IsNullOrEmpty(product.creator_twitter.url))
            {
                if (GUILayout.Button(Getc("open_in_browser", 810), MarketProductsButtonStyle, GUILayout.Width(productWidth)))
                {
                    Application.OpenURL(product.creator_twitter.url);
                }
            }
            if (GUILayout.Button(Get(818), SubButtonStyle))
            {
                UnmarkProduct(product.id);
            }

            GUILayout.EndVertical();
            GUILayout.EndHorizontal();
        }

        private static void LoadMyListProducts()
        {
            string markFilePath = "Packages/com.sabuworks.eauploader/Editor/Scripts/MainWindow/TabContents/VRCWMarket/mark.json";
            if (File.Exists(markFilePath))
            {
                string jsonTextFromFile = File.ReadAllText(markFilePath);
                MarkedProductList markedProductList = JsonUtility.FromJson<MarkedProductList>(jsonTextFromFile);
                if (markedProductList.markedProducts != null)
                {

                    foreach (var markedProduct in markedProductList.markedProducts)
                    {

                        markedProducts.Add(new Product
                        {
                            id = markedProduct.id,
                            name = markedProduct.name,
                            price = markedProduct.price,
                            price_display_mode = markedProduct.price_display_mode,
                            creator_name = markedProduct.creator_name,
                            created_at = markedProduct.created_at,
                            url = markedProduct.url,
                            imagePath = markedProduct.imagePath,

                        });
                    }
                }
            }
        }

        private static void ProcessJsonData(string jsonText)
        {

            ProductList newProducts = JsonUtility.FromJson<ProductList>(jsonText);
            if (newProducts.products != null)
            {
                foreach (var newProduct in newProducts.products)
                {

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


            DownloadAllProductImagesAsync();
        }


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
            [NonSerialized] public string imagePath;
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

        public struct MarkedProductList
        {
            public MarkedProduct[] markedProducts;
        }

        [Serializable]
        public struct MarkedProduct
        {
            public int id;
            public string name;
            public int price;
            public string price_display_mode;
            public string creator_name;
            public string created_at;
            public string url;
            public string imagePath;
            public string creator_twitter_url;
        }

        [InitializeOnLoadMethod]
        private static void InitializeOnLoad()
        {
            FetchProducts();
        }
    }

    public static class UnityWebRequestAsyncExtension
    {
        public static Task<UnityWebRequest> ToAwaitable(this UnityWebRequestAsyncOperation asyncOp)
        {
            var tcs = new TaskCompletionSource<UnityWebRequest>();
            asyncOp.completed += (ao) => tcs.SetResult(asyncOp.webRequest);
            return tcs.Task;
        }
    }

    public class SortOptionsWindow : EditorWindow
    {
        private bool isMyList;

        public static void ShowWindow(bool isMyListPage)
        {
            var window = ScriptableObject.CreateInstance<SortOptionsWindow>();
            window.isMyList = isMyListPage;
            var windowSize = new Vector2(220, 180);
            window.ShowAsDropDown(new Rect(GUIUtility.GUIToScreenPoint(Event.current.mousePosition), Vector2.zero), windowSize);
        }

        private void OnGUI()
        {
            if (GUILayout.Button(Get(801), ClearButtonStyle, GUILayout.Height(50)))
            {
                if (!isMyList)
                {
                    VRCWMarket.SortProductsByDate();
                }
                else
                {
                    VRCWMarket.SortMyListByDate();
                }
                Close();
            }
            GUILayout.Space(5);
            if (GUILayout.Button(Get(802), ClearButtonStyle, GUILayout.Height(50)))
            {
                if(!isMyList)
                {
                    VRCWMarket.SortProductsByPriceDescending();
                }
                else
                {
                    VRCWMarket.SortMyListByPriceAscending();
                }
                Close();
            }
            GUILayout.Space(5);
            if (GUILayout.Button(Get(803), ClearButtonStyle, GUILayout.Height(50)))
            {
                if(!isMyList)
                {
                    VRCWMarket.SortProductsByPriceAscending();
                }
                else
                {
                    VRCWMarket.SortMyListByPriceDescending();
                }
                Close();
            }
        }
    }
}
