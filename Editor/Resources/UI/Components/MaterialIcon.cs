using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace EAUploader.UI.Components
{
    public class MaterialIcon : Label
    {
        public new class UxmlFactory : UxmlFactory<MaterialIcon, UxmlTraits> { };

        public new class UxmlTraits : VisualElement.UxmlTraits
        {
            private readonly UxmlStringAttributeDescription icon = new() { name = "icon", defaultValue = "" };

            public override void Init(VisualElement ve, IUxmlAttributes bag, CreationContext cc)
            {
                base.Init(ve, bag, cc);
                MaterialIcon target = ve as MaterialIcon;

                target.icon = icon.GetValueFromBag(bag, cc);

                target.AddToClassList("material-icons");

                if (string.IsNullOrEmpty(target.icon))
                {
                    target.text = "";
                }
                else
                {
                    target.text = "\\u" + GetCodepointByIconName(target.icon);
                }
            }
        }

        private static Dictionary<string, string> iconNameToCodepoint;
        private string _icon;

        public string icon
        {
            get => _icon;
            set
            {
                _icon = value;
                if (string.IsNullOrEmpty(_icon))
                {
                    text = "";
                }
                else
                {
                    text = "\\u" + GetCodepointByIconName(_icon);
                }
            }
        }

        public MaterialIcon()
        {
            AddToClassList("material-icons");

            if (string.IsNullOrEmpty(icon))
            {
                text = "";
            }
            else
            {
                text = "\\u" + GetCodepointByIconName(icon);
            }
        }

        public MaterialIcon(string icon) : this()
        {
            Init(icon);
        }

        public void Init(string icon)
        {
            this.icon = icon;
        }

        private static string GetCodepointByIconName(string iconName)
        {
            if (iconNameToCodepoint == null
                || iconNameToCodepoint.Count == 0)
            {
                iconNameToCodepoint = new Dictionary<string, string>();
                string codepointsString = Resources.Load<TextAsset>("UI/Components/codepoints").text;
                string[] codepointLines = codepointsString.Split("\n");
                foreach (string codepointLine in codepointLines)
                {
                    string[] iconNameAndCodepoint = codepointLine.Trim().Split(" ");
                    if (iconNameAndCodepoint.Length == 2)
                    {
                        string iconNameLocal = iconNameAndCodepoint[0];
                        string codepointLocal = iconNameAndCodepoint[1];
                        iconNameToCodepoint[iconNameLocal] = codepointLocal;
                    }
                }
            }

            if (iconNameToCodepoint.TryGetValue(iconName, out string codepoint))
            {
                return codepoint;
            }

            Debug.LogError($"Codepoint not found for icon name {iconName}");
            return iconName;
        }
    }
}