using UnityEditor;
using UnityEngine;
using static labels;
using static styles;

public class EABuilder
{
    private static readonly float BorderWidth = 2f;
    private static GameObject selectedPrefabInstance = null;
    private static Editor gameObjectEditor;

    public static void Draw(Rect drawArea)
    {
        labels.Initialize();
        styles.Initialize();

        GUILayout.BeginHorizontal();
        float totalWidth = drawArea.width;
        float listWidth = totalWidth * 3 / 10 - BorderWidth;
        float previewWidth = totalWidth * 4 / 10 - 2 * BorderWidth; 
        float toolWidth = totalWidth * 3 / 10 - BorderWidth;

        DrawAvatarList(new Rect(0, 0, listWidth, drawArea.height));
        DrawVerticalBorder(new Rect(listWidth, 0, BorderWidth, drawArea.height));
        DrawAvatarPreview(new Rect(listWidth + BorderWidth, 0, previewWidth, drawArea.height));
        DrawVerticalBorder(new Rect(listWidth + BorderWidth + previewWidth, 0, BorderWidth, drawArea.height));
        DrawUploadTool(new Rect(listWidth + 2 * BorderWidth + previewWidth, 0, toolWidth, drawArea.height));

        GUILayout.EndHorizontal();
    }

    private static void DrawAvatarList(Rect area)
    {
        GUILayout.BeginArea(area);
        GUILayout.Label(C5, h1Style);

        if (GUILayout.Button(B3, SubButtonStyle))
        {
            CustomPrefabUtility.UpdatePrefabList();
        }

        var avatarsWithPreview = CustomPrefabUtility.GetVrchatAvatarList();
        foreach (var avatar in avatarsWithPreview)
        {
            Texture2D preview = avatar.Value; 
            GUIContent content = new GUIContent(avatar.Key, preview);

            if (GUILayout.Button(content, prefabButtonStyle, GUILayout.Height(80)))
            {
                CustomPrefabUtility.SelectPrefabAndSetupScene(avatar.Key);
                selectedPrefabInstance = CustomPrefabUtility.selectedPrefabInstance;
                if (gameObjectEditor != null)
                {
                    UnityEngine.Object.DestroyImmediate(gameObjectEditor);
                }
                gameObjectEditor = Editor.CreateEditor(selectedPrefabInstance);
            }
        }

        GUILayout.EndArea();
    }

    private static void DrawAvatarPreview(Rect area)
    {
        GUILayout.BeginArea(area);

        if (selectedPrefabInstance != null)
        {
            GUILayout.Label(C6 + selectedPrefabInstance.name, h1Style);

            if (gameObjectEditor != null)
            {
                GUIStyle bgColor = new GUIStyle();
                bgColor.normal.background = EditorGUIUtility.whiteTexture;
                float aspectRatio = 1.0f;
                float previewSize = Mathf.Min(area.width, area.height * aspectRatio);
                Rect r = GUILayoutUtility.GetRect(previewSize, previewSize / aspectRatio); 
                gameObjectEditor.OnInteractivePreviewGUI(r, bgColor);
            }
        }
        else
        {
            GUILayout.Label(C6no, h1Style);
        }

        GUILayout.EndArea();
    }

    private static void DrawUploadTool(Rect area)
    {
        GUILayout.BeginArea(area);
        GUILayout.Label("WIP");
        GUILayout.EndArea();
    }

    private static void DrawVerticalBorder(Rect position)
    {
        EditorGUI.DrawRect(position, Color.gray);
    }
}
