using EAUploader.UI.Components;
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
        public static StyleSheet styles;
        public static StyleSheet tailwind;

        public void CreateGUI()
        {
            try
            {
                styles = Resources.Load<StyleSheet>("UI/styles");
                tailwind = Resources.Load<StyleSheet>("UI/tailwind");
                currentTab = "settings";
                rootVisualElement.styleSheets.Add(styles);
                rootVisualElement.styleSheets.Add(tailwind);
                rootVisualElement.style.flexDirection = FlexDirection.ColumnReverse;

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
            catch (System.Exception)
            {
                if (EditorUtility.DisplayDialog("EAUploader Error", T7e.Get("Restart EAUploader because there is a problem with EAUploader; if restarting EAUploader does not solve the problem, delete EAUploader from VCC and add EAUploader again."), T7e.Get("Restart"), T7e.Get("Cancel")))
                {
                    AssetDatabase.ImportAsset("Packages/tech.uslog.eauploader", ImportAssetOptions.ImportRecursive);
                }
            }
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
                default:
                    break;
            }

            LanguageUtility.Localization(rootVisualElement);
        }

        private void OnGUI()
        {
            rootVisualElement.MarkDirtyRepaint();
        }
    }
}