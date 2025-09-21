using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace BT
{
    using Nodes;
    using System;

    [CreateAssetMenu(fileName = "BT_NAME", menuName = "BT/Behavior Tree Asset")]
    public class BehaviorTree : ScriptableObject
    {
        public bool IsInitiated { get; private set; } = false;

        public List<BaseNode> Nodes = new List<BaseNode>();
        public BaseNode RootNode;
        [HideInInspector] public Dictionary<BaseNode, string> _nodeVariables = new Dictionary<BaseNode, string>();
        [HideInInspector] private Dictionary<string, BaseNode> _variableToNode = new();
        [HideInInspector] public BlackboardVariablesContainer BlackboardContainer;
        [HideInInspector] public MonoBehaviour Agent;
        [HideInInspector] public NodeState TreeState = NodeState.Running;

        public void AwakeBT()
        {
            RootNode.OnAwake();
            IsInitiated = true;
        }

        public void StartBT()
        {
            RootNode.State = NodeState.Running;
        }

        public NodeState UpdateBT()
        {
            if(RootNode.State == NodeState.Running)
            {
                TreeState = RootNode.Update();
            }

            return TreeState;
        }

        public void StopBT()
        {
            RootNode.Abort();
        }


        [HideInInspector] public bool IsMinimapDisplayed = false;
        [HideInInspector] public bool IsBlackboardDisplayed = false;
        // the values that center the view at the zoom of one.
        [HideInInspector] public Vector3 ViewPosition = new Vector3(431, 358, 0f);
        [HideInInspector] public Vector3 ViewZoom = Vector3.one;

        private const string UNDO_REDO_CREATE_NODE_ID = "Behavior Tree (CreateNode)";
        private const string UNDO_REDO_DELETE_NODE_ID = "Behavior Tree (DeleteNode)";


        #region Node Variables
        //public void UpdateVariable(BaseNode node)
        //{
        //    string variable = node.VariableName;

        //    if(string.IsNullOrEmpty(variable))
        //    {
        //        _nodeVariables.Remove(node);
        //    }

        //    if (_nodeVariables.TryGetValue(node, out string previousVariable))
        //    {
        //        _variableToNode.Remove(previousVariable);
        //    }

        //    _nodeVariables[node] = variable;
        //    _variableToNode[variable] = node;
        //}

        public bool VariableNameExists(string variableName, BaseNode ownerNode)
        {
            if (_variableToNode.Count == 0)
            {
                return false;
            }

            return _variableToNode.TryGetValue(variableName, out BaseNode existingNode) && existingNode != ownerNode;
        }

        public BaseNode GetNodeByVariable(string variableName)
        {
            if(_variableToNode.Count == 0)
            {
                return null;
            }

            return _variableToNode.TryGetValue(variableName, out BaseNode node) ? node : null;
        }
        #endregion

#if UNITY_EDITOR
        public BaseNode CreateNode(Type type)
        {
            BaseNode node = ScriptableObject.CreateInstance(type) as BaseNode;
            node.name = type.Name;
            node.GUID = GUID.Generate().ToString();
            node.Blackboard = BlackboardContainer;

            // TODO: update the BT reference in the cloned nodes to be the cloned tree. So far, it's not needed.
            node.BehaviorTree = this;

            Undo.RecordObject(this, UNDO_REDO_CREATE_NODE_ID);
            Nodes.Add(node);

            // if the application is not playing, then save the object to the data assets
            if (!Application.isPlaying)
            {
                AssetDatabase.AddObjectToAsset(node, this);
            }

            Undo.RegisterCreatedObjectUndo(node, UNDO_REDO_CREATE_NODE_ID);

            node.ViewDetails.Name = ObjectNames.NicifyVariableName(node.GetType().Name.Replace("Node", string.Empty));

            AssetDatabase.SaveAssets();
            return node;
        }

        public void DeleteNode(BaseNode node)
        {
            Undo.RecordObject(this, UNDO_REDO_DELETE_NODE_ID);
            Nodes.Remove(node);

            Undo.DestroyObjectImmediate(node);
            AssetDatabase.SaveAssets();
        }
#endif

        /// <summary>
        /// Returns a runtime duplicate of the behavior tree
        /// </summary>
        /// <returns></returns>
        public BehaviorTree Clone()
        {
            BehaviorTree tree = Instantiate(this);
            tree.RootNode = tree.RootNode.Clone();
            tree.Nodes = new List<BaseNode>();

            // fill the tree with all the children of the root node. Perform a depth first traversel :,)
            Traverse(tree.RootNode, n =>
            {
                tree.Nodes.Add(n);
            });

            return tree;
        }

        public void Bind(MonoBehaviour agent)
        {
            Agent = agent;
            Traverse(RootNode, node => node.Bind(agent, BlackboardContainer));
        }

        public void Traverse(BaseNode node, Action<BaseNode> visitor)
        {
            if (node == null)
            {
                Debug.LogError("No node");
                return;
            }
         
            visitor?.Invoke(node);
            List<BaseNode> children = node.GetChildren();
            children.ForEach(n => Traverse(n, visitor));
        }


#if UNITY_EDITOR
        #region Blackboard
        public void CreateBlackboardContainer()
        {
            if(BlackboardContainer != null)
            {
                return;
            }

            BlackboardContainer = ScriptableObject.CreateInstance<BlackboardVariablesContainer>();
            BlackboardContainer.name = "Blackboard";
            BlackboardContainer.BehaviorTree = this;


            if (!Application.isPlaying)
            {
                AssetDatabase.AddObjectToAsset(BlackboardContainer, this);
            }

            AssetDatabase.SaveAssets();
        }
#endregion

        public void Refresh()
        {
            if (BlackboardContainer == null)
            {
                return;
            }

            if (BlackboardContainer.BehaviorTree == null)
            {
                BlackboardContainer.BehaviorTree = this;
            }

            Nodes.ForEach(n => n.Blackboard = BlackboardContainer);

            if (BlackboardContainer == null || BlackboardContainer.Variables == null || BlackboardContainer.Variables.Count == 0)
            {
                return;
            }

            RefreshNodeVariables();

            RefreshBlackBoard();
        }

        private void OnEnable()
        {
            RefreshNodeVariables();
        }

        private void RefreshNodeVariables()
        {
            if (_variableToNode.Count == 0)
            {
                _variableToNode.Clear();
                _nodeVariables.Clear();

                // refresh the variable names
                for (int i = 0; i < Nodes.Count; i++)
                {
                    var node = Nodes[i];

                    if(node == null)
                    {
                        continue;
                    }

                    //if (string.IsNullOrEmpty(node.VariableName))
                    //{
                    //    continue;
                    //}

                    //if (_variableToNode.ContainsKey(node.VariableName))
                    //{
                    //    continue;
                    //}

                    //_nodeVariables.Add(node, node.VariableName);
                    //_variableToNode.Add(node.VariableName, node);
                }
            }
        }

        private void RefreshBlackBoard()
        {
            string path = AssetDatabase.GetAssetPath(this);
            UnityEngine.Object[] subAssets = AssetDatabase.LoadAllAssetsAtPath(path);

            foreach (var asset in subAssets)
            {
                if (asset != this)
                {
                    if (asset is ExposedProperty property && !BlackboardContainer.Variables.Contains(property))
                    {
                        AssetDatabase.RemoveObjectFromAsset(asset);

                        DestroyImmediate(asset, true);

                        AssetDatabase.SaveAssets();
                    }
                }
            }
        }
#endif
    }
}
