#if !EA_ONBUILD
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
        string articlesFolderPath = $"Packages/tech.uslog.eauploader/Editor/EALibrary/Articles/EAUploader/{currentLanguage}";
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
        string indexFilePath = "Packages/tech.uslog.eauploader/Editor/EALibrary/library_index.json";
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
    private List<string> tags = new List<string>();
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
        string indexFilePath = "Packages/tech.uslog.eauploader/Editor/EALibrary/library_index.json";
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

    /// <summary>
    /// 通常のEALibraryを指定エリアに描画するAPI
    /// </summary>
    /// <param name="area"></param>
    public void Draw(Rect area)
    {
        UpdateCurrentLanguage();
        try
        {
            GUILayout.BeginArea(area);

            GUILayout.Label("EAUploader LIBRARY", h1LabelStyle);

            EditorGUILayout.BeginHorizontal();
            searchQuery = EditorGUILayout.TextField(searchQuery, styles.TextFieldStyle, GUILayout.Height(40));
            if (GUILayout.Button(Getc("search", 144), SearchButtonStyle))
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

            EditorGUILayout.BeginHorizontal();
            GUILayout.Label(Get(600), NoMargeh2LabelStyle);
            int newSelectedTagIndex = EditorGUILayout.Popup(selectedTagIndex, tags.ToArray(), PopupStyle);
            if (newSelectedTagIndex != selectedTagIndex)
            {
                selectedTagIndex = newSelectedTagIndex;
                string selectedTag = tags[selectedTagIndex];
                SearchArticles(searchQuery, selectedTag);
            }
            EditorGUILayout.EndHorizontal();
            Texture.DrawHorizontalLine(Color.black, 20, area.width*2);

            if (!string.IsNullOrEmpty(currentArticleContent))
            {
                if (GUILayout.Button(Getc("arrow_back", 601), SubButtonStyle, GUILayout.Height(40)))
                {
                    Return();
                }
                Texture.DrawHorizontalDottedLine(Color.black, 12, area.width*2);

                scrollPosition = GUILayout.BeginScrollView(scrollPosition);
                ArticleRenderer.RenderRichTextContent(area, currentArticleContent);
                GUILayout.Space(30);
                GUILayout.EndScrollView();
            }
            else
            {
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
        catch(Exception ex)
        {
            Debug.LogError("EALibrary.Draw ERROR:" + ex);
        }
        
    }

    /// <summary>
    /// orderTagでフィルタリングした上でライブラリをエリアに描画
    /// thumbnailSizeはサムネイル画像の横幅
    /// </summary>
    /// <param name="area"></param>
    /// <param name="orderTag"></param>
    /// <param name="thumbnailSize"></param>
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
            if (GUILayout.Button("Back", SubButtonStyle))
            {
                Return();
            }
            Texture.DrawHorizontalDottedLine(Color.black, 12, area.width*2);

            scrollPosition = GUILayout.BeginScrollView(scrollPosition);
            ArticleRenderer.RenderRichTextContent(area, currentArticleContent);
            GUILayout.Space(30);
            GUILayout.EndScrollView();
        }
        else
        {
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
        ArticleRenderer.currentArticleFilePath = currentArticleFilePath;
    }

    public void Return()
    {
        currentArticleContent = null;
    }

    private void SearchArticles(string query, string orderTag)
    {
        string indexFilePath = "Packages/tech.uslog.eauploader/Editor/EALibrary/library_index.json";
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
                currentArticleFilePath = file;

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
                    currentArticleFilePath = file;

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
            Sprite thumbnailSprite = Texture2DToSprite(thumbnailImage);

            float aspectRatio = (float)thumbnailImage.width / thumbnailImage.height;
            float imageHeight = 100;
            float imageWidth = imageHeight * aspectRatio;
            GUILayout.Label("", GUILayout.Width(imageWidth), GUILayout.Height(imageHeight));
            Rect lastRect = GUILayoutUtility.GetLastRect();
            
            if (thumbnailSprite != null)
            {
                GUI.DrawTexture(new Rect(lastRect.x, lastRect.y, imageWidth, imageHeight), thumbnailSprite.texture);
            }
        }
    }

    private Sprite Texture2DToSprite(Texture2D texture)
    {
        if (texture == null) return null;

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
                return file;
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
        selectedTagIndex = 0;
    }

    private string GetArticlesFolderPath()
    {
        return $"Packages/tech.uslog.eauploader/Editor/EALibrary/Articles/EAUploader/{currentLanguage}";
    }
}
#endif