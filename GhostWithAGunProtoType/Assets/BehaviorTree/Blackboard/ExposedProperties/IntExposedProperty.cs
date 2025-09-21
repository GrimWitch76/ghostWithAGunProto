using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BT
{
    public class IntExposedProperty : ExposedProperty
    {
        public int Value;

        public override object GetValue()
        {
            return Value;
        }

        public override void SetValue(object newValue)
        {
            Value = (int)newValue;
        }
    }
}
