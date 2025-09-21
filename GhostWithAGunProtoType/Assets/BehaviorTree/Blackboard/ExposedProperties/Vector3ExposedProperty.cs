using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BT
{
    public class Vector3ExposedProperty : ExposedProperty
    {
        public Vector3 Value;

        public override object GetValue()
        {
            return Value;
        }

        public override void SetValue(object newValue)
        {
            Value = (Vector3)newValue;
        }
    }
}
