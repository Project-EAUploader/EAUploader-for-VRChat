using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace EAUploader_beta.UI.MainWindow
{
    internal class EAUploader_Beta : EditorWindow
    {
        [MenuItem("EAUploader_beta/Open EAUploader")]
        public static void ShowWindow()
        {
            EAUploader_Beta wnd = GetWindow<EAUploader_Beta>();
            wnd.titleContent = new GUIContent("EAUploader_beta");
            wnd.position = new Rect(100, 100, 1280, 640);
            wnd.minSize = new Vector2(960, 640);
        }

        private VisualElement contentRoot = null;
        private string currentTab = "settings";

        public void CreateGUI()
        {
            currentTab = "settings";
            rootVisualElement.styleSheets.Add(Resources.Load<StyleSheet>("UI/styles"));
            rootVisualElement.style.unityFont = Resources.Load<Font>("Fonts/NotoSansJP-Regular");

            // Import UXML
            var visualTree = Resources.Load<VisualTreeAsset>("UI/MainWindow"); 
            visualTree.CloneTree(rootVisualElement);

            contentRoot = rootVisualElement.Q("contentRoot");
            ImportSettings.Main.ShowContent(contentRoot);

            // Call the event handler
            SetupButtonHandler();
        }

        private void SetupButtonHandler()
        {
            VisualElement tabs = rootVisualElement.Q("tabs");
            var buttons = tabs.Query<Button>();
            buttons.ForEach(RegisterHandler);
        }

        private void RegisterHandler(Button button)
        {
            button.RegisterCallback<ClickEvent>(ChangeContent);
        }

        private void ChangeContent(ClickEvent evt)
        {
            Button button = evt.currentTarget as Button;
            string buttonName = button.name;

            if (currentTab == buttonName)
            {
                return;
            }

            currentTab = buttonName;

            contentRoot.Clear();

            switch (buttonName)
            {
                case "settings":
                    ImportSettings.Main.ShowContent(contentRoot);
                    break;
                case "setup":
                    Setup.Main.ShowContent(contentRoot);
                    break;
                case "upload":
                    Upload.Main.ShowContent(contentRoot);
                    break;
                case "market":
                    Market.Main.ShowContent(contentRoot);
                    break;
                default:
                    break;
            }
        }
    }

}