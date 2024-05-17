using System;
using System.Collections.Generic;
using UnityEngine.UIElements;

namespace EAUploader.UI.Components
{
    // Derives from BaseField<bool> base class. Represents a container for its input part.
    public class ContentWarningsField : VisualElement
    {
        private static readonly string[] CONTENT_WARNING_TAGS = { "content_sex", "content_adult", "content_violence", "content_gore", "content_horror" };

        private string GetContentWarningName(string tag)
        {
            switch (tag)
            {
                case "content_sex":
                    return T7e.Get("Sexually Suggestive");
                case "content_adult":
                    return T7e.Get("Adult Language and Themes");
                case "content_violence":
                    return T7e.Get("Graphic Violence");
                case "content_gore":
                    return T7e.Get("Excessive Gore");
                case "content_horror":
                    return T7e.Get("Extreme Horror");
                default:
                    return null;
            }
        }

        public new class UxmlFactory : UxmlFactory<ContentWarningsField, UxmlTraits> { }

        public new class UxmlTraits : VisualElement.UxmlTraits
        {
            UxmlStringAttributeDescription _label = new UxmlStringAttributeDescription { name = "label" };

            public override IEnumerable<UxmlChildElementDescription> uxmlChildElementsDescription
            {
                get { yield break; }
            }

            public override void Init(VisualElement ve, IUxmlAttributes bag, CreationContext cc)
            {
                base.Init(ve, bag, cc);
                var checkBox = ve as ContentWarningsField;

                checkBox.label = _label.GetValueFromBag(bag, cc);
            }
        }

        public EventHandler<Tuple<string, bool>> OnToggleTag;

        public bool value;

        public string label
        {
            get { return _label; }
            set
            {
                _label = value;
                var label = this.Q<Label>();
                if (label != null)
                    label.text = value;
            }
        }

        private string _label;

        public List<string> Tags = new List<string>();

        public ContentWarningsField()
        {
            var label = new Label()
            {
                text = _label,
                style =
                {
                    width = 120
                }
            };

            Add(label);

            var group = new VisualElement()
            {
                style =
                {
                    flexDirection = FlexDirection.Row,
                    flexWrap = Wrap.Wrap
                }
            };

            foreach (string tag in CONTENT_WARNING_TAGS)
            {
                var tagButton = new Button()
                {
                    name = $"tag-button:{tag}",
                };
                tagButton.AddToClassList("tag-button");

                var tagToggle = new Toggle();

                var tagLabel = new Label()
                {
                    text = GetContentWarningName(tag)
                };

                tagButton.Add(tagToggle);
                tagButton.Add(tagLabel);

                tagButton.RegisterCallback<ClickEvent>(evt =>
                {
                    tagToggle.value = !tagToggle.value;
                    tagButton.EnableInClassList("checked", tagToggle.value);

                    if (tagToggle.value)
                    {
                        Tags.Add(tag);
                    }
                    else
                    {
                        Tags.Remove(tag);
                    }

                    OnToggleTag?.Invoke(this, new Tuple<string, bool>(tag, tagToggle.value));
                });

                group.Add(tagButton);
            }

            Add(group);
        }

        public void SetTags(List<string> NewTags)
        {
            Tags = new List<string>(NewTags);
            foreach (string tag in CONTENT_WARNING_TAGS)
            {
                var tagButton = this.Q<Button>($"tag-button:{tag}");
                var tagToggle = tagButton.Q<Toggle>();
                tagToggle.value = NewTags.Contains(tag);
                tagButton.EnableInClassList("checked", tagToggle.value);
            }
        }
    }
}