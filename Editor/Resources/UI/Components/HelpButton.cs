using System.Collections.Generic;
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

                helpButton.msg_id =Å@_msgId.GetValueFromBag(bag, cc);
            }
        }

        private int msg_id;
        private readonly Button _button;

        public HelpButton()
        {
            _button = new Button()
            {
                style =
                {
                    flexDirection = FlexDirection.Row,
                    paddingRight = 4,
                    paddingLeft = 4,
                }
            };
            var icon = new MaterialIcon()
            {
                icon = "help"
            };
            _button.Add(icon);
            var label = new Label(Translate.Get("Help"));
            _button.Add(label);
            Add(_button);

            _button.RegisterCallback<ClickEvent>(OnButtonClicked);
        }

        private void OnButtonClicked(ClickEvent evt)
        {
            EAUploaderMessageWindow.ShowMsg(msg_id);
        }

    }
}