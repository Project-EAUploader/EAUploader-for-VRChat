using UnityEditor;
using UnityEngine;
using System.IO;
using System.Collections.Generic;
using Newtonsoft.Json;
using static styles;

public class ArticleInfo
{
    public string ImageName { get; set; }
    public string Url { get; set; }
}

public static class EAInfo
{
    private static List<ArticleInfo> articles = new List<ArticleInfo>();
    private static int currentIndex = 0;
    private static float timer = 0;
    private static readonly float SlideInterval = 10.0f;
    private static readonly string folderPath = @"Packages\com.sabuworks.eauploader\Editor\Resources\Info";

    static EAInfo()
    {
        LoadArticles(folderPath);
    }

    private static void LoadArticles(string folderPath)
    {
        articles.Clear();
        var jsonFiles = Directory.GetFiles(folderPath, "*.json");
        foreach (var file in jsonFiles)
        {
            var jsonContent = File.ReadAllText(file);

            var articleInfos = JsonConvert.DeserializeObject<List<ArticleInfo>>(jsonContent);
            if (articleInfos != null)
            {
                articles.AddRange(articleInfos);
            }
        }
    }

    public static void Draw(Rect position)
    {

        Event currentEvent = Event.current;
        if (currentEvent.type == EventType.ScrollWheel && position.Contains(currentEvent.mousePosition))
        {

            return;
        }

        UpdateTimer();

        GUILayout.BeginArea(position);
        GUILayout.BeginHorizontal();

        GUIStyle buttonStyle = new GUIStyle(GUI.skin.button);
        buttonStyle.normal.background = null;


        if (GUILayout.Button("◀", prefabButtonStyle, GUILayout.Width(30), GUILayout.Height(position.height)))
        {
            PreviousSlide();
            ResetTimer();
        }

        if (articles != null && articles.Count > 0)
        {
            var article = articles[currentIndex];
            var imagePath = Path.Combine(folderPath, article.ImageName);
            if (File.Exists(imagePath))
            {
                Texture2D texture = AssetDatabase.LoadAssetAtPath<Texture2D>(imagePath);
                float aspectRatio = (float)texture.width / texture.height;

                float imageWidth = availableWidth;
                float imageHeight = availableWidth / aspectRatio;


                if (imageHeight > position.height)
                {
                    imageHeight = position.height;
                    imageWidth = imageHeight * aspectRatio;
                }



                if (GUI.Button(new Rect(imageX, 0, imageWidth, imageHeight), texture, GUIStyle.none))
                {
                    if (!string.IsNullOrEmpty(article.Url))
                    {
                        Application.OpenURL(article.Url);
                    }
                }
            }
        }



        if (GUI.Button(new Rect(rightButtonX, 0, 30, position.height), "▶", prefabButtonStyle))
        {
            NextSlide();
            ResetTimer();
        }

        GUILayout.EndHorizontal();
        GUILayout.EndArea();
    }

    private static void UpdateTimer()
    {
        timer += Time.deltaTime;
        if (timer >= SlideInterval)
        {
            NextSlide();
            ResetTimer();
        }
    }

    private static void NextSlide()
    {
        if (articles.Count > 0)
        {
            currentIndex = (currentIndex + 1) % articles.Count;

        }
    }

    private static void PreviousSlide()
    {
        if (articles.Count > 0)
        {
            currentIndex = (currentIndex - 1 + articles.Count) % articles.Count;

        }
    }

    private static void ResetTimer()
    {
        timer = 0;
    }
}
