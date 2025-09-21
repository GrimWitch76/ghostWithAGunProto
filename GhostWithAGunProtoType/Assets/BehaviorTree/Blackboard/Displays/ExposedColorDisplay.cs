using System.Collections;
using System.Collections.Generic;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace BT
{
#if UNITY_EDITOR
    public class ExposedColorDisplay : ExposedVariableDisplay
    {
        public override void Initialize()
        {
            MenuCommand = "Add Color";
            PropertyTypeName = "Color";
            PropertyClass = typeof(ColorExposedProperty);
            IconColor = new Color(0.90f, 0.45f, 0.70f);
        }

        public override object GetDefaultValue()
        {
            return default(Color);
        }

        public override VisualElement SetUpField(ExposedProperty property)
        {
            Color propertyValue = property.GetValue() == null ? (Color)GetDefaultValue() : (Color)property.GetValue();

            ColorField colorField = new ColorField("Value:")
            {
                value = propertyValue
            };

            colorField.RegisterValueChangedCallback(evt =>
            {
                property.SetValue(evt.newValue);
            });

            return colorField;
        }
    }
#endif
}
