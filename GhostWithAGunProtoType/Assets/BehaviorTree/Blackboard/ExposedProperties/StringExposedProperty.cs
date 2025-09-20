using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BT
{
    public class StringExposedProperty : ExposedProperty
    {
        public string Value;

        public override object GetValue()
        {
            return Value;
        }

        public override void SetValue(object newValue)
        {
            Value = (string)newValue;
        }
    }
}
