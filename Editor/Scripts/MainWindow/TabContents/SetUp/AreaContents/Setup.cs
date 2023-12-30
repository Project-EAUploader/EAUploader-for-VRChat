using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using static labels;
using static styles;

public class Setup
{
    private static float windowWidth = EditorGUIUtility.currentViewWidth;
    List<string> convertRequests = new List<string>();
    private static Vector2 scrollPosition;
    private static GUIContent content;

    public static void Draw(string[] prefabPaths, string[] prefabNames)
    {
        // EAuploader.unity
        CustomPrefabUtility.EnsureEAUploaderSceneExists();

        if (prefabPaths == null || prefabNames == null)
        {
            // Debug.LogError("Prefab paths or names are null");
            return;
        }

        GUILayout.BeginVertical(noBackgroundStyle);
        GUILayout.Label(Get(110), h1LabelStyle);

        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

        for (int i = 0; i < prefabNames.Length; i++)
        {
            Texture.DrawHorizontalLine(Color.cyan, 12, windowWidth);
            Texture2D preview = null;
            if (CustomPrefabUtility.prefabsWithPreview != null && CustomPrefabUtility.prefabsWithPreview.ContainsKey(prefabPaths[i]))
            {
                preview = CustomPrefabUtility.prefabsWithPreview[prefabPaths[i]];
            }

            string status = CustomPrefabUtility.GetPrefabStatus(prefabPaths[i]);
            GUIContent content;

            // GUIStyleの設定
            GUIStyle editingStyle = new GUIStyle(prefabButtonStyle)
            {
                normal = { textColor = Color.red }
            };
            if (status == "editing")
            {
                content = new GUIContent(prefabNames[i], preview);
                // editing状態の場合は特定のスタイルを使用
                if (GUILayout.Button(content, editingStyle, GUILayout.Height(80)))
                {
                    CustomPrefabUtility.SelectPrefabAndSetupScene(prefabPaths[i]);
                }
            }
            else if (status == "show")
            {
                content = new GUIContent(prefabNames[i], preview);
                if (GUILayout.Button(content, prefabButtonStyle, GUILayout.Height(80)))
                {
                    CustomPrefabUtility.SelectPrefabAndSetupScene(prefabPaths[i]);
                }
            }
            else
            {
                continue;
            }
            if (CustomPrefabUtility.IsVRMPrefab(prefabPaths[i]))
            {
                GUILayout.BeginHorizontal();
                if (GUILayout.Button(Getc("help", 500), HelpButtonStyle))
                {
                    EAUploaderMessageWindow.ShowMsg(102);
                }
                if (GUILayout.Button(Get(146), miniButtonStyle, GUILayout.Width(150)))
                {
                    GameObject selectedPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPaths[i]);
                    if (selectedPrefab != null)
                    {
                        Selection.activeObject = selectedPrefab;
                        EditorApplication.ExecuteMenuItem("VRM0/Duplicate and Convert for VRChat");
                    }
                }
                GUILayout.EndHorizontal();
            }
        }
        Texture.DrawHorizontalLine(Color.cyan, 12, windowWidth);
        GUILayout.Space(30);
        EditorGUILayout.EndScrollView();
        GUILayout.EndVertical();
    }
}

