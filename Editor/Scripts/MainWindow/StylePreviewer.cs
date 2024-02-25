using UnityEditor;
using UnityEngine;
using System.Reflection;
using System.Collections.Generic;

/// <summary>
/// 開発時のスタイル参考用
/// </summary>
public class StylePreviewerWindow : EditorWindow
{
    private Vector2 scrollPosition;

    [MenuItem("EAUploader/Show Style Samples(For Dev)")]
    public static void ShowWindow()
    {
        var window = GetWindow<StylePreviewerWindow>();
        window.titleContent = new GUIContent("Style Samples");
        window.Show();
    }

    private void OnGUI()
    {
        // stylesクラスの初期化を確実に行う
        styles.Initialize();

        // 背景色を白色に設定
        GUI.backgroundColor = Color.white;

        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

        // stylesクラスの全ての公開されている静的フィールドを取得
        FieldInfo[] fields = typeof(styles).GetFields(BindingFlags.Public | BindingFlags.Static);
        foreach (var field in fields)
        {
            object fieldValue = field.GetValue(null);
            if (fieldValue is GUIStyle style)
            {
                DrawStyleSample(field.Name, style);
            }
        }

        EditorGUILayout.EndScrollView();
    }

    private void DrawStyleSample(string styleName, GUIStyle style)
    {
        EditorGUILayout.LabelField(styleName, EditorStyles.boldLabel);

        if (style.wordWrap)
        {
            EditorGUILayout.LabelField("wordWrap");
        }

        EditorGUILayout.BeginHorizontal();
        if (style == GUIStyle.none)
        {
            EditorGUILayout.LabelField("No style applied");
        }
        else
        {
            // スタイルに応じて適切なGUIコンポーネントを描画
            if (styleName.Contains("Button"))
            {
                GUILayout.Button("Button", style);
            }
            else if (styleName.Contains("Label"))
            {
                Rect labelRect = GUILayoutUtility.GetRect(new GUIContent("Label"), style);
                EditorGUI.DrawRect(labelRect, Color.white);
                GUI.Label(labelRect, "Label", style);
            }
            else if (styleName.Contains("Popup"))
            {
                EditorGUILayout.Popup(0, new string[] { "Option 1", "Option 2" }, style);
            } 
            else if (styleName.Contains("Box"))
            {
                GUILayout.Box("Box", style);
            }
        }
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space();
    }
}
