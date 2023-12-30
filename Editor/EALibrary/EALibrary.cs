using UnityEditor;
using UnityEngine;
using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using static labels;
using static styles;
using static Texture;

[System.Serializable]
public class Article
{
    public string title { get; set; }
    public string date { get; set; }
    public string[] tags { get; set; }
    public string[] keywords { get; set; }
    public string contentFile { get; set; }
    public string thumbnail { get; set; }
}

public class ArticleIndex
{
    public string Title { get; set; }
    public string[] Keywords { get; set; }
    public string[] Tags { get; set; }
}

public class LibraryIndexer
{
    private static string currentLanguage;

    public static void CreateIndex()
    {
        currentLanguage = LanguageUtility.GetCurrentLanguage();
        string articlesFolderPath = $"Packages/com.sabuworks.eauploader/Editor/EALibrary/Articles/EAUploader/{currentLanguage}";
        string[] jsonFiles = Directory.GetFiles(articlesFolderPath, "*.json", SearchOption.AllDirectories);

        List<ArticleIndex> indexList = new List<ArticleIndex>();
        foreach (var file in jsonFiles)
        {
            string jsonData = File.ReadAllText(file);
            Article article = JsonConvert.DeserializeObject<Article>(jsonData);
            indexList.Add(new ArticleIndex
            {
                Title = article.title,
                Keywords = article.keywords,
                Tags = article.tags
            });
        }
        string indexFilePath = "Packages/com.sabuworks.eauploader/Editor/EALibrary/library_index.json";
        File.WriteAllText(indexFilePath, JsonConvert.SerializeObject(indexList, Formatting.Indented));

        // タグの収集
        HashSet<string> tagSet = new HashSet<string>();
        foreach (var article in indexList)
        {
            foreach (var tag in article.Tags)
            {
                tagSet.Add(tag);
            }
        }

        // EALibraryインスタンスにタグリストを渡す
        EALibrary library = new EALibrary();
        library.UpdateTags(tagSet.ToList());
    }
}

public class EALibrary
{
    private string searchQuery = "";
    private Vector2 scrollPosition;
    private string currentLanguage = LanguageUtility.GetCurrentLanguage();
    private string currentArticleContent = "";
    private string currentArticleFilePath;
    private List<string> tags = new List<string>(); // tags List
    private List<string> searchResults = new List<string>();
    private int selectedTagIndex = 0;
    private bool searchPerformed = false;

    public EALibrary()
    {
        InitializeTags();
    }

    private void InitializeTags()
    {
        UpdateCurrentLanguage();
        LoadTags();
    }

    private void UpdateCurrentLanguage()
    {
        currentLanguage = LanguageUtility.GetCurrentLanguage();
    }

    private void LoadTags()
    {
        string indexFilePath = "Packages/com.sabuworks.eauploader/Editor/EALibrary/library_index.json";
        if (File.Exists(indexFilePath))
        {
            string indexJson = File.ReadAllText(indexFilePath);
            try{
                List<ArticleIndex> indexList = JsonConvert.DeserializeObject<List<ArticleIndex>>(indexJson);

                HashSet<string> tagSet = new HashSet<string>();
                foreach (var article in indexList)
                {
                    foreach (var tag in article.Tags)
                    {
                        tagSet.Add(tag);
                    }
                }

                UpdateTags(tagSet.ToList());
            }
            catch (JsonException ex)
            {
                Debug.LogError("JSON デシリアライズ中にエラーが発生しました: " + ex.Message);
            }
            catch (Exception ex)
            {
                Debug.LogError("予期しないエラーが発生しました: " + ex.Message);
            }
        }
    }

    public void Draw(Rect area)
    {
        UpdateCurrentLanguage();

        GUILayout.BeginArea(area);

        // Display the library title
        GUILayout.Label("EAUploader LIBRARY", h1LabelStyle);

        // Search field and button
        EditorGUILayout.BeginHorizontal();
        searchQuery = EditorGUILayout.TextField(searchQuery, styles.TextFieldStyle, GUILayout.Height(40));
        if (GUILayout.Button(Getc("search", 144), SearchButtonStyle)) // ボタンの高さもテキストフィールドと同じにする
        {
            if (!string.IsNullOrEmpty(searchQuery))
            {
                string selectedTag = tags[selectedTagIndex];
                SearchArticles(searchQuery, selectedTag);
            }
            else
            {
                searchPerformed = false;
            }
        }
        EditorGUILayout.EndHorizontal();

        // タグによるフィルタリングのドロップダウンメニュー
        EditorGUILayout.BeginHorizontal();
        GUILayout.Label(Get(600), NoMargeh2LabelStyle);
        int newSelectedTagIndex = EditorGUILayout.Popup(selectedTagIndex, tags.ToArray(), PopupStyle);
        if (newSelectedTagIndex != selectedTagIndex)
        {
            selectedTagIndex = newSelectedTagIndex;
            string selectedTag = tags[selectedTagIndex]; // 再宣言を避けるため、ここで selectedTag を更新
            SearchArticles(searchQuery, selectedTag); // タグが変更されたら検索を再実行
        }
        EditorGUILayout.EndHorizontal();
        Texture.DrawHorizontalLine(Color.black, 20, area.width*2);

        if (!string.IsNullOrEmpty(currentArticleContent))
        {
            // Back button
            if (GUILayout.Button(Getc("arrow_back", 601), SubButtonStyle, GUILayout.Height(40)))
            {
                Return();
            }
            Texture.DrawHorizontalDottedLine(Color.black, 12, area.width*2);
            // Display the content of the article
            scrollPosition = GUILayout.BeginScrollView(scrollPosition);
            ArticleRenderer.RenderRichTextContent(area, currentArticleContent);
            GUILayout.Space(30);
            GUILayout.EndScrollView();
        }
        else
        {
            // Scroll view for articles
            scrollPosition = GUILayout.BeginScrollView(scrollPosition);

            if (searchPerformed)
            {
                if (searchResults.Count > 0)
                {
                    DrawSearchResults(area);
                }
                else
                {
                    GUILayout.Label(Get(180), h2LabelStyle);
                    Texture.DrawHorizontalDottedLine(Color.black, 12, area.width*2);
                    if (selectedTagIndex == 0)
                    {
                        DisplayAllArticles(area);
                    }
                }
            }
            else
            {
                DisplayAllArticles(area);
            }
            GUILayout.Space(50);
            GUILayout.EndScrollView();
        }

        GUILayout.EndArea();
    }

    public void DrawPrivateLibrary(Rect area, string orderTag, int thumbnailSize)
    {
        GUILayout.BeginArea(area);

        GUILayout.Label("");

        EditorGUILayout.BeginHorizontal();
        searchQuery = EditorGUILayout.TextField(searchQuery, styles.TextFieldStyle, GUILayout.Height(40));
        if (GUILayout.Button(Get(144), SearchButtonStyle))
        {
            if (searchQuery != "")
            {
                SearchArticles(searchQuery, orderTag);
            }
            else
            {
                searchPerformed = false;
            }
        }
        EditorGUILayout.EndHorizontal();

        Texture.DrawHorizontalLine(Color.black, 20, area.width);
        

        if (!string.IsNullOrEmpty(currentArticleContent))
        {
            // Back button
            if (GUILayout.Button("Back", SubButtonStyle))
            {
                Return();
            }
            Texture.DrawHorizontalDottedLine(Color.black, 12, area.width*2);
            // Display the content of the article
            scrollPosition = GUILayout.BeginScrollView(scrollPosition);
            ArticleRenderer.RenderRichTextContent(area, currentArticleContent);
            GUILayout.Label(""); // 調整用
            GUILayout.EndScrollView();
        }
        else
        {
            // Scroll view for articles
            scrollPosition = GUILayout.BeginScrollView(scrollPosition);

            if (searchPerformed && searchResults.Count == 0)
            {
                GUILayout.Label(Get(180), h2LabelStyle);
                Texture.DrawHorizontalDottedLine(Color.black, 12, area.width);
                DisplayAllArticleswithTag(area, orderTag, thumbnailSize);
            }
            else if (searchResults.Count > 0)
            {
                DrawSearchResults(area);
            }
            else
            {
                DisplayAllArticleswithTag(area, orderTag, thumbnailSize);
            }

            GUILayout.EndScrollView();
        }

        GUILayout.EndArea();
    }

    public void ShowArticle(string content)
    {
        currentArticleContent = content;
        ArticleRenderer.currentArticleFilePath = currentArticleFilePath; // filePath を ArticleRenderer に渡す
    }

    public void Return()
    {
        currentArticleContent = null;
    }

    // Serch
    private void SearchArticles(string query, string orderTag)
    {
        string indexFilePath = "Packages/com.sabuworks.eauploader/Editor/EALibrary/library_index.json";
        string indexJson = File.ReadAllText(indexFilePath);
        List<ArticleIndex> indexList = JsonConvert.DeserializeObject<List<ArticleIndex>>(indexJson);

        searchPerformed = true;
        searchResults.Clear();

        foreach (var articleIndex in indexList)
        {
            bool queryMatches = string.IsNullOrEmpty(query) ||
                                articleIndex.Title.IndexOf(query, StringComparison.OrdinalIgnoreCase) >= 0 ||
                                articleIndex.Keywords.Any(keyword => keyword.IndexOf(query, StringComparison.OrdinalIgnoreCase) >= 0);
            bool tagMatches = orderTag == Get(182) || articleIndex.Tags.Contains(orderTag);
            if (queryMatches && tagMatches)
            {
                searchResults.Add(articleIndex.Title);
            }
        }
    }

    private void DisplayAllArticles(Rect area)
    {
        string articlesFolderPath = GetArticlesFolderPath();
        if (!Directory.Exists(articlesFolderPath))
        {
            Debug.LogError($"Directory not found: {articlesFolderPath}");
            return;
        }

        string[] jsonFiles = Directory.GetFiles(articlesFolderPath, "*.json", SearchOption.AllDirectories);
        foreach (var file in jsonFiles)
        {
            string jsonData = File.ReadAllText(file);
            Article article = JsonConvert.DeserializeObject<Article>(jsonData);

            EditorGUILayout.BeginHorizontal();
            // サムネイルの表示
            string thumbnailPath = Path.Combine(Path.GetDirectoryName(file), article.thumbnail);
            Texture2D thumbnailImage = AssetDatabase.LoadAssetAtPath<Texture2D>(thumbnailPath);
            if (thumbnailImage != null)
            {
                float aspectRatio = (float)thumbnailImage.width / thumbnailImage.height;
                float imageHeight = 100;
                float imageWidth = imageHeight * aspectRatio;
                GUILayout.Label("", GUILayout.Width(imageWidth), GUILayout.Height(imageHeight));
                Rect lastRect = GUILayoutUtility.GetLastRect();
                GUI.DrawTexture(new Rect(lastRect.x, lastRect.y, imageWidth, imageHeight), thumbnailImage);
            }
            if (GUILayout.Button($"{article.title}", LibraryButtonStyle))
            {
                // JSON ファイルのパスを currentArticleFilePath に設定
                currentArticleFilePath = file;

                // 記事の内容を表示
                string txtFilePath = Path.Combine(Path.GetDirectoryName(file), article.contentFile);
                ShowArticle(File.ReadAllText(txtFilePath));
            }
            EditorGUILayout.EndHorizontal();
            Texture.DrawHorizontalLine(Color.cyan, 12, area.width);
        }
    }

    private void DisplayAllArticleswithTag(Rect area, string orderTag, int thumbnailSize)
    {
        string articlesFolderPath = GetArticlesFolderPath();
        if (!Directory.Exists(articlesFolderPath))
        {
            Debug.LogError($"Directory not found: {articlesFolderPath}");
            return;
        }

        string[] jsonFiles = Directory.GetFiles(articlesFolderPath, "*.json", SearchOption.AllDirectories);
        foreach (var file in jsonFiles)
        {
            string jsonData = File.ReadAllText(file);
            Article article = JsonConvert.DeserializeObject<Article>(jsonData);

            if (article.tags.Contains(orderTag))
            {
                EditorGUILayout.BeginHorizontal();
                // サムネイルの表示
                string thumbnailPath = Path.Combine(Path.GetDirectoryName(file), article.thumbnail);
                Texture2D thumbnailImage = AssetDatabase.LoadAssetAtPath<Texture2D>(thumbnailPath);
                if (thumbnailImage != null)
                {
                    float aspectRatio = (float)thumbnailImage.width / thumbnailImage.height;
                    float imageHeight = thumbnailSize;
                    float imageWidth = imageHeight * aspectRatio;
                    GUILayout.Label("", GUILayout.Width(imageWidth), GUILayout.Height(imageHeight));
                    Rect lastRect = GUILayoutUtility.GetLastRect();
                    GUI.DrawTexture(new Rect(lastRect.x, lastRect.y, imageWidth, imageHeight), thumbnailImage);
                }
                if (GUILayout.Button($"{article.title}", LibraryButtonStyle))
                {
                    // JSON ファイルのパスを currentArticleFilePath に設定
                    currentArticleFilePath = file;

                    // 記事の内容を表示
                    string txtFilePath = Path.Combine(Path.GetDirectoryName(file), article.contentFile);
                    ShowArticle(File.ReadAllText(txtFilePath));
                }
                EditorGUILayout.EndHorizontal();
                Texture.DrawHorizontalLine(Color.cyan, 12, area.width);
            }
        }
    }

    private void DrawSearchResults(Rect area)
    {
        GUILayout.Label(Get(181,searchResults.Count), h2LabelStyle);

        if (searchResults.Count == 0)
        {
            GUILayout.Label(Get(180), h2LabelStyle);
            return;
        }

        foreach (var title in searchResults)
        {
            EditorGUILayout.BeginHorizontal();
            string jsonFilePath = FindArticlePathByTitle(title);
            if (!string.IsNullOrEmpty(jsonFilePath))
            {
                Article article = JsonConvert.DeserializeObject<Article>(File.ReadAllText(jsonFilePath));

                DisplayThumbnail(jsonFilePath, article);

                // Display article button
                if (GUILayout.Button(title, LibraryButtonStyle))
                {
                    string contentFilePath = Path.Combine(Path.GetDirectoryName(jsonFilePath), article.contentFile);
                    Debug.Log($"Opening article: {title}");
                    ShowArticle(File.ReadAllText(contentFilePath), jsonFilePath);
                }
            }
            EditorGUILayout.EndHorizontal();
            Texture.DrawHorizontalLine(Color.cyan, 12, area.width);
        }
    }

    private void DisplayThumbnail(string jsonFilePath, Article article)
    {
        string thumbnailPath = Path.Combine(Path.GetDirectoryName(jsonFilePath), article.thumbnail);
        Texture2D thumbnailImage = AssetDatabase.LoadAssetAtPath<Texture2D>(thumbnailPath);
        if (thumbnailImage != null)
        {
            // Texture2D から Sprite への変換
            Sprite thumbnailSprite = Texture2DToSprite(thumbnailImage);

            float aspectRatio = (float)thumbnailImage.width / thumbnailImage.height;
            float imageHeight = 100; // 適宜調整
            float imageWidth = imageHeight * aspectRatio;
            GUILayout.Label("", GUILayout.Width(imageWidth), GUILayout.Height(imageHeight));
            Rect lastRect = GUILayoutUtility.GetLastRect();
            
            // Sprite を描画
            if (thumbnailSprite != null)
            {
                GUI.DrawTexture(new Rect(lastRect.x, lastRect.y, imageWidth, imageHeight), thumbnailSprite.texture);
            }
        }
    }

    private Sprite Texture2DToSprite(Texture2D texture)
    {
        if (texture == null) return null;

        // Texture2D から Sprite の作成
        return Sprite.Create(texture, new Rect(0.0f, 0.0f, texture.width, texture.height), new Vector2(0.5f, 0.5f));
    }

    private string FindArticlePathByTitle(string title)
    {
        string articlesFolderPath = GetArticlesFolderPath();
        string[] jsonFiles = Directory.GetFiles(articlesFolderPath, "*.json", SearchOption.AllDirectories);
        foreach (var file in jsonFiles)
        {
            string jsonData = File.ReadAllText(file);
            Article article = JsonConvert.DeserializeObject<Article>(jsonData);
            if (article.title == title)
            {
                return file; // Return the full path of the JSON file
            }
        }
        return null;
    }

    public void ShowArticle(string content, string jsonFilePath)
    {
        currentArticleContent = content;
        currentArticleFilePath = jsonFilePath;
    }

    public void UpdateTags(List<string> newTags)
    {
        tags.Clear();
        tags.Add(Get(182));
        tags.AddRange(newTags);
        selectedTagIndex = 0; // タグリストが更新されたときにインデックスをリセット
    }

    private string GetArticlesFolderPath()
    {
        return $"Packages/com.sabuworks.eauploader/Editor/EALibrary/Articles/EAUploader/{currentLanguage}";
    }
}