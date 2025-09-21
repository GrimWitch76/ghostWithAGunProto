using UnityEditor;
using UnityEngine;

namespace MyPackage.Attributes
{
    [CustomPropertyDrawer(typeof(EditorNameAttribute))]
    public class EditorNameDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var attribute = (EditorNameAttribute)this.attribute;
            label.text = attribute.DisplayName;
            EditorGUI.PropertyField(position, property, label, true);
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return EditorGUI.GetPropertyHeight(property, label, true);
        }
    }
}