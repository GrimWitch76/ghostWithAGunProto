using System.Collections;
using System.Collections.Generic;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace BT
{
    public class ExposedStringDisplay : ExposedVariableDisplay
    {
        public override void Initialize()
        {
            MenuCommand = "Add String";
            PropertyTypeName = "String";
            PropertyClass = typeof(StringExposedProperty);
            IconColor = new Color(0.58f, 0.44f, 0.86f);
        }

        public override object GetDefaultValue()
        {
            return default(string);
        }

        public override VisualElement SetUpField(ExposedProperty property)
        {
            string propertyValue = property.GetValue() == null ? string.Empty : (string)property.GetValue();

            TextField textField = new TextField("Value:")
            {
                value = propertyValue
            };
            textField.RegisterValueChangedCallback(evt =>
            {
                property.SetValue(evt.newValue);
            });

            return textField;
        }
    }
}
