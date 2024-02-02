#if !EA_ONBUILD
using UnityEngine;
using UnityEditor;
using static labels;
using static styles;
using UnityEngine.UIElements;

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
            EAUploaderMessageWindow.ShowMsg(108);
        }
        GUILayout.EndArea();
        GUILayout.BeginArea(previewRectArea);

        if (gameObjectEditor != null)
        {
            previewSeciton(position, previewRectArea);
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
                    // Debug.Log($"Path = {prefabPath}");
                    CustomPrefabUtility.SetPrefabStatus(prefabPath, "editing");
                    // Debug.Log($"Call SetPrefabStatus {prefabPath} to editing");
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
                    // Debug.Log($"Path = {prefabPath}");
                    CustomPrefabUtility.SetPrefabStatus(prefabPath, "show");
                    // Debug.Log($"Call SetPrefabStatus {prefabPath} to show");
                    CustomPrefabUtility.UpdatePrefabInfo();
                }
            }
            if (GUILayout.Button(Getc("delete", 0), SubButtonStyle, GUILayout.Width(40)))
            {
                Manager.DeletePrefab(prefabPath);
            }
            GUILayout.Space(10);
        }
        GUILayout.EndHorizontal();
        GUILayout.EndArea();
    }

    private static void previewSeciton(Rect position, Rect previewRectArea)
    {
        GUIStyle previewBackgroundStyle = new GUIStyle();
        previewBackgroundStyle.normal.background = EditorGUIUtility.whiteTexture;

        float previewSizeWidth = position.width * previewScale;
        float previewSizeHeight = position.height * previewScale;
        Rect previewRect = new Rect(
            position.x + previewOffset.x,
            position.y + previewOffset.y,
            previewSizeWidth,
            previewSizeHeight
        );

        HandleMouseEvents(position, previewRect, previewRectArea);
        gameObjectEditor.OnInteractivePreviewGUI(previewRect, previewBackgroundStyle);
    }

    private static void HandleMouseEvents(Rect position, Rect previewRect, Rect previewRectArea)
    {
        Event e = Event.current;

        // 拡大縮小
        if (e.type == EventType.ScrollWheel && previewRectArea.Contains(e.mousePosition))
        {
            // スケール変更量
            float scaleDelta = -e.delta.y * 0.05f;
            float newScale = Mathf.Max(previewScale + scaleDelta, 0.1f);
            float oldScale = previewScale;

            previewScale = newScale;
                 
            Vector2 localPreviewCenter = new Vector2(position.width / 2 * newScale, position.height / 2 * newScale);
            Vector2 mousePosition = e.mousePosition;
            Vector2 fixedToCenter = mousePosition - localPreviewCenter;
            Vector2 PreviewCenter = previewRect.center;
            Vector2 relativeMousePosition = mousePosition - PreviewCenter;

            previewOffset = fixedToCenter - relativeMousePosition * (newScale / oldScale);

            e.Use();
        }

        // 移動
        if (e.type == EventType.MouseDrag && e.button == 1 && previewRectArea.Contains(e.mousePosition))
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
#endif