using EAUploader.CustomPrefabUtility;
using EAUploader.UI.Components;
using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace EAUploader.UI.Windows
{
    public class DialogPro : EditorWindow
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
            var eauWindow = GetWindow<EAUploader>(null, focus: false);
            DialogPro wnd = GetWindow<DialogPro>();
            wnd.titleContent = new GUIContent(title);
            wnd.position = new Rect(eauWindow.position.x + eauWindow.position.width / 2 - 200, eauWindow.position.y + eauWindow.position.height / 2 - 100, 400, 200);
            wnd.minSize = new Vector2(400, 200);

            wnd.rootVisualElement.styleSheets.Add(EAUploader.styles);
            wnd.rootVisualElement.styleSheets.Add(EAUploader.tailwind);

            wnd.rootVisualElement.Clear();
            var visualTree = Resources.Load<VisualTreeAsset>("UI/Windows/DialogPro"); 
            visualTree.CloneTree(wnd.rootVisualElement);

            LanguageUtility.Localization(wnd.rootVisualElement);

            wnd.rootVisualElement.Q<Label>("title").text = title;
            wnd.rootVisualElement.Q<Label>("message").text = message;

            var icon = wnd.rootVisualElement.Q<MaterialIcon>("icon");

            var copyButton = wnd.rootVisualElement.Q<Button>("copy");
            var okButton = wnd.rootVisualElement.Q<Button>("ok");

            copyButton.clickable.clicked += () =>
            {
                EditorGUIUtility.systemCopyBuffer = message;
            };

            okButton.clickable.clicked += () =>
            {
                wnd.Close();
            };

            switch (dialogType)
            {
                case DialogType.Info:
                    icon.icon = "info";
                    copyButton.style.display = DisplayStyle.None;
                    break;
                case DialogType.Warning:
                    icon.icon = "warning";
                    icon.AddToClassList("warning");
                    break;
                case DialogType.Error:
                    icon.icon = "error";
                    icon.AddToClassList("danger");
                    break;
                case DialogType.Success:
                    icon.icon = "check_circle";
                    icon.AddToClassList("success");
                    copyButton.style.display = DisplayStyle.None;
                    break;
            }

            //wnd.ShowModal();
            wnd.Show();
        }
    }
}