using System.Collections;
using System.Collections.Generic;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace BT
{
#if UNITY_EDITOR
    public class ExposedGameObjectDisplay : ExposedVariableDisplay
    {
        public override void Initialize()
        {
            MenuCommand = "Add GameObject";
            PropertyTypeName = "GameObject";
            PropertyClass = typeof(GameObjectExposedProperty);
            IconColor = new Color(0.35f, 0.76f, 0.35f);
        }

        public override object GetDefaultValue()
        {
            return default(GameObject);
        }

        public override VisualElement SetUpField(ExposedProperty property)
        {
            GameObject propertyValue = property.GetValue() == null ? null : (GameObject)property.GetValue(); 
            
            ObjectField gameObjectField = new ObjectField("Value:") 
            { 
                objectType = typeof(GameObject), value = propertyValue
            }; 
            
            gameObjectField.RegisterValueChangedCallback(evt => 
            { 
                property.SetValue(evt.newValue); 
            }); 
            
            return gameObjectField;
        }
    }
#endif
}
