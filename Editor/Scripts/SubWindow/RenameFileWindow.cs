using UnityEditor;
using UnityEngine;
using System.IO;
using static labels;
using static styles;

public class RenameFileWindow : EditorWindow
{
    private string filePath;
    private string newFileName;

    public static void ShowWindow(string path)
    {
        RenameFileWindow window = (RenameFileWindow)EditorWindow.GetWindow(typeof(RenameFileWindow), true, Get(314));
        window.filePath = path;
        window.newFileName = Path.GetFileNameWithoutExtension(path);
        window.minSize = new Vector2(400, 200);
        window.maxSize = window.minSize;
        window.ShowAuxWindow();
    }

    void OnGUI()
    {
        EditorGUI.DrawRect(new Rect(0, 0, position.width, position.height), Color.white);

        GUILayout.Space(20);

        GUILayout.Label(Get(315), styles.NoMargeh2LabelStyle);
        newFileName = EditorGUILayout.TextArea(newFileName, styles.TextAreaStyle, GUILayout.Width(400), GUILayout.Height(50));


        newFileName = newFileName.Replace("\n", "").Replace("\r", "");

        GUILayout.Space(20);

        if (GUILayout.Button(Get(316), styles.SubButtonStyle, GUILayout.Width(200)))
        {
            string directory = Path.GetDirectoryName(filePath);
            string newFilePath = Path.Combine(directory, newFileName + Path.GetExtension(filePath));

            if (!File.Exists(newFilePath))
            {
                AssetDatabase.MoveAsset(filePath, newFilePath);
                AssetDatabase.Refresh();
                Close();
            }
            else
            {
                EditorUtility.DisplayDialog(Get(317), Get(318), Get(319));
            }
        }
    }
}
