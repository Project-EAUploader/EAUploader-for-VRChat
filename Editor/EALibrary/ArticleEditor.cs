/*
using UnityEditor;
using UnityEngine;
using System;
using System.Linq;
using System.IO;
using static styles;
using static Texture;

public class ArticleEditor : EditorWindow
{
    private string newFolderName = "";

    private string selectedFolderPath;
    private bool folderSelected = false;
    private string title = "";
    private string thumbnail = "";
    private string tags = "";
    private string keywords = "";
    private float previewWidth = 0;
    private Vector2 scrollPosition; 

    [MenuItem("EAUploader/Article Editor")]
    public static void ShowWindow()
    {
        var window = GetWindow<ArticleEditor>("EALibrary Article Editor");
        window.minSize = new Vector2(800, 600); // 最小サイズの設定
    }

    private void OnEnable()
    {

    }

    void OnGUI()
    {
        // 全体の背景色を白に設定
        EditorGUI.DrawRect(new Rect(0, 0, position.width, position.height), Color.white);

        GUILayout.BeginHorizontal();

        // 左側のエリア全体
        GUILayout.BeginVertical(GUILayout.Width(position.width / 2));
        DrawLeftSide();
        GUILayout.EndVertical();

        // 境界線の代わりにスペースを追加
        GUILayout.Space(1);

        // 右側のエリア
        GUILayout.BeginVertical(GUILayout.Width(position.width / 2));
        float drawPreviewWidth = position.width / 2;
        DrawPreview(drawPreviewWidth); // 追加の引数を渡します
        GUILayout.EndVertical();

        GUILayout.EndHorizontal();
    }

    private void DrawVerticalLine(float xPosition, float height)
    {
        EditorGUI.DrawRect(new Rect(xPosition, 0, 1, height), Color.black);
    }

    private void DrawLeftSide()
    {
        GUILayout.BeginHorizontal();

        // 左側エリアの左部分の幅
        float leftPartWidth = (position.width*0.15f);

        // 左側エリアの左部分
        GUILayout.BeginVertical(GUILayout.Width(leftPartWidth));
        DrawArticlesList(leftPartWidth);
        GUILayout.EndVertical();

        // 左側エリアの右部分
        GUILayout.BeginVertical(GUILayout.Width((position.width*0.35f)));
        DrawEditor();
        GUILayout.EndVertical();

        GUILayout.EndHorizontal();
    }

    private void DrawPreview(float widtht)
    {
        GUILayout.Label("Preview", h1LabelStyle);

        if (folderSelected)
        {
            if (HasExistingJSON(selectedFolderPath))
            {
                // 横幅サイズの入力欄
                GUILayout.Label("Preview Width", NoMargeh2LabelStyle);

                string jsonPath = Path.Combine(selectedFolderPath, "article.json");
                if (File.Exists(jsonPath))
                {
                    string jsonContent = File.ReadAllText(jsonPath);
                    var articleData = JsonUtility.FromJson<ArticleData>(jsonContent);

                    // サムネイルとタイトルの表示
                    GUILayout.BeginHorizontal();
                    if (!string.IsNullOrEmpty(articleData.thumbnail))
                    {
                        // 正しいアセットパスに変更する必要があります
                        string assetPath = ConvertToAssetPath(articleData.thumbnail);
                        Texture2D thumbnailTexture = AssetDatabase.LoadAssetAtPath<Texture2D>(assetPath);
                        if (thumbnailTexture != null)
                        {
                            GUILayout.Label(new GUIContent(thumbnailTexture), GUILayout.Width(200), GUILayout.Height(200));
                        }
                    }
                    else
                    {
                        GUILayout.Label("サムネイルを指定してください", h2LabelStyle);
                    }
                    GUILayout.Label(articleData.title, h1LabelStyle);
                    GUILayout.EndHorizontal();
                }

                string articleContent = GetArticleContentFromSelectedFolder();
                if (!string.IsNullOrEmpty(articleContent))
                {
                    // スクロールビューの追加
                    scrollPosition = GUILayout.BeginScrollView(scrollPosition);
                    Rect previewArea = new Rect(0, 100, previewWidth, position.height - 100);

                    // ArticleRendererに現在の記事ファイルパスを設定
                    ArticleRenderer.currentArticleFilePath = selectedFolderPath;

                    // 記事の内容をレンダリング
                    ArticleRenderer.RenderRichTextContent(previewArea, articleContent);
                    GUILayout.EndScrollView();
                }
            }
        }
    }

    private string ConvertToAssetPath(string originalPath)
    {
        // 例: "Packages/com.sabuworks.eauploader/Editor/EALibrary/Articles/Editing/SDK1/SDK1_Thumbnail.png" 
        // を "Assets/MyFolder/SDK1_Thumbnail.png" に変換
        // パス変換ロジックをここに実装
        string relativePath = ;
        return relativePath;
    }

    private string GetArticleContentFromSelectedFolder()
    {
        var txtFiles = Directory.GetFiles(selectedFolderPath, "*.txt");
        if (txtFiles.Length > 0)
        {
            return File.ReadAllText(txtFiles[0]);
        }
        return null;
    }

    private void DrawArticlesList(float width)
    {
        // 左側記事一覧と作成ボタン
        newFolderName = EditorGUILayout.TextField("", newFolderName, TextFieldStyle, GUILayout.Height(30));
        if (GUILayout.Button("Create New Folder", SubButtonStyle))
        {
            CreateNewFolder(newFolderName);
            newFolderName = "";
        }

        string editingPath = "Packages/com.sabuworks.eauploader/Editor/EALibrary/Articles/Editing";
        DisplayFolders(editingPath, width);
    }

    private void DrawEditor()
    {
        if (folderSelected)
        {
            if (HasExistingJSON(selectedFolderPath))
            {
                DrawContentEditor();
            }
            else
            {
                DrawNewArticleUI();
            }
        }
    }

    private void CreateNewFolder(string folderName)
    {
        string path = Path.Combine("Packages/com.sabuworks.eauploader/Editor/EALibrary/Articles/Editing", folderName);
        if (Directory.Exists(path))
        {
            EditorUtility.DisplayDialog("Error", "A folder with the same name already exists.", "OK");
        }
        else
        {
            Directory.CreateDirectory(path);
            AssetDatabase.Refresh();
        }
    }

    private void DisplayFolders(string path, float width)
    {
        var directories = Directory.GetDirectories(path);
        foreach (var directory in directories)
        {
            string folderName = Path.GetFileName(directory);
            if (GUILayout.Button(folderName, prefabButtonStyle))
            {
                selectedFolderPath = directory;
                folderSelected = true;
                CheckForExistingJSON(directory);
            }
            DrawHorizontalDottedCenterLine(Color.black, 8, width);
        }
    }

    private void CheckForExistingJSON(string folderPath)
    {
        var jsonFiles = Directory.GetFiles(folderPath, "*.json");
        if (jsonFiles.Length == 0)
        {
            // JSONファイルがない場合の処理
            DrawNewArticleUI();
        }
        else
        {
            // JSONファイルがある場合の処理（未実装）
        }
    }

    private void DrawNewArticleUI()
    {
        GUILayout.Label("Create New Article", h1LabelStyle);
        GUILayout.Label("Title", NoMargeh2LabelStyle);
        title = EditorGUILayout.TextField(title, TextFieldStyle);
        GUILayout.Label("Thumbnail", NoMargeh2LabelStyle);
        thumbnail = EditorGUILayout.TextField(thumbnail, TextFieldStyle);
        GUILayout.Label("Tags (comma separated)", NoMargeh2LabelStyle);
        tags = EditorGUILayout.TextField(tags, TextFieldStyle);
        GUILayout.Label("Keywords (comma separated)", NoMargeh2LabelStyle);
        keywords = EditorGUILayout.TextField(keywords, TextFieldStyle);

        if (GUILayout.Button("Create Article", MainButtonStyle))
        {
            SaveNewArticle();
        }
    }

    private void DrawContentEditor()
    {
        GUILayout.Label("Edit Content", h1LabelStyle);
        // ここにContent.txtの編集UIを追加
    }

    private void SaveNewArticle()
    {
        var articleData = new ArticleData
        {
            title = title,
            date = DateTime.Now.ToString("yyyy-MM-dd"),
            tags = tags.Split(',').Select(t => t.Trim()).ToArray(),
            keywords = keywords.Split(',').Select(k => k.Trim()).ToArray(),
            contentFile = "Content.txt",
            thumbnail = thumbnail
        };

        string jsonPath = Path.Combine(selectedFolderPath, "article.json");
        string contentPath = Path.Combine(selectedFolderPath, "Content.txt");

        string json = JsonUtility.ToJson(articleData, true);
        File.WriteAllText(jsonPath, json);

        // Content.txtが存在しない場合は新しく作成
        if (!File.Exists(contentPath))
        {
            File.WriteAllText(contentPath, "");
        }

        AssetDatabase.Refresh();
    }

    private bool HasExistingJSON(string folderPath)
    {
        var jsonFiles = Directory.GetFiles(folderPath, "*.json");
        return jsonFiles.Length > 0;
    }

    private class ArticleData
    {
        public string title;
        public string date;
        public string[] tags;
        public string[] keywords;
        public string contentFile;
        public string thumbnail;
    }
}
*/