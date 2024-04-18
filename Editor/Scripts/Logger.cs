using System;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace EAUploader
{
    public class Logger : EditorWindow
    {
        private static StringBuilder _stringBuilder;

        private void OnEnable()
        {
            _stringBuilder = new StringBuilder();
            _stringBuilder.Append($"ÉçÉOàÍóó");
        }

        internal static void OnReceiveLog(string logText, string stackTrace, LogType logType)
        {
            if (logType == LogType.Exception || logType == LogType.Error)
            {
                var eauWindow = GetWindow<UI.EAUploader>(null, focus: false);
                Logger wnd = GetWindow<Logger>();
                wnd.titleContent = new GUIContent(T7e.Get("An error has occurred"));
                wnd.position = new Rect(eauWindow.position.x + eauWindow.position.width / 2 - 200, eauWindow.position.y + eauWindow.position.height / 2 - 100, 400, 200);
                wnd.minSize = new Vector2(400, 200);

            }
            
        }
    }
}