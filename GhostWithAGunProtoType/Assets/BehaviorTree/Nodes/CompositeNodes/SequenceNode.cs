using UnityEngine;

namespace BT.Nodes
{
    [NodePath(COMPOSITE_NODE_PATH + "Sequence Node")]
    public class SequenceNode : CompositeNode
    {
        private int _currentChild;

        protected override NodeState OnStart()
        {
            _currentChild = 0;

            return NodeState.Running;
        }

        protected override NodeState OnUpdate()
        {
            BaseNode child = Children[_currentChild];
            NodeState state;

            //if (child == null)
            //{
            //    Debug.LogError($"Child is null. Index: {_currentChild} out of {Children.Count}");
            //}

            state = child.Update();

            //if(VariableName == "TT")
            //Debug.Log($"Updating child {_currentChild}. State: {state}. At {Time.time}");

            switch (state)
            {
                case NodeState.Running:
                    {
                        return NodeState.Running;
                    }
                case NodeState.Failure:
                    {
                        return NodeState.Failure;
                    }
                    // REVIEW: should interruption be a success?
                case NodeState.Interrupted:
                case NodeState.Success:
                    {
                        break;
                    }
                case NodeState.Aborted:
                    {
                        return NodeState.Aborted;
                    }
            }

            _currentChild++;

            if (_currentChild == Children.Count)
            {
                return NodeState.Success;
            }
            else
            {
                return NodeState.Running;
            }
        }

        protected override void OnExit()
        {

        }

        public override void Interrupt()
        {
            base.Interrupt();

            if (_currentChild < Children.Count && _currentChild > -1)
            {
                Children[_currentChild].Interrupt();
            }
        }

        public override bool Abort()
        {
            if (base.Abort())
            {
                if (_currentChild < Children.Count && _currentChild > -1)
                {
                    Children[_currentChild].Abort();
                }

                return true;
            }

            return false;
        }
    }
}
