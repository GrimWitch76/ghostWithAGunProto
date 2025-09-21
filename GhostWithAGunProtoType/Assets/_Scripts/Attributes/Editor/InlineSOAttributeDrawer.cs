using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace TCustomAttributes
{
    [CustomPropertyDrawer(typeof(InlineSOAttribute))]
    public class InlineSOAttributeDrawer : PropertyDrawer
    {
        private Editor _cachedEditor;

        private const float PADDING_AMOUNT = 30f;

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            float height = EditorGUIUtility.singleLineHeight + 8f; // field + outer box padding

            if (property.objectReferenceValue != null)
            {
                if (_cachedEditor == null || _cachedEditor.target != property.objectReferenceValue)
                    Editor.CreateCachedEditor(property.objectReferenceValue, null, ref _cachedEditor);

                SerializedProperty prop = _cachedEditor.serializedObject.GetIterator();
                if (prop.NextVisible(true))
                {
                    do height += EditorGUI.GetPropertyHeight(prop, true) + 4;
                    while (prop.NextVisible(false));
                }

                height += 12f; // padding for inner box
            }

            return height;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if (property.objectReferenceValue != null &&
                (_cachedEditor == null || _cachedEditor.target != property.objectReferenceValue))
            {
                Editor.CreateCachedEditor(property.objectReferenceValue, null, ref _cachedEditor);
            }

            float lineHeight = EditorGUIUtility.singleLineHeight;
            float spacing = 4f;

            // --- Outer Box
            float totalHeight = GetPropertyHeight(property, label);
            Rect outerBox = new Rect(position.x, position.y, position.width, totalHeight);
            GUI.Box(outerBox, GUIContent.none, EditorStyles.helpBox);

            // --- Draw Reference Field
            Rect fieldRect = new Rect(position.x + 4, position.y + 4, position.width - 8, lineHeight);
            var rawName = property.name;
            label = new GUIContent(ObjectNames.NicifyVariableName(rawName));
            EditorGUI.PropertyField(fieldRect, property, label);

            // --- Draw Inner Inspector
            if (property.objectReferenceValue == null || _cachedEditor == null)
                return;

            SerializedObject so = _cachedEditor.serializedObject;
            SerializedProperty prop = so.GetIterator();

            float y = fieldRect.y + lineHeight + spacing;

            float contentHeight = 0f;
            if (prop.NextVisible(true))
            {
                do contentHeight += EditorGUI.GetPropertyHeight(prop, true) + spacing;
                while (prop.NextVisible(false));
            }

            Rect innerBox = new Rect(position.x + 8, y - spacing / 2f, position.width - 16, contentHeight + 8);
            GUI.Box(innerBox, GUIContent.none, EditorStyles.helpBox);

            y += 4;

            prop = so.GetIterator();
            if (prop.NextVisible(true))
            {
                do
                {
                    float h = EditorGUI.GetPropertyHeight(prop, true);
                    Rect r = new Rect(innerBox.x + 4, y, innerBox.width - 8, h);
                    EditorGUI.PropertyField(r, prop, true);
                    y += h + spacing;
                } while (prop.NextVisible(false));
            }
        }
    }
}
