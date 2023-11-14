using UnityEditor;
using UnityEngine;
using System.IO;
using static labels;
using static styles;

public class Import
{
    private static string lastImportedFile;
    public static bool isVRMAvailable = EAUploader.HasVRM;

    public static void Draw(Rect position)
    {
        GUILayout.BeginArea(position);
        GUILayout.Label(labels.T2, styles.h1Style);

        GUILayout.Label(labels.C1, styles.h2Style);
        if (GUILayout.Button(labels.B1, styles.MainButtonStyle))
        {
            string path = EditorUtility.OpenFilePanel(labels.B1, "", "prefab");
            if (!string.IsNullOrEmpty(path))
            {
                AssetDatabase.ImportAsset(path);
                lastImportedFile = Path.GetFileName(path);
            }
        }

        GUILayout.Label(labels.C2, styles.h2Style);
        if (GUILayout.Button(labels.B2, styles.MainButtonStyle))
        {
            string path = EditorUtility.OpenFilePanel(labels.B2, "", "unitypackage");
            if (!string.IsNullOrEmpty(path))
            {
                AssetDatabase.ImportPackage(path, true);
                lastImportedFile = Path.GetFileName(path);
            }
        }

        if (isVRMAvailable == true)
        {
            GUILayout.Label(labels.T4, styles.h1Style);
            GUILayout.Label(labels.C4, styles.h2Style);
            if (GUILayout.Button(labels.B5, styles.MainButtonStyle))
            {
                // "VRM0/Import from VRM 0.x" メニューアクションを実行
                EditorApplication.ExecuteMenuItem("VRM0/Import from VRM 0.x");
            }
        }
        else
        {
            GUILayout.Label(labels.C4no, styles.h2Style);
            if(GUILayout.Button(labels.B6, styles.MainButtonStyle))
            {
                Application.OpenURL("https://github.com/vrm-c/UniVRM/releases");
            }
        }

        if (!string.IsNullOrEmpty(lastImportedFile))
            {
                GUILayout.Label(lastImportedFile + "をインポートしました");
                lastImportedFile = string.Empty;
            }
        

        GUILayout.EndArea();
    }
}
