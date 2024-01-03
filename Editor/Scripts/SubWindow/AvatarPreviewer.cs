using UnityEditor;
using UnityEngine;

public class AvatarPreviewerWindow : EditorWindow
{
    private Texture2D previewTexture;
    private string prefabName;
    private const float windowHeight = 480; // ウィンドウの横幅を固定

    public static void ShowWindow(string prefabName)
    {
        AvatarPreviewerWindow window = (AvatarPreviewerWindow)GetWindow(typeof(AvatarPreviewerWindow), true, "Avatar Previewer");
        window.prefabName = prefabName;
        window.LoadPreviewTexture();
        if (window.previewTexture != null)
        {
            float aspectRatio = (float)window.previewTexture.height / window.previewTexture.width;
            float windowWidth = windowHeight * aspectRatio;

            window.minSize = new Vector2(windowWidth, windowHeight);
            window.maxSize = window.minSize;
        }
        window.ShowAuxWindow();
    }

    private void OnGUI()
    {
        if (previewTexture != null)
        {
            GUI.DrawTexture(new Rect(0, 0, position.width, position.height), previewTexture, ScaleMode.ScaleToFit);
        }
        else
        {
            EditorGUILayout.LabelField("No preview available");
        }
    }

    private void LoadPreviewTexture()
    {
        string previewPath = $"Assets/EAUploader/PrefabPreviews/{prefabName}.png";
        previewTexture = AssetDatabase.LoadAssetAtPath<Texture2D>(previewPath);

        if (previewTexture == null)
        {
            Debug.LogError("Failed to load preview texture for prefab: " + prefabName);
        }
    }
}