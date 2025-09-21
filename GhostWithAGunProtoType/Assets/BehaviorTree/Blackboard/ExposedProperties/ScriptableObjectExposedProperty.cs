using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BT
{
    public class ScriptableObjectExposedProperty : ExposedProperty
    {
        public ScriptableObject Value;

        public override object GetValue()
        {
            return Value;
        }

        public override void SetValue(object newValue)
        {
            if(newValue == null)
            {
                Value = null;
                return;
            }

            if (newValue is ScriptableObject scriptableObject)
            {
                Value = scriptableObject;
            }
            else
            {
                Debug.LogError("New value is not a ScriptableObject!");
            }
        }
    }
}
