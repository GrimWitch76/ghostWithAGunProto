using UnityEngine;

namespace MyPackage.Attributes
{
    public class EditorNameAttribute : PropertyAttribute
    {
        public string DisplayName { get; }

        public EditorNameAttribute(string displayName)
        {
            DisplayName = displayName;
        }
    }
}