using UnityEditor;
using UnityEngine;
using System.IO;
using static labels;

public class EAUploaderMessageWindow : EditorWindow
{
    private Vector2 scrollPosition;
    private string content;
    private static readonly Vector2 windowSize = new Vector2(600, 300);


    public static void ShowMsg(int msgNum)
    {
        var window = GetWindow<EAUploaderMessageWindow>(Get(400));
        window.ShowUtility();
        window.LoadMsg(msgNum);
        window.minSize = window.maxSize = windowSize;
    }

    private void LoadMsg(int msgNum)
    {
        string language = LanguageUtility.GetCurrentLanguage();
        string filePath = $"Packages/com.sabuworks.eauploader/Editor/Resources/Message/{language}/{msgNum}.txt";
        if (File.Exists(filePath))
        {
            content = File.ReadAllText(filePath);
        }
        else
        {
            content = $"Message file not found for language {language} and message number {msgNum}.";
        }
    }

    private void OnGUI()
    {
        EditorGUI.DrawRect(new Rect(0, 0, position.width, position.height), Color.white);

        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
        ArticleRenderer.RenderRichTextContent(new Rect(0, 0, position.width, position.height*0.95f), content);
        EditorGUILayout.EndScrollView();

        if (GUILayout.Button(Get(401), styles.SubButtonStyle))
        {
            Close();
        }
    }
}
