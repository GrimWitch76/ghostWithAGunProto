using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BT
{
    public class Vector2ExposedProperty : ExposedProperty
    {
        public Vector2 Value;

        public override object GetValue()
        {
            return Value;
        }

        public override void SetValue(object newValue)
        {
            Value = (Vector2)newValue;
        }
    }
}
