using System.Diagnostics.CodeAnalysis;
using io.github.ykysnk.utils.NonUdon;
using UnityEngine.UIElements;

namespace io.github.ykysnk.utils.Editor.UIElements
{
    public class BooleanVector3Field : BindableElement, INotifyValueChanged<BooleanVector3>
    {
        private readonly VisualElement _input = new();

        private readonly Label _label = new();

        private readonly Toggle _x = new()
        {
            style =
            {
                marginLeft = 0,
                paddingLeft = 0
            },
            name = "x-toggle",
            label = "X",
            bindingPath = nameof(BooleanVector3.x)
        };

        private readonly Toggle _y = new()
        {
            style =
            {
                marginLeft = 0,
                paddingLeft = 0
            },
            name = "y-toggle",
            label = "Y",
            bindingPath = nameof(BooleanVector3.y)
        };

        private readonly Toggle _z = new()
        {
            style =
            {
                marginLeft = 0,
                paddingLeft = 0
            },
            name = "z-toggle",
            label = "Z",
            bindingPath = nameof(BooleanVector3.z)
        };

        private BooleanVector3 _value;

        public BooleanVector3Field()
        {
            AddToClassList("unity-base-field");
            AddToClassList("unity-composite-field");
            AddToClassList("unity-vector3-field");
            _label.AddToClassList("unity-label");
            _label.AddToClassList("unity-base-field__label");
            _label.AddToClassList("unity-composite-field__label");
            _label.AddToClassList("unity-vector3-field__label");
            _input.AddToClassList("unity-base-field__input");
            _input.AddToClassList("unity-composite-field__input");
            _input.AddToClassList("unity-vector3-field__input");
            _x.AddToClassList("unity-base-text-field");
            _x.AddToClassList("unity-composite-field__field");
            _x.AddToClassList("unity-composite-field__field--first");
            _y.AddToClassList("unity-base-text-field");
            _y.AddToClassList("unity-composite-field__field");
            _z.AddToClassList("unity-base-text-field");
            _z.AddToClassList("unity-composite-field__field");
            Add(_label);
            Add(_input);
            _input.Add(_x);
            _input.Add(_y);
            _input.Add(_z);

            _x.RegisterValueChangedCallback(_ => UpdateValueFromUI());
            _y.RegisterValueChangedCallback(_ => UpdateValueFromUI());
            _z.RegisterValueChangedCallback(_ => UpdateValueFromUI());
        }

        [SuppressMessage("ReSharper", "InconsistentNaming")]
        public string label
        {
            get => _label.text;
            set
            {
                _label.text = value;

                if (string.IsNullOrEmpty(value))
                    Remove(_label);
                else if (!Contains(_label))
                    Insert(0, _label);
            }
        }

        [SuppressMessage("ReSharper", "InconsistentNaming")]
        public bool x
        {
            get => _x.value;
            set => _x.value = value;
        }

        [SuppressMessage("ReSharper", "InconsistentNaming")]
        public bool y
        {
            get => _y.value;
            set => _y.value = value;
        }

        [SuppressMessage("ReSharper", "InconsistentNaming")]
        public bool z
        {
            get => _z.value;
            set => _z.value = value;
        }

        public BooleanVector3 value
        {
            get => _value;
            set
            {
                if (_value.Equals(value)) return;
                var previous = _value;
                _value = value;

                _x.SetValueWithoutNotify(value.x);
                _y.SetValueWithoutNotify(value.y);
                _z.SetValueWithoutNotify(value.z);

                using var evt = ChangeEvent<BooleanVector3>.GetPooled(previous, value);
                evt.target = this;
                SendEvent(evt);
            }
        }

        public void SetValueWithoutNotify(BooleanVector3 newValue)
        {
            _value = newValue;
            _x.SetValueWithoutNotify(newValue.x);
            _y.SetValueWithoutNotify(newValue.y);
            _z.SetValueWithoutNotify(newValue.z);
        }

        private void UpdateValueFromUI()
        {
            var newValue = new BooleanVector3
            {
                x = _x.value,
                y = _y.value,
                z = _z.value
            };

            if (!newValue.Equals(_value))
                value = newValue;
        }

        public new class UxmlFactory : UxmlFactory<BooleanVector3Field, UxmlTraits>
        {
        }

        public new class UxmlTraits : BindableElement.UxmlTraits
        {
            private readonly UxmlStringAttributeDescription _label = new()
            {
                name = "label"
            };

            private readonly UxmlBoolAttributeDescription _x = new()
            {
                name = "x"
            };

            private readonly UxmlBoolAttributeDescription _y = new()
            {
                name = "y"
            };

            private readonly UxmlBoolAttributeDescription _z = new()
            {
                name = "z"
            };

            public override void Init(VisualElement ve, IUxmlAttributes bag, CreationContext cc)
            {
                base.Init(ve, bag, cc);

                var field = (BooleanVector3Field)ve;
                field.label = _label.GetValueFromBag(bag, cc);
                field.x = _x.GetValueFromBag(bag, cc);
                field.y = _y.GetValueFromBag(bag, cc);
                field.z = _z.GetValueFromBag(bag, cc);
            }
        }
    }
}