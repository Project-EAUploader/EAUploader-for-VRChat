using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace EAUploader.UI.Components
{
    public class HelpButton : VisualElement
    {
        public new class UxmlFactory : UxmlFactory<HelpButton, UxmlTraits> { }

        public new class UxmlTraits : VisualElement.UxmlTraits
        {
            UxmlIntAttributeDescription _msgId = new UxmlIntAttributeDescription { name = "msg-id" };

            public override IEnumerable<UxmlChildElementDescription> uxmlChildElementsDescription
            {
                get { yield break; }
            }

            public override void Init(VisualElement ve, IUxmlAttributes bag, CreationContext cc)
            {
                base.Init(ve, bag, cc);
                var helpButton = ve as HelpButton;

                helpButton.msg_id = _msgId.GetValueFromBag(bag, cc);
            }
        }

        private int msg_id;

        public HelpButton()
        {
            var shadow = new Shadow()
            {
                shadowDistance = 0,
                shadowCornerRadius = 8,
                shadowOffsetY = 2,
            };
            var button = new Button();
            button.AddToClassList("help-button");

            var icon = new MaterialIcon()
            {
                icon = "help"
            };
            button.Add(icon);

            var label = new Label(T7e.Get("Help"));
            button.Add(label);

            button.RegisterCallback<ClickEvent>(OnButtonClicked);

            shadow.Add(button);

            Add(shadow);
        }

        private void OnButtonClicked(ClickEvent evt)
        {
            EAUploaderMessageWindow.ShowMsg(msg_id);
        }
    }

    public class EAUploaderMessageWindow : EditorWindow
    {
        private ScrollView scrollView;
        private static readonly Vector2 windowSize = new Vector2(600, 300);

        // Call this method to show the window
        public static void ShowMsg(int msgNum)
        {
            var window = GetWindow<EAUploaderMessageWindow>(T7e.Get("Message"));
            window.scrollView.Clear();
            window.LoadMsg(msgNum);
            window.minSize = windowSize;
            window.ShowUtility();
        }

        private void LoadMsg(int msgNum)
        {
            string language = LanguageUtility.GetCurrentLanguage();
            // get content path

            string contentPath = $"Packages/tech.uslog.eauploader/Editor/Resources/Message/{language}/{msgNum}.txt";
            var article = new ArticleRenderer(contentPath);

            scrollView.Add(article);
        }

        private void OnEnable()
        {
            var root = rootVisualElement;

            root.styleSheets.Add(Resources.Load<StyleSheet>("UI/styles"));

            // Create a ScrollView
            scrollView = new ScrollView();
            scrollView.style.flexGrow = 1;
            scrollView.style.flexShrink = 1;
            root.Add(scrollView);

            // Create a Button to close the window
            var closeButton = new ShadowButton()
            {
                name = "close_button",
            };
            closeButton.clicked += Close;
            closeButton.text = T7e.Get("Close");
            root.Add(closeButton);
        }
    }
}
