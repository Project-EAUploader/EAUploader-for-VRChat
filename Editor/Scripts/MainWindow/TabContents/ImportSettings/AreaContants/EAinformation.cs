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
            // 以下の行を修正
            var articleInfos = JsonConvert.DeserializeObject<List<ArticleInfo>>(jsonContent);
            if (articleInfos != null)
            {
                articles.AddRange(articleInfos);
            }
        }
    }

    public static void Draw(Rect position)
    {
        // 現在のイベントをチェック
        Event currentEvent = Event.current;
        if (currentEvent.type == EventType.ScrollWheel && position.Contains(currentEvent.mousePosition))
        {
            // スクロールイベントを無視し、処理をここで終了
            return;
        }

        UpdateTimer();

        GUILayout.BeginArea(position);
        GUILayout.BeginHorizontal();

        GUIStyle buttonStyle = new GUIStyle(GUI.skin.button);
        buttonStyle.normal.background = null;

        // 左矢印ボタン
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
                float availableWidth = position.width - 60; // 60ピクセルは左右の矢印ボタンの幅の合計
                float imageWidth = availableWidth;
                float imageHeight = availableWidth / aspectRatio;

                // 画像の高さが画面の高さを超えないように調整
                if (imageHeight > position.height)
                {
                    imageHeight = position.height;
                    imageWidth = imageHeight * aspectRatio;
                }

                // 画像ボタンの描画
                float imageX = 30 + (availableWidth - imageWidth) / 2; // 画像を中央に配置
                if (GUI.Button(new Rect(imageX, 0, imageWidth, imageHeight), texture, GUIStyle.none))
                {
                    if (!string.IsNullOrEmpty(article.Url))
                    {
                        Application.OpenURL(article.Url);
                    }
                }
            }
        }

        // 右矢印ボタン
        float rightButtonX = position.width - 30; // 右端から30ピクセルの位置
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
            ResetTimer(); // スライドを切り替えた後、タイマーをリセット
        }
    }

    private static void PreviousSlide()
    {
        if (articles.Count > 0)
        {
            currentIndex = (currentIndex - 1 + articles.Count) % articles.Count;
            ResetTimer(); // スライドを切り替えた後、タイマーをリセット
        }
    }

    private static void ResetTimer()
    {
        timer = 0;
    }
}
