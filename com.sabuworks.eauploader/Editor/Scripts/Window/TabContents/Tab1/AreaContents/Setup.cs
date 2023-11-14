using UnityEditor;
using UnityEngine;
using static labels;
using static styles;

public class Setup
{
    public static void Draw(Vector2 _scrollPosition, string[] prefabPaths, string[] prefabNames)
    {
        GUILayout.BeginVertical(noBackgroundStyle);
        GUILayout.Label(C5, h1Style);
        if (GUILayout.Button(B3, SubButtonStyle))
        {
            // Update the prefab list via CustomPrefabUtility
            CustomPrefabUtility.UpdatePrefabList();
        }

        _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition);

        for (int i = 0; i < prefabNames.Length; i++)
        {
            Texture2D preview = AssetPreview.GetAssetPreview(AssetDatabase.LoadAssetAtPath<GameObject>(prefabPaths[i]));

            // Create a GUIContent to hold both image and text
            GUIContent content = new GUIContent(prefabNames[i], preview);

            // Creating a button with prefabButtonStyle.
            if (GUILayout.Button(content, prefabButtonStyle, GUILayout.Height(80)))
            {
                CustomPrefabUtility.SelectPrefabAndSetupScene(prefabPaths[i]);
            }
        }

        EditorGUILayout.EndScrollView();
        GUILayout.EndVertical();
    }
}
