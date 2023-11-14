using UnityEditor;
using UnityEngine;
using System.IO;
using static styles;
using static labels;
using EAUploaderEditors;

public class SetUpTabDrawer
{
    public static void Draw(Rect position, Vector2 _scrollPosition, string[] prefabPaths, string[] prefabNames, string selectedPrefabName, GameObject selectedPrefabInstance)
    {
        EditorGUI.DrawRect(new Rect(0, 0, position.width, position.height), Color.white);

        GUILayout.BeginHorizontal();

        // Setup
        GUILayout.BeginArea(new Rect(0, 0, position.width * 0.3f, position.height));
        Setup.Draw(_scrollPosition, prefabPaths, prefabNames);
        GUILayout.EndArea();

        // Boundary between Setup and Preview
        EditorGUI.DrawRect(new Rect(position.width * 0.3f - 1, 0, 2, position.height), Color.black); // 2 pixel wide boundary for clarity

        // Preview
        GUILayout.BeginArea(new Rect(position.width * 0.3f + 1, 0, position.width * 0.4f - 2, position.height));
        // Define the Rect for drawing the preview and call Preview.Draw()
        Rect previewRect = new Rect(0, 0, position.width * 0.4f - 2, position.height); // adjusted width to account for boundary
        Preview.Draw(CustomPrefabUtility.selectedPrefabInstance, previewRect);
        GUILayout.EndArea();

        // Boundary between Preview and Editor
        EditorGUI.DrawRect(new Rect(position.width * 0.7f - 1, 0, 2, position.height), Color.black); // 2 pixel wide boundary for clarity

        // Editor
        GUILayout.BeginArea(new Rect(position.width * 0.7f + 1, 0, position.width * 0.3f - 2, position.height));
        EAUploaderEditors.Editors.Draw(new Rect(0, 0, position.width * 0.3f - 2, position.height)); // adjusted width to account for boundary
        GUILayout.EndArea();

        GUILayout.EndHorizontal();
    }
}
