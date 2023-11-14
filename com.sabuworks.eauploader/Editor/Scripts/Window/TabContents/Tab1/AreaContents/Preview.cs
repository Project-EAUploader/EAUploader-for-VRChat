using UnityEngine;
using UnityEditor;
using static labels;
using static styles;

public static class Preview
{
    private static Editor gameObjectEditor;
    private static GameObject currentPreviewObject;

    public static void Draw(GameObject selectedPrefabInstance, Rect position)
    {
        // Null check and display relevant information
        if (selectedPrefabInstance == null)
        {
            GUILayout.Label(C6no, h1Style);
            return;
        }

        // If the selected object has changed, create a new Editor instance
        if (currentPreviewObject != selectedPrefabInstance)
        {
            currentPreviewObject = selectedPrefabInstance;
            gameObjectEditor = Editor.CreateEditor(selectedPrefabInstance);
        }

        // Display prefab name
        GUILayout.Label(C6 + selectedPrefabInstance.name, h1Style);

        // Display model height
        Renderer[] renderers = selectedPrefabInstance.GetComponentsInChildren<Renderer>();
        if (renderers.Length > 0)
        {
            float minHeight = float.MaxValue;
            float maxHeight = float.MinValue;
            
            foreach (Renderer renderer in renderers)
            {
                minHeight = Mathf.Min(minHeight, renderer.bounds.min.y);
                maxHeight = Mathf.Max(maxHeight, renderer.bounds.max.y);
            }

            float height = maxHeight - minHeight;
            GUILayout.Label("Model Height: " + height.ToString("F2") + " units", h2Style);
        }
        else
        {
            GUILayout.Label("Unable to calculate model height", h2Style);
        }

        // Display prefab preview
        if (gameObjectEditor != null)
        {
            GUIStyle bgColor = new GUIStyle();
            bgColor.normal.background = EditorGUIUtility.whiteTexture;

            // Ensure that the preview takes up all available space while respecting the aspect ratio
            float aspectRatio = 1.0f; // Replace this with the aspect ratio of your 3D object
            float previewSize = Mathf.Min(position.width, position.height * aspectRatio);
            Rect r = GUILayoutUtility.GetRect(previewSize, previewSize / aspectRatio); 
            gameObjectEditor.OnInteractivePreviewGUI(r, bgColor);
        }
    }
}
