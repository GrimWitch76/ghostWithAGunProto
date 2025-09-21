using System.Collections;
using System.Collections.Generic;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace BT
{
#if UNITY_EDITOR
    public class ExposedVector2Display : ExposedVariableDisplay
    {
        public override void Initialize()
        {
            MenuCommand = "Add Vector2";
            PropertyTypeName = "Vector2";
            PropertyClass = typeof(Vector2ExposedProperty);
            IconColor = new Color(0.76f, 0.55f, 0.33f);
        }

        public override object GetDefaultValue()
        {
            return default(Vector2);
        }

        public override VisualElement SetUpField(ExposedProperty property)
        {
            Vector2 propertyValue = property.GetValue() == null ? (Vector2)GetDefaultValue() : (Vector2)property.GetValue();

            Vector2Field vector2Field = new Vector2Field("Value:")
            {
                value = propertyValue
            };

            vector2Field.RegisterValueChangedCallback(evt =>
            {
                property.SetValue(evt.newValue);
            });

            return vector2Field;
        }
    }
#endif
}
