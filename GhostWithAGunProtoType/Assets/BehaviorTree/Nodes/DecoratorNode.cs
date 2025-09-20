using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace BT.Nodes
{
    public abstract class DecoratorNode : BaseNode
    {
        [HideInInspector] public BaseNode Child;

        protected const string DECORATOR_NODE_PATH = MAIN_NODE_PATH + "Decorator Nodes/";

        public override void OnAwake()
        {
            Child.OnAwake();
        }

        public override void AddChild(BaseNode child)
        {
#if UNITY_EDITOR
            Undo.RecordObject(this, "Behavior Tree (Add Child)");
#endif

            Child = child;
            base.AddChild(child);
        }

        public override void RemoveChild(BaseNode child)
        {
#if UNITY_EDITOR
            Undo.RecordObject(this, "Behavior Tree (remove Child)");
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
            DecoratorNode node = Instantiate(this);
            node.Child = Child.Clone();
            return node;
        }

        protected override NodeState OnStart()
        {
            return NodeState.Running;
        }

        protected override void OnExit()
        {

        }

        public override void OnForceStopNode()
        {
            base.OnForceStopNode();

            State = NodeState.Failure;
            Child.OnForceStopNode();
        }

        public override void ClearChildren()
        {
            Child = null;
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
