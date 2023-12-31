using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.IO;
using static labels;
using static styles;

namespace EAUploaderEditors
{
    public static class Editors
    {
        private static Vector2 _scrollPosition;
        private static bool isEditorInfoLoaded = false;
        private static List<EditorRegistration> editorRegistrations;
        private static Dictionary<string, bool> editorFoldouts = new Dictionary<string, bool>();

        public static void Draw(Rect position)
        {
            if (!isEditorInfoLoaded)
            {
                editorRegistrations = new List<EditorRegistration>(EAUploaderEditorManager.GetRegisteredEditors());
                foreach (var editor in editorRegistrations)
                {
                    editorFoldouts[editor.EditorName] = false;
                }
                isEditorInfoLoaded = true;
            }

            GUILayout.BeginArea(position);
            
            GUILayout.Label(Get(150), h1LabelStyle);

            _scrollPosition = GUILayout.BeginScrollView(_scrollPosition, GUILayout.Width(position.width), GUILayout.Height(position.height));

            if (GUILayout.Button(Getc("help", 500), HelpButtonStyle))
            {
                EAUploaderMessageWindow.ShowMsg(103);
            }

            foreach (var editor in editorRegistrations)
            {
                DrawEditorItem(editor, position);
            }

            Texture.DrawHorizontalLine(Color.black, 12, position.width);

            if (GUILayout.Button(Getc("help", 500), HelpButtonStyle))
            {
                EAUploaderMessageWindow.ShowMsg(104);
            }

            if (GUILayout.Button(Getc("open_in_browser", 172), MainButtonStyle))
            {
                Application.OpenURL("https:
            }

            GUILayout.EndScrollView();
            GUILayout.EndArea();
        }

        private static void DrawEditorItem(EditorRegistration editor, Rect position)
        {
            if (GUILayout.Button(editor.EditorName, MainButtonStyle))
            {
                EditorApplication.ExecuteMenuItem(editor.MenuName);
            }

            if (!editorFoldouts[editor.EditorName])
            {
                if (GUILayout.Button(Getc("more_black", 167), FoldoutButtonStyle))
                {
                    editorFoldouts[editor.EditorName] = !editorFoldouts[editor.EditorName];
                }
            }
            else
            {
                if (GUILayout.Button(Getc("arrow_up_black", 167), FoldoutButtonStyle))
                {
                    editorFoldouts[editor.EditorName] = !editorFoldouts[editor.EditorName];
                }
            }
            
            if (editorFoldouts[editor.EditorName])
            {
                GUILayout.Label(Get(168)+ editor.Description, h4LabelStyle);
                GUILayout.Label(Get(169)+ editor.Version, h4LabelStyle);
                GUILayout.Label(Get(170)+ editor.Author, h4LabelStyle);

                if (!string.IsNullOrEmpty(editor.Url) && GUILayout.Button(Get(171), LinkButtonStyle))
                {
                    Application.OpenURL(editor.Url);
                }
            }

            Texture.DrawHorizontalDottedCenterLine(Color.black, 12, position.width);
        }
    }
}

