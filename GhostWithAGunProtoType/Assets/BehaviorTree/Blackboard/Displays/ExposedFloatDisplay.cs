using System.Collections;
using System.Collections.Generic;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace BT
{
#if UNITY_EDITOR
    public class ExposedFloatDisplay : ExposedVariableDisplay
    {
        public override void Initialize()
        {
            MenuCommand = "Add Float";
            PropertyTypeName = "Float";
            PropertyClass = typeof(FloatExposedProperty);
            IconColor = new Color(0.20f, 0.60f, 0.86f);
        }

        public override object GetDefaultValue()
        {
            return default(float);
        }

        public override VisualElement SetUpField(ExposedProperty property)
        {
            float propertyValue = property.GetValue() == null ? 0f : (float)property.GetValue();

            FloatField floatField = new FloatField("Value:")
            {
                value = propertyValue
            };

            floatField.RegisterValueChangedCallback(evt =>
            {
                property.SetValue(evt.newValue);
            });

            return floatField;
        }
    }
#endif
}
