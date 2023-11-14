using UnityEngine;
using UnityEditor;
using EAUploaderEditors;  // Import the namespace to use EAUploaderEditorManager
using System.Collections.Generic;
using static labels;
using static styles;

namespace EAUploaderEditors  // Define the namespace
{
    public static class Editors
    {
        public static void Draw(Rect position)
        {
            GUILayout.Label(H1, h1Style);

            // Load editor info from manage.json
            List<EAUploaderEditorManager.EditorInfo> editorInfos = EAUploaderEditorManager.LoadEditorInfos() ?? new List<EAUploaderEditorManager.EditorInfo>();

            // Iterate through all editors and create UI for each
            foreach (var editorInfo in editorInfos)
            {
                if (GUILayout.Button(editorInfo.display_name, styles.MainButtonStyle))
                {
                    EAUploaderEditorManager.OpenEditor(editorInfo.directory);
                }
                GUILayout.Label(editorInfo.description, styles.h4Style);
            }
        }
    }
}
