using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace EAUploader_beta
{
    public class MainWindow : EditorWindow
    {
        [MenuItem("EAUploader_beta/Open EAUploader")]
        public static void ShowWindow()
        {
            MainWindow wnd = GetWindow<MainWindow>();
            wnd.titleContent = new GUIContent("EAUploader_beta");
            wnd.position = new Rect(100, 100, 1280, 640);
            wnd.minSize = new Vector2(960, 640);
        }

        private VisualElement contentRoot = null;
        private string currentTab = "settings";

        public void CreateGUI()
        {
            //initial unityFontDefinition
            rootVisualElement.style.unityFont = Resources.Load<Font>("Fonts/NotoSansJP-Regular");

            // Import UXML
            var visualTree = Resources.Load<VisualTreeAsset>("UI/MainWindow"); 
            visualTree.CloneTree(rootVisualElement);

            contentRoot = rootVisualElement.Q("contentRoot");
            ImportSettings.ShowContent(contentRoot);

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
                    ImportSettings.ShowContent(contentRoot);
                    break;
                case "setup":
                    Setup.ShowContent(contentRoot);
                    break;
                case "upload":
                    Upload.ShowContent(contentRoot);
                    break;
                case "market":
                    Market.ShowContent(contentRoot);
                    break;
                default:
                    break;
            }
        }
    }

}