using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;

namespace BT.Nodes
{
    public abstract class BaseNode : ScriptableObject, IDisposable
    {
        [HideInInspector]
        public NodeState State = NodeState.NONE;
        public bool IsStarted { get; private set; } = false;
        [HideInInspector] public BlackboardVariablesContainer Blackboard { get; set; }
        [HideInInspector] public MonoBehaviour Agent;

        public BehaviorTree BehaviorTree { get; set; }

        //#if UNITY_EDITOR
        [HideInInspector]
        public string GUID;
        [HideInInspector]
        public Vector2 Position;

        [Serializable]
        public class NodeViewDetails
        {
            public string Name;
            [TextArea] public string Description;
        }

        public NodeViewDetails ViewDetails = new NodeViewDetails();

        /// <summary>
        /// The variable name that will allow access to this node from outside the BT system
        /// </summary>
        //public string VariableName;

        protected const string MAIN_NODE_PATH = "Default Nodes/";
        //#endif

        public NodeState Update()
        {
            if(State == NodeState.Interrupted)
            {
                State = NodeState.NONE;
                return State;
            }

            if (!IsStarted)
            {
                State = OnStart();
                IsStarted = true;
            }
            
            if (State == NodeState.Running)
            {
                State = OnUpdate();
            }

            if (State is NodeState.Failure or NodeState.Success)
            {
                OnExit();

                if (State == NodeState.Failure)
                {
                    OnForceStopNode();
                }

                IsStarted = false;
            }

            // if non of the conditions above are true and the node is still getting updated, then start it
            if(State == NodeState.NONE)
            {
                IsStarted = false;
            }

            return State;
        }

        public abstract void OnAwake();
        protected abstract NodeState OnStart();
        protected abstract NodeState OnUpdate();
        protected abstract void OnExit();

        public virtual void Interrupt()
        {
            OnExit();
            IsStarted = false;
            State = NodeState.Interrupted;
        }

        public virtual bool Abort()
        {
            if (State is not NodeState.Success and not NodeState.Failure)
            {
                State = NodeState.Aborted;
                IsStarted = false;
                OnExit();
                return true;
            }

            return false;
        }

        /// <summary>
        /// Called when the node is force-stopped
        /// </summary>
        public virtual void OnForceStopNode()
        {
            IsStarted = false;
        }

        public virtual void AddChild(BaseNode child)
        {
#if UNITY_EDITOR
            EditorUtility.SetDirty(this);
            AssetDatabase.SaveAssetIfDirty(this);
#endif
        }

        public virtual void RemoveChild(BaseNode child)
        {
#if UNITY_EDITOR
            EditorUtility.SetDirty(this);
            AssetDatabase.SaveAssetIfDirty(this);
#endif
        }

        /// <summary>
        /// Returns a copy of the node (Runtime copy) and clones its children if applicable.
        /// </summary>
        /// <returns>The copy of the node.</returns>
        public virtual BaseNode Clone()
        {
            State = NodeState.NONE;
            // Called when the nodes are created
            return Instantiate(this);
        }

        public virtual void Bind(MonoBehaviour agent, BlackboardVariablesContainer blackBoard)
        {
            Agent = agent;
            Blackboard = blackBoard;
        }

        public virtual void Dispose()
        {

        }

        #region Utilities
        public T GetAgent<T>() where T : MonoBehaviour
        {
            if (Agent == null)
            {
                Debug.LogError($"No agent found on node: {this.name}");
            }

            if (Agent is not T CastAgent)
            {
                Debug.LogError($"Agent for node: {this.name} is not of type {typeof(T).Name}");
                return null;
            }

            return CastAgent;
        }

        public virtual bool HasChildren()
        {
            return false;
        }

        public virtual List<BaseNode> GetChildren()
        {
            return null;
        }
        public virtual string GetNodeViewName()
        {
            return "Node Name";
        }

        public virtual void ClearChildren() { }
        #endregion
    }
}
