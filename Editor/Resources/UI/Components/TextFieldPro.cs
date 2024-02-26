using System.Collections.Generic;
using UnityEngine.UIElements;

namespace EAUploader.UI.Components
{
    public class TextFieldPro : TextField
    {
        public new class UxmlFactory : UxmlFactory<TextFieldPro, UxmlTraits> { }

        public new class UxmlTraits : TextField.UxmlTraits
        {
            private readonly UxmlStringAttributeDescription placeholder = new UxmlStringAttributeDescription { name = "placeholder" };
            private readonly UxmlBoolAttributeDescription required = new UxmlBoolAttributeDescription { name = "required" };

            public override IEnumerable<UxmlChildElementDescription> uxmlChildElementsDescription
            {
                get { yield break; }
            }

            public override void Init(VisualElement ve, IUxmlAttributes bag, CreationContext cc)
            {
                base.Init(ve, bag, cc);
                var textField = (TextFieldPro)ve;
                textField.placeholder = placeholder.GetValueFromBag(bag, cc);
                textField.required = required.GetValueFromBag(bag, cc);
            }
        }

        public string placeholder
        {
            get => _placeholder;
            set
            {
                _placeholder = value;
                if (string.IsNullOrWhiteSpace(_placeholder)) return;
                if (!string.IsNullOrEmpty(text)) return;
                SetValueWithoutNotify(_placeholder);
                AddToClassList(ussClassName + "__placeholder");
            }
        }

        private string _placeholder;
        private static readonly string PlaceholderClass = TextField.ussClassName + "__placeholder";
        public bool required
        {
            get => _required;
            set
            {
                _required = value;
            }
        }
        private bool _required;
        private bool loading;

        public bool Loading
        {
            get => loading;
            set
            {
                loading = value;
                SetEnabled(!loading);
                if (loading)
                {
                    text = "Loading...";
                }
                else
                {
                    if (text == "Loading...")
                    {
                        text = "";
                    }
                    FocusOut();
                }
                EnableInClassList(ussClassName + "__loading", loading);
            }
        }

        public TextFieldPro()
        {
            RegisterCallback<FocusOutEvent>(evt => FocusOut());
            RegisterCallback<FocusInEvent>(evt => FocusIn());
            this.RegisterValueChangedCallback(ValueChanged);
        }

        public void Reset()
        {
            if (string.IsNullOrEmpty(text))
            {
                FocusOut();
                return;
            };
            RemoveFromClassList(PlaceholderClass);
        }

        private void ValueChanged(ChangeEvent<string> evt)
        {
            if (!_required) return;
            this.Q<TextInputBase>().EnableInClassList("border-red", string.IsNullOrWhiteSpace(evt.newValue));
        }

        public string GetValue()
        {
            if (_required && IsPlaceholder())
            {
                this.Q<TextInputBase>().EnableInClassList("border-red", true);
                return null;
            } else if (IsPlaceholder())
            {
                return null;
            }
            return text;
        }

        private void FocusOut()
        {
            if (string.IsNullOrWhiteSpace(_placeholder)) return;
            if (!string.IsNullOrEmpty(text)) return;
            SetValueWithoutNotify(_placeholder);
            AddToClassList(ussClassName + "__placeholder");
        }

        private void FocusIn()
        {
            if (string.IsNullOrWhiteSpace(_placeholder)) return;
            if (!this.ClassListContains(ussClassName + "__placeholder")) return;
            this.value = string.Empty;
            this.RemoveFromClassList(ussClassName + "__placeholder");
        }

        public bool IsPlaceholder()
        {
            var placeholderClass = TextField.ussClassName + "__placeholder";
            return ClassListContains(placeholderClass);
        }
    }
}