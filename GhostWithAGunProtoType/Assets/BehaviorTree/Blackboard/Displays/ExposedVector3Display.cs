using System.Collections;
using System.Collections.Generic;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace BT
{
#if UNITY_EDITOR
    public class ExposedVector3Display : ExposedVariableDisplay
    {
        public override void Initialize()
        {
            MenuCommand = "Add Vector3";
            PropertyTypeName = "Vector3";
            PropertyClass = typeof(Vector3ExposedProperty);
            IconColor = new Color(0.41f, 0.67f, 0.63f);
        }

        public override object GetDefaultValue()
        {
            return default(Vector3);
        }

        public override VisualElement SetUpField(ExposedProperty property)
        {
            Vector3 propertyValue = property.GetValue() == null ? (Vector3)GetDefaultValue() : (Vector3)property.GetValue();

            Vector3Field vector3Field = new Vector3Field("Value:")
            {
                value = propertyValue
            };

            vector3Field.RegisterValueChangedCallback(evt =>
            {
                property.SetValue(evt.newValue);
            });

            return vector3Field;
        }
    }
#endif
}
