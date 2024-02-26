using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace EAUploader.UI
{
    public class EAUploader : EditorWindow
    {
        [MenuItem("EAUploader/Open EAUploader")]
        public static void ShowWindow()
        {
            EAUploader wnd = GetWindow<EAUploader>();
            wnd.titleContent = new GUIContent("EAUploader");
            wnd.position = new Rect(100, 100, 1280, 640);
            wnd.minSize = new Vector2(1080, 640);
        }

        private VisualElement contentRoot = null;
        private string currentTab = "settings";

        public void CreateGUI()
        {
            currentTab = "settings";

            rootVisualElement.styleSheets.Add(Resources.Load<StyleSheet>("UI/styles"));
            rootVisualElement.styleSheets.Add(Resources.Load<StyleSheet>("UI/tailwind"));

            // Import UXML
            var visualTree = Resources.Load<VisualTreeAsset>("UI/MainWindow");
            visualTree.CloneTree(rootVisualElement);

            rootVisualElement.schedule.Execute(() =>
            {
                LanguageUtility.Localization(rootVisualElement);
            }).Every(100);

            contentRoot = rootVisualElement.Q("contentRoot");
            ImportSettings.Main.ShowContent(contentRoot);

            rootVisualElement.Q<Button>("settings").EnableInClassList("tab-button__selected", true);

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

            rootVisualElement.Query<Button>().ForEach((b) =>
            {
                b.EnableInClassList("tab-button__selected", false);
            });

            button.EnableInClassList("tab-button__selected", true);

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

            LanguageUtility.Localization(rootVisualElement);
        }
    }

}