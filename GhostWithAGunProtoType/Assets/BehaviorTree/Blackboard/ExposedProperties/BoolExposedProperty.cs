using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BT
{
    public class BoolExposedProperty : ExposedProperty
    {
        public bool Value;

        public override object GetValue()
        {
            return Value;
        }

        public override void SetValue(object newValue)
        {
            Value = (bool)newValue;
        }
    }
}
