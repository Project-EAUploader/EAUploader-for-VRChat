#if !EA_ONBUILD
using UnityEngine;
using UnityEditor;
using System.IO;
using static LanguageUtility;

public static class AvatarWorldTabDrawer
{
    private static readonly float BorderHeight = 1f;
    private static Vector2[] scrollPositions = new Vector2[4];

    public static void Draw(Rect position)
    {
        string language = LanguageUtility.GetCurrentLanguage();
        EditorGUI.DrawRect(new Rect(0, 0, position.width, position.height), Color.white);

        float areaHeight = (position.height - BorderHeight * 3) / 4;

        DrawArea(position, 0, areaHeight, "Booth", $"{language}/Booth.txt", "Packages/tech.uslog.eauploader/Editor/Scripts/MainWindow/TabContents/AvatarWorld/AreaContents/Thumbnail/Booth_logo.png", "https://booth.pm/ja/items?tags%5B%5D=VRChat", 0);
        EditorGUI.DrawRect(new Rect(0, areaHeight, position.width, BorderHeight), Color.gray);

        DrawArea(position, areaHeight + BorderHeight, areaHeight, "EDEN", $"{language}/EDEN.txt", "Packages/tech.uslog.eauploader/Editor/Scripts/MainWindow/TabContents/AvatarWorld/AreaContents/Thumbnail/EDEN.png", "https://eden-world.net/market/", 1);
        EditorGUI.DrawRect(new Rect(0, 2 * areaHeight + BorderHeight, position.width, BorderHeight), Color.gray);

        DrawArea(position, 2 * areaHeight + 2 * BorderHeight, areaHeight, "VRChatの世界", $"{language}/vrcw.txt", "Packages/tech.uslog.eauploader/Editor/Scripts/MainWindow/TabContents/AvatarWorld/AreaContents/Thumbnail/vrcw.png", "https://www.vrcw.net/", 2);
        EditorGUI.DrawRect(new Rect(0, 3 * areaHeight + 2 * BorderHeight, position.width, BorderHeight), Color.gray);

        GUILayout.BeginArea(new Rect(0, 3 * areaHeight + 3 * BorderHeight, position.width, areaHeight));
        // 残りエリア
        GUILayout.EndArea();
    }

    private static void DrawArea(Rect position, float startY, float height, string areaName, string textFilePath, string imagePath, string link, int areaIndex)
    {
        float halfWidth = (position.width - BorderHeight) / 2;
        string filePath = $"Packages/tech.uslog.eauploader/Editor/Scripts/MainWindow/TabContents/AvatarWorld/AreaContents/{textFilePath}";
        string content = File.ReadAllText(filePath);

        GUILayout.BeginArea(new Rect(0, startY, halfWidth, height));
        DrawImageAsButton(imagePath, new Rect(0, 0, halfWidth, height), link);
        GUILayout.EndArea();

        GUILayout.BeginArea(new Rect(halfWidth + BorderHeight, startY, halfWidth, height));
        scrollPositions[areaIndex] = GUILayout.BeginScrollView(scrollPositions[areaIndex], GUILayout.Width(halfWidth), GUILayout.Height(height));
        ArticleRenderer.RenderRichTextContent(new Rect(0, 0, halfWidth, height), content);
        GUILayout.EndScrollView();
        GUILayout.EndArea();
    }

    private static void DrawImageAsButton(string imagePath, Rect area, string link)
    {
        Texture2D image = AssetDatabase.LoadAssetAtPath<Texture2D>(imagePath);
        if (image != null)
        {
            float aspectRatio = (float)image.width / image.height;
            float adjustedHeight = area.width / aspectRatio;
            float centeredY = area.y + (area.height - adjustedHeight) / 2;
            Rect imageRect = new Rect(area.x, centeredY, area.width, adjustedHeight);

            if (GUI.Button(imageRect, image, GUIStyle.none))
            {
                Application.OpenURL(link);
            }
        }
        else
        {
            EditorGUI.LabelField(area, "Image not found");
        }
    }
}
#endif