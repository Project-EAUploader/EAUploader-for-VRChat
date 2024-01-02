using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using static labels;
using static styles;
using static Texture;

public class EABuilder
{
    private static readonly float BorderWidth = 2f;
    private static Dictionary<string, Texture2D> avatarsWithPreview = new Dictionary<string, Texture2D>();
    private static GameObject selectedPrefabInstance = null;
    private static Editor gameObjectEditor;
    private static Vector2 scrollPosition;
    public static bool avatarsListUpdated = true;

    public static void Draw(Rect drawArea)
    {
        styles.Initialize();

        GUILayout.BeginHorizontal();
        float totalWidth = drawArea.width;
        float listWidth = totalWidth * 4 / 10 - BorderWidth;
        float previewAndToolWidth = totalWidth * 6 / 10 - 2 * BorderWidth;
        float previewHeight = drawArea.height * 0.7f - BorderWidth;
        float toolHeight = drawArea.height * 0.3f;

        DrawAvatarList(new Rect(0, 0, listWidth, drawArea.height));
        DrawVerticalBorder(new Rect(listWidth, 0, BorderWidth, drawArea.height));

        DrawAvatarPreview(new Rect(listWidth + BorderWidth, 0, previewAndToolWidth, previewHeight));
        DrawHorizontalBorder(new Rect(listWidth + BorderWidth, previewHeight, previewAndToolWidth, BorderWidth));
        DrawUploadTool(new Rect(listWidth + BorderWidth, previewHeight + BorderWidth, previewAndToolWidth, toolHeight));

        GUILayout.EndHorizontal();
    }

    private static void DrawAvatarList(Rect area)
    {
        float windowWidth = EditorGUIUtility.currentViewWidth;
        GUILayout.BeginArea(area);
        GUILayout.Label(Get(116), h1LabelStyle);
        if (GUILayout.Button(Getc("help", 500), HelpButtonStyle))
        {
            EAUploaderMessageWindow.ShowMsg(105);
        }
        DrawHorizontalDottedLine(Color.black, 12, windowWidth);
        
        if (avatarsListUpdated)
        {
            var currentAvatarsWithPreview = CustomPrefabUtility.GetVrchatAvatarList();
            if (avatarsWithPreview.Count == 0 || !IsSameDictionary(avatarsWithPreview, currentAvatarsWithPreview))
            {
                avatarsWithPreview = currentAvatarsWithPreview;
            }
            avatarsListUpdated = false;
        }

        scrollPosition = GUILayout.BeginScrollView(scrollPosition);
        foreach (var avatarEntry in avatarsWithPreview)
        {
            string avatarKey = avatarEntry.Key;
            Texture2D preview = avatarEntry.Value;

            string avatarName = CustomPrefabUtility.GetVrchatAvatarName(avatarKey);

            GUIContent content = new GUIContent(avatarName, preview);

            if (GUILayout.Button(content, prefabButtonStyle, GUILayout.Height(80)))
            {
                CustomPrefabUtility.SelectPrefabAndSetupScene(avatarKey);
                selectedPrefabInstance = CustomPrefabUtility.selectedPrefabInstance;
                if (gameObjectEditor != null)
                {
                    UnityEngine.Object.DestroyImmediate(gameObjectEditor);
                }
                gameObjectEditor = Editor.CreateEditor(selectedPrefabInstance);
            }
            DrawHorizontalLine(Color.cyan, 12, windowWidth);
        }
        GUILayout.Space(30);
        GUILayout.EndScrollView();
        GUILayout.EndArea();
    }

    private static void DrawAvatarPreview(Rect area)
    {
        GUILayout.BeginArea(area);

        if (selectedPrefabInstance != null)
        {
            GUILayout.Label(Get(112) + selectedPrefabInstance.name, h1LabelStyle);

            if (gameObjectEditor == null)
            {
                gameObjectEditor = Editor.CreateEditor(selectedPrefabInstance);
            }

            if (gameObjectEditor != null)
            {
                GUIStyle bgColor = new GUIStyle();
                bgColor.normal.background = EditorGUIUtility.whiteTexture;
                float aspectRatio = 1.0f;
                float previewSize = Mathf.Min(area.width, area.height * aspectRatio);
                Rect previewRect = GUILayoutUtility.GetRect(previewSize, previewSize / aspectRatio); 
                gameObjectEditor.OnInteractivePreviewGUI(previewRect, bgColor);
            }
        }
        else
        {
            GUILayout.FlexibleSpace();

            GUILayout.Label(Get(115), CenteredStyle, GUILayout.ExpandWidth(true));

            GUILayout.FlexibleSpace();
        }

        GUILayout.EndArea();
    }

    private static void DrawUploadTool(Rect area)
    {
        GUILayout.BeginArea(area);
        GUILayout.Label(Get(173), h1LabelStyle);
        if(selectedPrefabInstance != null)
        {
            if (GUILayout.Button(Getc("help", 500), HelpButtonStyle))
                {
                    EAUploaderMessageWindow.ShowMsg(106);
                }
            if (GUILayout.Button(Get(174), MainButtonStyle))
            {
                EditorApplication.ExecuteMenuItem("VRChat SDK/Show Control Panel");
            }
            DrawHorizontalDottedCenterLine(Color.black, 12, area.width);
            GUILayout.Space(30);

        }else{
            GUILayout.FlexibleSpace();

            GUILayout.Label(Get(175), CenteredStyle, GUILayout.ExpandWidth(true));

            GUILayout.FlexibleSpace();
        }

        GUILayout.EndArea();
    }

    private static string GenerateFilterJsonPath(string scriptPath)
    {
        string fileName = Path.GetFileNameWithoutExtension(scriptPath);
        return $"Packages/com.sabuworks.eauploader/Editor/Resources/Translation/EAUploader/VRCSDK/TransFilter/{fileName}_TransFilter.json";
    }

    private static void DrawVerticalBorder(Rect position)
    {
        EditorGUI.DrawRect(position, Color.gray);
    }

    private static void DrawHorizontalBorder(Rect position)
    {
        EditorGUI.DrawRect(position, Color.gray);
    }

    private static bool IsSameDictionary(Dictionary<string, Texture2D> dict1, Dictionary<string, Texture2D> dict2)
    {
        if (dict1.Count != dict2.Count)
            return false;

        foreach (var key in dict1.Keys)
        {
            if (!dict2.ContainsKey(key) || dict1[key] != dict2[key])
                return false;
        }
        return true;
    }
}
