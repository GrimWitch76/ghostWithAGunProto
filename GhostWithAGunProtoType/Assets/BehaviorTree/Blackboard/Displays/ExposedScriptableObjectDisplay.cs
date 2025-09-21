using System.Collections;
using System.Collections.Generic;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace BT
{
#if UNITY_EDITOR
    public class ExposedScriptableObjectDisplay : ExposedVariableDisplay
    {
        public override void Initialize()
        {
            MenuCommand = "Add ScriptableObject";
            PropertyTypeName = "ScriptableObject";
            PropertyClass = typeof(ScriptableObjectExposedProperty);
            IconColor = new Color(0.24f, 0.20f, 0.81f); // Adjust the color as needed
        }

        public override object GetDefaultValue()
        {
            return null; // Default is no ScriptableObject selected
        }

        public override VisualElement SetUpField(ExposedProperty property)
        {
            ScriptableObject propertyValue = property.GetValue() as ScriptableObject;

            ObjectField objectField = new ObjectField("Value:")
            {
                objectType = typeof(ScriptableObject),
                value = propertyValue
            };

            objectField.RegisterValueChangedCallback(evt =>
            {
                property.SetValue(evt.newValue);
            });

            return objectField;
        }
    }
#endif
}
