using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace EAUploader_beta
{
    public class Upload
    {
        public static void ShowContent(VisualElement root)
        {
            var visualTree = Resources.Load<VisualTreeAsset>("UI/TabContents/Upload/Upload");
            visualTree.CloneTree(root);

            SetupButtonHandler(root);
        }

        private static void SetupButtonHandler(VisualElement root)
        {
            var buttons = root.Query<Button>();
            buttons.ForEach(RegisterHandler);
        }

        private static void RegisterHandler(Button button)
        {
            button.RegisterCallback<ClickEvent>(PrintClickMessage);
        }

        private static void PrintClickMessage(ClickEvent evt)
        {
            Button button = evt.currentTarget as Button;
            string buttonName = button.name;

            UnityEngine.Debug.Log($"Button {buttonName} was clicked.");
        }
    }
}
