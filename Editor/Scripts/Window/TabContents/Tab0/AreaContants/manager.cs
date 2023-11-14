using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using System.IO;
using static labels;
using static styles;

public class Manager
{
    private static Dictionary<string, Texture2D> prefabsWithPreview = new Dictionary<string, Texture2D>();
    private static string lastRemovedPrefab;

    public static void Draw(Rect position)
    {
        prefabsWithPreview = CustomPrefabUtility.GetPrefabList();
        GUILayout.BeginArea(position);
        GUILayout.Label(labels.T3, styles.h1Style);

        GUILayout.Label(labels.C3, styles.h2Style);
        if (GUILayout.Button(labels.B3, styles.SubButtonStyle))
        {
            prefabsWithPreview = CustomPrefabUtility.GetPrefabList();
        }

        GUILayout.BeginVertical(styles.listBorderStyle);
        foreach (var kvp in prefabsWithPreview)
        {
            string prefab = kvp.Key;
            Texture2D preview = kvp.Value;

            GUILayout.Space(10); // 項目間のスペースを追加

            GUILayout.BeginHorizontal();
            
            // Preview背景を白に設定
            Rect previewRect = GUILayoutUtility.GetRect(80, 80, GUILayout.Width(80), GUILayout.Height(80));
            EditorGUI.DrawRect(previewRect, Color.white);
            if (preview != null)
            {
                GUI.DrawTexture(previewRect, preview, ScaleMode.ScaleToFit);
            }

            GUILayout.Label(prefab, styles.h2Style);

            if (GUILayout.Button(labels.B4, styles.miniButtonRedStyle, GUILayout.Width(80))) // ボタンのサイズを修正
            {
                if (EditorUtility.DisplayDialog(labels.B4C1, prefab + labels.B4C2, labels.B4B1, labels.B4B2))
                {
                    AssetDatabase.DeleteAsset(prefab);
                    lastRemovedPrefab = prefab;
                    prefabsWithPreview = CustomPrefabUtility.GetPrefabList();
                }
            }
            GUILayout.EndHorizontal();
        }
        GUILayout.EndVertical();

        if (!string.IsNullOrEmpty(lastRemovedPrefab))
        {
            GUILayout.Label(lastRemovedPrefab + "を削除しました");
            lastRemovedPrefab = string.Empty;
        }

        GUILayout.EndArea();
    }
}
