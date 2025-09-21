using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEditor;

namespace BT.Nodes
{
    public abstract class CompositeNode : BaseNode
    {
        protected const string COMPOSITE_NODE_PATH = MAIN_NODE_PATH + "Composite Nodes/";

        [HideInInspector] public List<BaseNode> Children = new List<BaseNode>();

        public override void OnAwake()
        {
            //try
            //{
                foreach (var child in Children)
                {
                    if(child == null)
                    {
                        Debug.LogError($"child with index {Children.IndexOf(child)} is null");
                        continue;
                    }

                    child.OnAwake();
                }
            //}
            //catch
            //{
            //    Debug.Log(ViewDetails.Name);
            //}
        }

        public override void AddChild(BaseNode child)
        {
#if UNITY_EDITOR
            Undo.RecordObject(this, "Behavior Tree (Add Child)");
#endif

            Children.Add(child);
            base.AddChild(child);
        }

        public override void RemoveChild(BaseNode child)
        {
#if UNITY_EDITOR
            Undo.RecordObject(this, "Behavior Tree (Remove Child)");
#endif

            Children.Remove(child);
            base.RemoveChild(child);
        }

        public override List<BaseNode> GetChildren()
        {
            base.GetChildren();
            return Children;
        }

        public override bool HasChildren()
        {
            Children.RemoveAll(c => c == null);

            return Children.Count > 0;
        }

        public override BaseNode Clone()
        {
            // we need to clone the node and its child and then assign the child to its new parent
            CompositeNode node = Instantiate(this);
            node.Children = Children.ConvertAll(c => c.Clone());
            return node;
        }

        public void OnRefresh()
        {
            Children = Children.OrderBy(c => c.Position.y).ToList();
        }

        public override void OnForceStopNode()
        {
            base.OnForceStopNode();

            State = NodeState.Failure;
            Children.ForEach(c => c.OnForceStopNode());
        }

        public override void ClearChildren()
        {
            Children.Clear();
        }
    }
}
