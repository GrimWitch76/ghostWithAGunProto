using UnityEngine;

namespace BT.Nodes
{
    [NodePath(DECORATOR_NODE_PATH + "Force State Node")]
    public class ForceStateNode : DecoratorNode
	{
		[SerializeField] private NodeState _returnedState;

		protected override NodeState OnStart()
        {
            return NodeState.Running;
        }

		protected override NodeState OnUpdate()
		{
			NodeState state = Child.Update();

			if (state == NodeState.Running)
			{
				return state;
			}

			return _returnedState;
		}

		protected override void OnExit()
		{
			// end logic
		}
	}
}