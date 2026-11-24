using System.Diagnostics.CodeAnalysis;
using io.github.ykysnk.utils.NonUdon;
using UnityEngine.UIElements;

namespace io.github.ykysnk.utils.Editor.UIElements
{
    /// <summary>
    ///     UI Toolkit field that edits a <see cref="BooleanVector3" /> as three X/Y/Z toggles on one row. It reuses
    ///     Unity's composite vector field USS classes so it lines up with built-in fields, and can be created from UXML
    ///     with the <c>label</c>, <c>x</c>, <c>y</c> and <c>z</c> attributes.
    /// </summary>
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

        /// <summary>
        ///     Builds the field with an empty label and all toggles off, and routes toggle changes into
        ///     <see cref="value" /> so a <c>ChangeEvent</c> is sent for them.
        /// </summary>
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

        /// <summary>
        ///     Text shown in front of the toggles. An empty value removes the label element so the field can be used
        ///     inside a row that already has one.
        /// </summary>
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

        /// <summary>
        ///     State of the X toggle, mirrored into <see cref="value" />.
        /// </summary>
        [SuppressMessage("ReSharper", "InconsistentNaming")]
        public bool x
        {
            get => _x.value;
            set => _x.value = value;
        }

        /// <summary>
        ///     State of the Y toggle, mirrored into <see cref="value" />.
        /// </summary>
        [SuppressMessage("ReSharper", "InconsistentNaming")]
        public bool y
        {
            get => _y.value;
            set => _y.value = value;
        }

        /// <summary>
        ///     State of the Z toggle, mirrored into <see cref="value" />.
        /// </summary>
        [SuppressMessage("ReSharper", "InconsistentNaming")]
        public bool z
        {
            get => _z.value;
            set => _z.value = value;
        }

        /// <summary>
        ///     The edited vector. Setting a different value updates the three toggles and sends a
        ///     <see cref="ChangeEvent{T}" />; setting the current value again does nothing.
        /// </summary>
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

        /// <summary>
        ///     Sets the value and updates the toggles without sending a <see cref="ChangeEvent{T}" />, for pushing data
        ///     that originates from a binding.
        /// </summary>
        /// <param name="newValue">Value to display.</param>
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

        /// <summary>
        ///     Creates the field from UXML.
        /// </summary>
        public new class UxmlFactory : UxmlFactory<BooleanVector3Field, UxmlTraits>
        {
        }

        /// <summary>
        ///     Reads the <c>label</c>, <c>x</c>, <c>y</c> and <c>z</c> UXML attributes of
        ///     <see cref="BooleanVector3Field" />.
        /// </summary>
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

            /// <summary>
            ///     Applies the UXML attributes to the created element.
            /// </summary>
            /// <param name="ve">Element created for this tag.</param>
            /// <param name="bag">Attribute bag holding the UXML values.</param>
            /// <param name="cc">Creation context of the UXML document.</param>
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