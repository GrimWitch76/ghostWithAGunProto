using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BT
{
    public class GameObjectExposedProperty : ExposedProperty
    {
        public GameObject Value;

        public override object GetValue()
        {
            return Value;
        }

        public override void SetValue(object newValue)
        {
            Value = (GameObject)newValue;
        }
    }
}
