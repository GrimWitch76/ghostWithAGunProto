using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace BT.Nodes
{
    public class RootNode : BaseNode
    {
        //[HideInInspector]
        [SerializeField]
        private BaseNode Child;

        public override void OnAwake()
        {
            Child.OnAwake();
        }

        protected override NodeState OnStart()
        {
            return NodeState.Running;
        }

        protected override void OnExit()
        {

        }

        protected override NodeState OnUpdate()
        {
            return Child.Update();
        }

        public override void AddChild(BaseNode child)
        {
#if UNITY_EDITOR
            Undo.RecordObject(this, "Behavior Tree (Add Child)");

            if (child == null)
            {
                Debug.Log("CHILD IS NULL");
            }
#endif

            Child = child;
            base.AddChild(child);
        }

        public override void RemoveChild(BaseNode child)
        {
#if UNITY_EDITOR
            Undo.RecordObject(this, "Behavior Tree (Remove Child)");
#endif

            Child = null;
            base.RemoveChild(child);
        }

        public override List<BaseNode> GetChildren()
        {
            base.GetChildren();
            return new List<BaseNode>() { Child };
        }

        public override bool HasChildren()
        {
            return Child != null;
        }

        public override BaseNode Clone()
        {
            // we need to clone the node and its child and then assign the child to its new parent
            RootNode node = Instantiate(this);
            node.Child = Child.Clone();
            return node;
        }

        public override void OnForceStopNode()
        {
            base.OnForceStopNode();

            State = NodeState.Success;
            Child.OnForceStopNode();
        }

        public override void Interrupt()
        {
            base.Interrupt();

            Child.Interrupt();
        }

        public override bool Abort()
        {
            if (base.Abort())
            {
                Child.Abort();

                return true;
            }

            return false;
        }
    }
}
