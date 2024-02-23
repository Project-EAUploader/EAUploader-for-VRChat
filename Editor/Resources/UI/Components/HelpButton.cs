using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace EAUploader.UI.Components
{
    public class HelpButton : Button
    {
        public new class UxmlFactory : UxmlFactory<HelpButton, UxmlTraits> { }

        public new class UxmlTraits : Button.UxmlTraits
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
            AddToClassList("help-button");

            var icon = new MaterialIcon()
            {
                icon = "help"
            };
            Add(icon);

            var label = new Label(T7e.Get("Help"));
            Add(label);

            RegisterCallback<ClickEvent>(OnButtonClicked);
        }

        private void OnButtonClicked(ClickEvent evt)
        {
            EAUploaderMessageWindow.ShowMsg(msg_id);
        }
    }

    public class EAUploaderMessageWindow : EditorWindow
    {
        private ScrollView scrollView;
        private Label contentLabel;
        private static readonly Vector2 windowSize = new Vector2(600, 300);

        // Call this method to show the window
        public static void ShowMsg(int msgNum)
        {
            var window = GetWindow<EAUploaderMessageWindow>(T7e.Get("Message"));
            window.LoadMsg(msgNum);
            window.minSize = windowSize;
            window.ShowUtility();
        }

        private void LoadMsg(int msgNum)
        {
            string language = LanguageUtility.GetCurrentLanguage();
            string content = Resources.Load<TextAsset>($"Message/{language}/{msgNum}").text;
            if (content != null)
            {
                contentLabel.text = content;
            }
            else
            {
                contentLabel.text = $"Message file not found for language {language} and message number {msgNum}.";
            }
        }

        private void OnEnable()
        {
            var root = rootVisualElement;

            root.styleSheets.Add(Resources.Load<StyleSheet>("UI/styles"));

            // Create a ScrollView
            scrollView = new ScrollView();
            scrollView.style.flexGrow = 1;
            root.Add(scrollView);

            // Create a Label for the content
            contentLabel = new Label();
            contentLabel.style.whiteSpace = WhiteSpace.Normal;
            scrollView.Add(contentLabel);

            // Create a Button to close the window
            var closeButton = new Button(Close);
            closeButton.text = T7e.Get("Close");
            closeButton.style.marginTop = 10;
            root.Add(closeButton);
        }
    }
}