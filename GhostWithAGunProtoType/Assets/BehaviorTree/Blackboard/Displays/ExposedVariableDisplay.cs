using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace BT
{
    public abstract class ExposedVariableDisplay : ScriptableObject
    {
        public Color IconColor;

        public Type PropertyClass { get; set; }
        public string MenuCommand { get; set; }
        public string PropertyTypeName { get; set; }

        public abstract VisualElement SetUpField(ExposedProperty property);
        public abstract object GetDefaultValue();
        public abstract void Initialize();

        public ExposedVariableDisplay()
        {
            Initialize();
        }
    }
}
