using System.Reflection;
using System.Collections.Generic;
using UnityEngine.UIElements;
using UnityEditor;
using UnityEngine;
using BT.Nodes;

namespace BT.BTEditor
{
    public class InspectorView : VisualElement
    {
        public new class UxmlFactor : UxmlFactory<InspectorView, VisualElement.UxmlTraits> { }
        // the custom inspector
        private Editor _editor;
        private NodeView _currentNodeView;
        private static Texture2D S_BlackBoardKeyIcon;

        public InspectorView()
        {

        }

        internal void UpdateSelection(NodeView nodeView)
        {
            _currentNodeView = nodeView;
            // clear previous selection elements
            Clear();
            // destroy the previous editor
            Object.DestroyImmediate(_editor);

            // create a new editor and pass the class to serialize the properties of
            _editor = Editor.CreateEditor(nodeView.Node);
            // create a container to hold the values of the editor
            IMGUIContainer container = new IMGUIContainer(RenderInspector);
            // add the container as a visual element to the inspector view
            Add(container);
        }

        private void RenderInspector()
        {
            if (_editor.target == null)
            {
                return;
            }

            // begin listening to changes in the inspector GUI
            EditorGUI.BeginChangeCheck();

            // get the serialized object
            SerializedObject serializedObject = _editor.serializedObject;
            serializedObject.Update();

            // iterate through all serialized properties
            SerializedProperty property = serializedObject.GetIterator();
            bool enterChildren = true;

            BaseNode node = _editor.target as BaseNode;

            if(node == null)
            {
                Debug.LogError("WTF");
            }

            //string nodeVariableName = node.VariableName;

            List<FieldInfo> blackboardFields = new List<FieldInfo>();

            while (property.NextVisible(enterChildren))
            {
                enterChildren = false;

                // Skip rendering if the field is of type BlackboardKey
                FieldInfo field = _editor.target.GetType().GetField(property.name, BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);

                // check for bb variables and add them to the list of bb properties
                if (field != null && field.FieldType == typeof(BlackboardKey))
                {
                    blackboardFields.Add(field);
                    continue;
                }

                //if (field != null && field.Name == "VariableName" && !string.IsNullOrEmpty(nodeVariableName) && node.BehaviorTree != null)
                //{
                //    DrawNodeVariableField(property, node, nodeVariableName);
                //    continue;
                //}

                // draw field normally
                EditorGUILayout.PropertyField(property, true);
            }

            serializedObject.ApplyModifiedProperties();

            for (int i = 0; i < blackboardFields.Count; i++)
            {
                BlackboardKey blackboardKey = blackboardFields[i].GetValue(node) as BlackboardKey;
                string blackboardName = ObjectNames.NicifyVariableName(blackboardFields[i].Name);
                DrawBlackboardKeyDropdown(blackboardKey, blackboardName);
            }

            // Apply changes made during GUI rendering
            if (EditorGUI.EndChangeCheck())
            {
                serializedObject.ApplyModifiedProperties();
            }
        }

        //private static void DrawNodeVariableField(SerializedProperty property, BaseNode node, string nodeVariableName)
        //{
        //    if (nodeVariableName.Contains(" "))
        //    {
        //        nodeVariableName = nodeVariableName.Replace(" ", "");
        //        node.VariableName = nodeVariableName;
        //    }

        //    if (!node.BehaviorTree.VariableNameExists(nodeVariableName, node))
        //    {
        //        node.BehaviorTree.UpdateVariable(node);
        //        EditorGUILayout.PropertyField(property, true);
        //    }
        //    else
        //    {
        //        EditorGUILayout.BeginHorizontal();

        //        // Display the PropertyField with red color
        //        GUI.color = Color.red;
        //        EditorGUILayout.PropertyField(property, true);
        //        GUI.color = Color.white;

        //        // Add a warning icon with a tooltip
        //        GUIContent iconContent = EditorGUIUtility.IconContent("console.erroricon");
        //        iconContent.tooltip = "Variable name already exists in the Behavior Tree";
        //        EditorGUILayout.LabelField(iconContent, GUILayout.Width(20), GUILayout.Height(EditorGUIUtility.singleLineHeight));

        //        // End the horizontal group
        //        EditorGUILayout.EndHorizontal();
        //    }
        //}

        private void DrawBlackboardKeyDropdown(BlackboardKey blackboardKey, string name)
        {
            Texture2D icon = GetBlackboardKeyIcon();

            // Create GUIContent with the icon and the label text
            GUIContent dropdownLabel = new GUIContent(" " + name, icon);

            List<string> options = GetBlackboardKeys();
            // highlight the selected element
            int selectedIndex = options.IndexOf(blackboardKey.Value);

            if (selectedIndex == -1)
            {
                selectedIndex = 0;
            }

            int newSelectedIndex = EditorGUILayout.Popup(dropdownLabel, selectedIndex, options.ToArray());

            if (newSelectedIndex != selectedIndex)
            {
                // Update BlackboardKey.Value when a new option is selected
                blackboardKey.Value = options[newSelectedIndex];
                EditorUtility.SetDirty(_currentNodeView.Node);
            }

            if(options.Count == 1)
            {
                blackboardKey.Value = options[0];
                EditorUtility.SetDirty(_currentNodeView.Node);
            }
        }

        private List<string> GetBlackboardKeys()
        {
            List<string> blackboardKeys = new List<string>();
            List<ExposedProperty> properties = _currentNodeView.Node.Blackboard.Variables;

            for (int i = 0; i < properties.Count; i++)
            {
                blackboardKeys.Add(properties[i].PropertyName);
            }

            return blackboardKeys;
        }

        private Texture2D GetBlackboardKeyIcon()
        {
            if (S_BlackBoardKeyIcon != null)
            {
                return S_BlackBoardKeyIcon;
            }

            string path = BehaviorTreeSettings.CORE_DIRECTORY + "/Icons/T_KeyIcon_small.png";
            S_BlackBoardKeyIcon = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
            return S_BlackBoardKeyIcon;
        }
    }
}