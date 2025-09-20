using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace BT
{
    public class BlackboardVariablesContainer : ScriptableObject
    {
        public BehaviorTree BehaviorTree;

#if UNITY_EDITOR
        [field: SerializeField] public Vector2 Position { get; set; }
        [field: SerializeField] public Vector2 Size { get; set; } = new Vector2(300f, 300f);

        public Rect GetDimensions()
        {
            return new Rect(Position, Size);
        }
#endif
        [field: SerializeField] public List<ExposedProperty> Variables { get; set; } = new List<ExposedProperty>();

#if UNITY_EDITOR
        public void RemoveProperty(ExposedProperty propertyToRemove)
        {
            if (BehaviorTree == null || propertyToRemove == null)
            {
                Debug.LogError("Invalid parent or sub-object.");
                return;
            }

            Variables.Remove(propertyToRemove);

            // Remove the sub-object from the parent asset
            AssetDatabase.RemoveObjectFromAsset(propertyToRemove);

            // Destroy the sub-object
            DestroyImmediate(propertyToRemove, true);

            // Save the changes
            AssetDatabase.SaveAssets();
        }
#endif

        public bool HasValue(BlackboardKey key)
        {
            return Variables.Exists(v => v.PropertyName == key.Value);
        }

        public T GetValue<T>(BlackboardKey key)
        {
            if (!Variables.Exists(p => p.PropertyName == key.Value))
            {
                Debug.LogError($"No property with the name {key.Value} found in the blackboard");
                return default(T);
            }

            //Debug.Log($"successfully got {key.Value} from blackboard");
            return (T)Variables.Find(p => p.PropertyName == key.Value).GetValue();
        }

        public void SetValue<T>(BlackboardKey key, T value)
        {
            ExposedProperty property = Variables.Find(v => v.PropertyName == key.Value);

            if(property == null)
            {
                Debug.LogError($"Blackboard doesn't contain a property with the key {key}");
                return;
            }

            property.SetValue(value);
        }

        public void AddProperty(ExposedProperty property)
        {
            if (Variables == null)
            {
                Variables = new List<ExposedProperty>();
            }

            Variables.Add(property);
        }
    }
}
