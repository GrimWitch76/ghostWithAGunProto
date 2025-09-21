using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BT
{
    public class FloatExposedProperty : ExposedProperty
    {
        public float Value;

        public override object GetValue()
        {
            return Value;
        }

        public override void SetValue(object newValue)
        {
            Value = (float)newValue;
        }
    }
}
