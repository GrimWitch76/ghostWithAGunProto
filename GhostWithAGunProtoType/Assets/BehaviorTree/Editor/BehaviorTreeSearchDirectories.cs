using System;
using System.Linq;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace BT
{
    using Nodes;

    [Serializable]
    public class NodeSearchDirectory
    {
        [HideInInspector] public string ClassName { get; set; }

        public string Directory;
        public Type ClassType;
    }

    [CreateAssetMenu(menuName = "BT/Behavior Tree Search Directories (don't touch)")]
    public class BehaviorTreeSearchDirectories : ScriptableObject
    {
        [field: SerializeField] public List<NodeSearchDirectory> Directories { get; private set; } = new List<NodeSearchDirectory>();

        public void RefreshDictionary()
        {
            TypeCache.TypeCollection nodeTypes = TypeCache.GetTypesDerivedFrom(typeof(BaseNode));
            List<Type> projectTypes = nodeTypes.ToList();

            // remove types that don't exist in the project
            for (int i = Directories.Count - 1; i >= 0; i--)
            {
                if(!projectTypes.Exists(t => t == Directories[i].ClassType))
                {
                    Directories.RemoveAt(i);
                }
            }

            for (int i = Directories.Count - 1; i >= 0; i--)
            {
                string[] names = Directories[i].Directory.Split('/');
                string className = names.Last();

                if(nodeTypes.ToList().Exists(c => c == Directories[i].ClassType))
                {
                    Directories.Remove(Directories[i]);
                }
            }

            foreach (Type nodeType in nodeTypes)
            {
                if(Directories.Exists(d => d.ClassType == nodeType) || nodeType.IsAbstract || nodeType == typeof(RootNode))
                {
                    continue;
                }

                string childDirectory = $"New Nodes/{ObjectNames.NicifyVariableName(nodeType.BaseType.Name + "s")}/{ObjectNames.NicifyVariableName(nodeType.Name)}";

                try
                {
                    Attribute attr = Attribute.GetCustomAttribute(nodeType, typeof(NodePathAttribute));

                    if (attr is NodePathAttribute customAttribute)
                    {
                        childDirectory = customAttribute.Path;
                        childDirectory = string.IsNullOrEmpty(childDirectory) ? "New Nodes" : childDirectory;
                    }
                    else
                    {
                        Debug.Log($"There is No attribute for node {nodeType.Name}");
                    }
                }
                catch (Exception ex)
                {
                    Debug.Log($"No attribute for node {nodeType.Name}");
                }

                Directories.Add(new NodeSearchDirectory()
                {
                    ClassName = nodeType.FullName,
                    Directory = childDirectory,
                    ClassType = nodeType
                });
            }
        }

        public void ResetDictionary()
        {
            Directories.Clear();
        }
    }
}
