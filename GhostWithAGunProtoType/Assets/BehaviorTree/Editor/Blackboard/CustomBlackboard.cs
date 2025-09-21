
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine.UIElements;

namespace BT.BTEditor
{
    public class CustomBlackboard : Blackboard
    {
        private BehaviorTreeView _treeView;
        private BehaviorTreeEditor _editor;
        private BlackboardVariablesContainer _blackboardVariables;

        private readonly List<ExposedVariableDisplay> VariableDisplays = new List<ExposedVariableDisplay>()
        {
            new ExposedIntDisplay(), new ExposedFloatDisplay(),
            new ExposedBoolDisplay(), new ExposedStringDisplay(),
            new ExposedGameObjectDisplay(), new ExposedVector2Display(), new ExposedVector3Display(),
            new ExposedColorDisplay(), new ExposedScriptableObjectDisplay()
        };

        public CustomBlackboard(BehaviorTreeView treeView, BehaviorTreeEditor editor) : base(treeView)
        {
            _treeView = treeView;
            _editor = editor;
            //_blackboardVariables = _editor.BehaviorTree.BlackboardContainer;
            _blackboardVariables = BehaviorTreeEditor.SelectedBehaviorTree.BlackboardContainer;
            InitializeBlackboard();

            this.RegisterCallback<KeyDownEvent>(evt =>
            {
                if (evt.keyCode == KeyCode.Delete)
                {
                    // Prevent the default delete logic from executing
                    evt.StopImmediatePropagation();

                    // Now implement your custom deletion logic
                    HandleDeleteKey();
                }
            }, TrickleDown.TrickleDown);

            // register for the dimensions change to store the size and position
            this.RegisterCallback<GeometryChangedEvent>(OnGeometryChanged);
        }

        private void OnGeometryChanged(GeometryChangedEvent evt)
        {
            // update the dimensions
            _blackboardVariables.Position = new Vector2(this.resolvedStyle.left, this.resolvedStyle.top);
            _blackboardVariables.Size = new Vector2(this.resolvedStyle.width, this.resolvedStyle.height);
        }

        private void HandleDeleteKey()
        {
            if (this.selection.Count == 0)
            {
                return;
            }

            var selection = this.selection[0];

            if (selection is VisualElement container)
            {
                while (container.name != "Container")
                {
                    if (container.parent == null)
                    {
                        return;
                    }
                    container = container.parent;
                }


                if (container.name != "Container")
                {
                    Debug.Log($"Not the container. Just {container.name}");
                    return;
                }

                BlackboardField selectedField = container.Q<BlackboardField>();

                if (selectedField == null)
                {
                    Debug.LogError($"No blackboard field at the selected visual element.");
                    return;
                }

                // Find and remove the corresponding property in the ScriptableObject
                ExposedProperty propertyToRemove = _blackboardVariables.Variables
                    .Find(p => p.PropertyName == selectedField.text);

                if (propertyToRemove != null)
                {
                    _blackboardVariables.RemoveProperty(propertyToRemove);
                }
                else
                {
                    Debug.LogError("No property to delete");
                }

                // Remove the field from the blackboard UI
                container.RemoveFromHierarchy();
            }
        }

        private void InitializeBlackboard()
        {
            this.scrollable = true;
            this.Add(new BlackboardSection { title = "Variables" });
            this.SetPosition(_blackboardVariables.GetDimensions());

            // called when the plus icon is selected
            this.addItemRequested = AddItemRequested;
            // called when the value's text field is modified
            this.editTextRequested = EditTextRequested;
            // loads the values of the blackboard variables container into the visual part of the blackboard
            LoadBlackboard();
        }

        private void AddItemRequested(Blackboard blackboard)
        {
            GenericMenu menu = new GenericMenu();

            for (int i = 0; i < VariableDisplays.Count; i++)
            {
                ExposedVariableDisplay display = VariableDisplays[i];
                menu.AddItem(new GUIContent(display.MenuCommand), false, () => AddNewProperty(display));
            }

            menu.ShowAsContext();
        }

        private void AddNewProperty(ExposedVariableDisplay display)
        {
            ExposedProperty property = GetPropertyInstance(display);

            if (!Application.isPlaying)
            {
                AssetDatabase.AddObjectToAsset(property, _blackboardVariables);
            }

            AssetDatabase.SaveAssets();

            if (_blackboardVariables == null)
            {
                Debug.Log("Issue with variabels");
            }

            if (_blackboardVariables == null)
            {
                Debug.Log("Issue with variabels");
            }

            // if there is a property that already exists with the same name as the created property, then rename the new property
            while (_blackboardVariables.Variables.Exists(p => p.PropertyName == display.PropertyTypeName))
            {
                display.PropertyTypeName = $"{display.PropertyTypeName}_01";
            }

            property.PropertyName = display.PropertyTypeName;
            property.PropertyTypeName = display.PropertyTypeName;

            // set a default value
            object propertyValue = display.GetDefaultValue();
            property.SetValue(propertyValue);

            _blackboardVariables.AddProperty(property);

            VisualElement container = CreateContainer();
            BlackboardField blackboardField = CreateBlackboardField(property, display.IconColor);
            container.Add(blackboardField);
            VisualElement propertyValueField = display.SetUpField(property);

            // the first parameter is the parent of the row and the second is the child
            BlackboardRow bbRow = CreateBlackboardRow(property, blackboardField, propertyValueField);

            container.Add(bbRow);
            this.Add(container);
        }

        private void AddExistingProperty(ExposedProperty property, ExposedVariableDisplay display)
        {
            VisualElement container = CreateContainer();
            BlackboardField blackboardField = CreateBlackboardField(property, display.IconColor);
            container.Add(blackboardField);
            // make the row or the field expanded if property.Expanded is true
            VisualElement propertyValueField = display.SetUpField(property);
            BlackboardRow bbRow = CreateBlackboardRow(property, blackboardField, propertyValueField);

            container.Add(bbRow);
            this.Add(container);
        }

        private VisualElement CreateContainer()
        {
            VisualElement container = new VisualElement()
            {
                name = "Container"
            };

            return container;
        }

        private BlackboardField CreateBlackboardField(ExposedProperty property, Color color)
        {
            BlackboardField blackboardField = new BlackboardField
            {
                text = property.PropertyName,
                typeText = $"{property.PropertyTypeName} property",
                icon = GenerateCircleTexture(color)
            };

            return blackboardField;
        }

        private BlackboardRow CreateBlackboardRow(ExposedProperty property, VisualElement parent, VisualElement child)
        {
            BlackboardRow bbRow = new BlackboardRow(parent, child)
            {
                expanded = property.Expanded
            };

            bbRow.RegisterCallback<GeometryChangedEvent>(evt =>
            {
                property.Expanded = bbRow.expanded;
            });

            return bbRow;
        }

        private Texture2D GenerateCircleTexture(Color color, int size = 16)
        {
            Texture2D texture = new Texture2D(size, size);
            float radius = size / 2f;
            Vector2 center = new Vector2(radius, radius);

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float distance = Vector2.Distance(center, new Vector2(x, y));
                    if (distance <= radius)
                    {
                        texture.SetPixel(x, y, color);
                    }
                    else
                    {
                        texture.SetPixel(x, y, Color.clear);
                    }
                }
            }

            texture.Apply();
            return texture;
        }
        //#endregion

        private ExposedProperty GetPropertyInstance(ExposedVariableDisplay display)
        {
            ExposedProperty property = ScriptableObject.CreateInstance(display.PropertyClass) as ExposedProperty;
            property.name = display.PropertyTypeName;
            return property;
        }

        private void EditTextRequested(Blackboard blackboard, VisualElement element, string newValue)
        {
            string oldPropertyName = ((BlackboardField)element).text;

            if (_blackboardVariables.Variables.Exists(p => p.PropertyName == newValue))
            {
                EditorUtility.DisplayDialog("Error", $"A property with the name \"{newValue}\" already exists.", "Okay");
                return;
            }

            ExposedProperty selectedProperty = _blackboardVariables.Variables.Find(p => p.PropertyName == oldPropertyName);
            selectedProperty.PropertyName = newValue;
            ((BlackboardField)element).text = newValue;
        }


        //#region Load Blackboard properties
        private void LoadBlackboard()
        {
            List<ExposedProperty> properties = _blackboardVariables.Variables;

            if (properties == null || properties.Count == 0)
            {
                return;
            }

            for (int i = 0; i < properties.Count; i++)
            {
                ExposedProperty property = properties[i];
                ExposedVariableDisplay display = VariableDisplays.Find(d => d.PropertyClass == property.GetType());

                if (display == null)
                {
                    continue;
                }

                AddExistingProperty(property, display);
            }
        }
        //#endregion
    }
}

