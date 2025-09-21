using UnityEngine;

namespace BT.Nodes
{
    [NodePath(COMPOSITE_NODE_PATH + "Selector Node")]
    public class SelectorNode : CompositeNode
    {
        private int _currentChildIndex;
        
        protected override NodeState OnStart()
		{
			_currentChildIndex = 0;

            return NodeState.Running;
        }

        protected override NodeState OnUpdate()
        {
            BaseNode child = Children[_currentChildIndex];

            if (child == null)
            {
                Debug.LogError($"Child is null. Index: {_currentChildIndex} out of {Children.Count}");
            }

            NodeState state = child.Update();

            switch (state)
            {
                case NodeState.Running:
                case NodeState.NONE:
                    {
                        return NodeState.Running;
                    }
                case NodeState.Failure:
                case NodeState.Interrupted:
                    {
                        break;
                    }
                case NodeState.Success:
                    {
                        return NodeState.Success;
                    }
                case NodeState.Aborted:
                    {
                        return NodeState.Aborted;
                    }
            }

            _currentChildIndex++;

            if (_currentChildIndex == Children.Count)
            {
                return NodeState.Failure;
            }

            return NodeState.Running;
        }

		protected override void OnExit()
		{
            // end logic
        }

        public override void Interrupt()
        {
            base.Interrupt();

            if (_currentChildIndex < Children.Count && _currentChildIndex >= 0)
            {
                Children[_currentChildIndex].Interrupt();
            }
        }

        public override bool Abort()
        {
            if (base.Abort())
            {
                if (_currentChildIndex < Children.Count && _currentChildIndex >= 0)
                {
                    Children[_currentChildIndex].Abort();
                }

                return true;
            }

            return false;
        }
    }
}