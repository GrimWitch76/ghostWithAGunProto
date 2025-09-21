using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BT
{
    public abstract class ExposedProperty : ScriptableObject
    {
        public string PropertyName;

        [field: SerializeField] public bool Expanded { get; set; }
        public string PropertyTypeName { get; set; }

        public abstract object GetValue();
        public abstract void SetValue(object newValue);
    }
}
