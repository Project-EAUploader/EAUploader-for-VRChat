using UnityEngine;
using UnityEditor;
using static labels;
using static styles;

public static class Preview
{
    private static Editor gameObjectEditor;
    private static GameObject currentPreviewObject;
    private static float previewScale = 1.0f;
    private static Vector2 previewOffset = Vector2.zero;

    public static void Draw(GameObject selectedPrefabInstance, Rect position)
    {
        var topRect = new Rect(position.x, position.y, position.width, position.height * 0.1f);
        var previewRectArea = new Rect(position.x, position.y + position.height * 0.1f, position.width, position.height * 0.8f);
        var bottomRect = new Rect(position.x, position.y + position.height * 0.9f, position.width, position.height * 0.1f);

        GUILayout.BeginArea(topRect);

        if (selectedPrefabInstance == null)
        {
            GUILayout.FlexibleSpace();


            GUILayout.Label(Get(113), CenteredStyle, GUILayout.ExpandWidth(true));

            GUILayout.FlexibleSpace();
            GUILayout.EndArea();
            return;
        }

        if (currentPreviewObject != selectedPrefabInstance)
        {
            currentPreviewObject = selectedPrefabInstance;
            gameObjectEditor = Editor.CreateEditor(selectedPrefabInstance);
            string prefabPath = AssetDatabase.GetAssetPath(selectedPrefabInstance);
        }

        GUILayout.Label(Get(112) + selectedPrefabInstance.name, h1LabelStyle);

        Renderer[] renderers = selectedPrefabInstance.GetComponentsInChildren<Renderer>();
        if (renderers.Length > 0)
        {
            float avatarHeight = CustomPrefabUtility.GetAvatarHeight(selectedPrefabInstance);
            if (avatarHeight != 0f)
            {
                string heightStr = $"{avatarHeight:F2}m";
                GUILayout.Label($"{Get(164)}{heightStr}", h2LabelStyle);
            }
            else
            {
                GUILayout.Label(Get(166), h2LabelStyle);
            }
           
        }
        else
        {
            GUILayout.Label(Get(166), h2LabelStyle);
        }
        if (GUILayout.Button(Getc("help", 500), HelpButtonStyle))
        {
            EAUploaderMessageWindow.ShowMsg(107);
        }
        GUILayout.EndArea();
        GUILayout.BeginArea(previewRectArea);

        if (gameObjectEditor != null)
        {
            GUIStyle previewBackgroundStyle = new GUIStyle();
            previewBackgroundStyle.normal.background = EditorGUIUtility.whiteTexture;

            float aspectRatio = 1.0f;
            float previewSizeWidth = position.width * previewScale;
            float previewSizeHeight = position.height * aspectRatio * previewScale;
            Rect previewRect = new Rect(
                position.x + previewOffset.x, 
                position.y + previewOffset.y, 
                previewSizeWidth, 
                previewSizeHeight
            );

            HandleMouseEvents(previewRect);

            gameObjectEditor.OnInteractivePreviewGUI(previewRect, previewBackgroundStyle);
        }
        GUILayout.EndArea();

        GUILayout.BeginArea(bottomRect);
        GUILayout.BeginHorizontal();
        if (gameObjectEditor != null)
        {
            GUILayout.Space(10);
            if (GUILayout.Button(Getc("reset_black", 179), noBackgroundStyle, GUILayout.Height(40)))
            {
                ResetPreview();
            }
            GameObject prefabAsset = PrefabUtility.GetCorrespondingObjectFromSource(selectedPrefabInstance);
            string prefabPath = AssetDatabase.GetAssetPath(prefabAsset);
            string prefabstatus = CustomPrefabUtility.GetPrefabStatus(prefabPath);
            if (prefabstatus == "show")
            {
                if (GUILayout.Button(Getc("create", 147), SubButtonStyle))
                {
                    RenameFileWindow.ShowWindow(prefabPath);
                }
                if (GUILayout.Button(Getc("pin", 176), SubButtonStyle))
                {
                    CustomPrefabUtility.SetPrefabStatus(prefabPath, "editing");
                    CustomPrefabUtility.UpdatePrefabInfo();
                }
            }
            else
            {
                if (GUILayout.Button(Getc("create", 147), SubButtonStyle))
                {
                    RenameFileWindow.ShowWindow(prefabPath);
                }
                if (GUILayout.Button(Getc("pin", 177), SubButtonStyle))
                {
                    CustomPrefabUtility.SetPrefabStatus(prefabPath, "show");
                    CustomPrefabUtility.UpdatePrefabInfo();
                }
            }
            GUILayout.Space(10);
        }
        GUILayout.EndHorizontal();
        GUILayout.EndArea();
    }

    private static void HandleMouseEvents(Rect previewRectArea)
    {
        Event e = Event.current;

        if (previewRectArea.Contains(e.mousePosition) && e.type == EventType.ScrollWheel)
        {
            float scaleDelta = -e.delta.y * 0.05f;
            float newScale = Mathf.Max(previewScale + scaleDelta, 0.1f);

            Vector2 oldCenter = new Vector2(
                previewRectArea.x + previewOffset.x + (previewRectArea.width * previewScale / 2),
                previewRectArea.y + previewOffset.y + (previewRectArea.height * previewScale / 2)
            );

            previewScale = newScale;

            Vector2 newCenter = new Vector2(
                previewRectArea.x + previewOffset.x + (previewRectArea.width * newScale / 2),
                previewRectArea.y + previewOffset.y + (previewRectArea.height * newScale / 2)
            );

            previewOffset += (oldCenter - newCenter);

            e.Use();
        }

        if (e.type == EventType.MouseDrag && e.button == 0 && previewRectArea.Contains(e.mousePosition))
        {
            previewOffset += e.delta;
            e.Use();
        }
    }

    private static void ResetPreview()
    {
        previewScale = 1.0f;
        previewOffset = Vector2.zero;
    }
}
