using System;
using System.Collections.Generic;
using UnityEngine.UIElements;

namespace EAUploader.UI.Components
{
    public class ShadowButton : VisualElement 
    {
        private readonly VisualElement _container;

        public override VisualElement contentContainer => _container;

        public new class UxmlFactory : UxmlFactory<ShadowButton, UxmlTraits> { }

        public new class UxmlTraits : VisualElement.UxmlTraits
        {
            public override IEnumerable<UxmlChildElementDescription> uxmlChildElementsDescription
            {
                get 
                {
                    yield return new UxmlChildElementDescription(typeof(VisualElement));
                }
            }

            UxmlStringAttributeDescription _text = new UxmlStringAttributeDescription { name = "text" };

            public override void Init(VisualElement ve, IUxmlAttributes bag, CreationContext cc)
            {
                base.Init(ve, bag, cc);
                var ShadowButton = ve as ShadowButton; 

                ShadowButton.text = _text.GetValueFromBag(bag, cc);
            }
        }

        public Action clicked;

        public string text;

        public ShadowButton()
        { 
            _container = new VisualElement();
            _container.AddToClassList("none");
            hierarchy.Add(_container);

            RegisterCallback<AttachToPanelEvent>(OnAttachToPanel);
        }

        private void OnAttachToPanel(AttachToPanelEvent evt)
        {
            var shadow = new Shadow()
            {
                shadowDistance = 0,
                shadowCornerRadius = 8,
                shadowOffsetY = 2,
            };
            var button = new Button();

            // Get children and set them up
            var shadowChildren = new List<VisualElement>(contentContainer.Children());

            foreach (var child in shadowChildren)
            {
                button.Add(child);
            }

            var label = new Label(text);
            button.Add(label);

            shadow.Add(button);

            Add(shadow);

            button.clicked += () => clicked?.Invoke();
        }

    }
}
