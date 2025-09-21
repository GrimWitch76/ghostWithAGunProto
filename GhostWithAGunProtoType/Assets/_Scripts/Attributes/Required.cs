using System;
using UnityEngine;

namespace TCustomAttributes
{
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = true)]
    public class Required : PropertyAttribute { }
}
