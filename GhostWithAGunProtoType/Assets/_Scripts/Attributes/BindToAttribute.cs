using UnityEngine;

namespace TCustomAttributes
{
    public class BindToAttribute : PropertyAttribute
    {
        public string MethodName;

        public BindToAttribute(string methodName)
        {
            MethodName = methodName;
        }
    }
}
