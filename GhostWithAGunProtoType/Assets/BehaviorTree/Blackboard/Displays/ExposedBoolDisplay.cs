using System.Collections;
using System.Collections.Generic;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace BT
{
    public class ExposedBoolDisplay : ExposedVariableDisplay
    {
        public override void Initialize()
        {
            MenuCommand = "Add Bool";
            PropertyTypeName = "Bool";
            PropertyClass = typeof(BoolExposedProperty);
            IconColor = new Color(0.91f, 0.30f, 0.24f);
        }

        public override object GetDefaultValue()
        {
            return default(bool);
        }

        public override VisualElement SetUpField(ExposedProperty property)
        {
            bool propertyValue = property.GetValue() == null ? false : (bool)property.GetValue(); 
            Toggle toggleField = new Toggle("Value:") 
            { 
                value = propertyValue 
            }; 

            toggleField.RegisterValueChangedCallback(evt => 
            { 
                property.SetValue(evt.newValue); 
            }); 
            
            return toggleField;
        }
    }
}
