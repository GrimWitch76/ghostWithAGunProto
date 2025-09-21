using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BT
{
    public class ColorExposedProperty : ExposedProperty
    {
        public Color Value;

        public override object GetValue()
        {
            return Value;
        }

        public override void SetValue(object newValue)
        {
            Value = (Color)newValue;
        }
    }
}
