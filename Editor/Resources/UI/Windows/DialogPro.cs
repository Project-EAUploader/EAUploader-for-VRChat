using EAUploader.UI.Components;
using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace EAUploader.UI.Windows
{
    public class DialogPro
    {
        public enum DialogType
        {
            Info,
            Warning,
            Error,
            Success
        }

        public static void Show(DialogType dialogType, string title, string message)
        {
            if (dialogType == DialogType.Success)
            {
                EditorUtility.DisplayDialog(title, message, "OK");
            }
            else 
            {
                if (EditorUtility.DisplayDialogComplex(title, message, "Copy", "OK", "") == 0)
                {
                    EditorGUIUtility.systemCopyBuffer = message;
                }
            }
        }
    }
}