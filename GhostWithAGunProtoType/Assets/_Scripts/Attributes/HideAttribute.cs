using System;
using UnityEditor;
using UnityEngine;

namespace TCustomAttributes
{
    /// <summary>
    /// Hides the property if the condition passed is met.
    /// </summary>
    public class HideAttribute : PropertyAttribute
    {
        /// <summary>
        /// The condition passed.
        /// </summary>
        public string ConditionCode;
        public HideAttribute(string conditionCode)
        {
            ConditionCode = conditionCode;
        }
    }

#if UNITY_EDITOR
    [CustomPropertyDrawer(typeof(HideAttribute))]
    public class HideAttributeDrawer : PropertyDrawer
    {
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            if (!ShouldHide(property))
            {
                return EditorGUI.GetPropertyHeight(property, label, true);
            }

            return -EditorGUIUtility.standardVerticalSpacing;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if (!ShouldHide(property))
            {
                EditorGUI.PropertyField(position, property, label, true);
            }
        }

        private bool ShouldHide(SerializedProperty property)
        {
            try
            {
                var hideAttr = attribute as HideAttribute;
                property.serializedObject.ApplyModifiedProperties();
                var target = property.serializedObject.targetObject;
                return HideCodeEvaluator.ShouldHide(hideAttr.ConditionCode, target);
            }
            catch
            {
                Debug.LogError("Something went wrong in ShouldHIde");
                return false;
            }
        }
    }
#endif
}