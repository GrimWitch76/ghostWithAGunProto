using System.Collections;
using System.Collections.Generic;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace BT
{
#if UNITY_EDITOR
    public class ExposedIntDisplay : ExposedVariableDisplay
    {
        public override void Initialize()
        {
            MenuCommand = "Add Int";
            PropertyTypeName = "Int";
            PropertyClass = typeof(IntExposedProperty);
            IconColor = new Color(0.95f, 0.77f, 0.06f);
        }

        public override object GetDefaultValue()
        {
            return default(int);
        }

        public override VisualElement SetUpField(ExposedProperty property)
        {
            int propertyValue = property.GetValue() == null ? 0 : (int)property.GetValue();

            IntegerField intField = new IntegerField("Value:")
            {
                value = propertyValue
            };

            intField.RegisterValueChangedCallback(evt =>
            {
                property.SetValue(evt.newValue);
            });

            return intField;
        }
    }
#endif
}
