

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace EAUploader.UI.Components
{
    public class Link : Label
    {
        public new class UxmlFactory : UxmlFactory<Link, UxmlTraits> { }

        public new class UxmlTraits : Label.UxmlTraits
        {
            UxmlStringAttributeDescription _href = new UxmlStringAttributeDescription { name = "href" };

            public override IEnumerable<UxmlChildElementDescription> uxmlChildElementsDescription
            {
                get { yield break; }
            }

            public override void Init(VisualElement ve, IUxmlAttributes bag, CreationContext cc)
            {
                base.Init(ve, bag, cc);
                var link = ve as Link;

                link.href = _href.GetValueFromBag(bag, cc);
            }
        }

        public string href;

        public Link()
        {

        }

        public Link(string text) : base(text)
        {
            RegisterCallback<PointerDownEvent>(HyperlinkOnPointerUp);
        }

        void HyperlinkOnPointerUp(PointerDownEvent evt)
        {
            Application.OpenURL(href);
        }
    }
}
