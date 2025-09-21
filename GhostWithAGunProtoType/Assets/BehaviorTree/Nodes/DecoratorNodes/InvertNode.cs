namespace BT.Nodes
{
	[NodePath(DECORATOR_NODE_PATH + "Invert Node")]
	public class InvertNode : DecoratorNode
	{
		protected override NodeState OnStart()
        {
            return NodeState.Running;
        }

		protected override NodeState OnUpdate()
		{
			NodeState state = Child.Update();

			switch (state)
			{
				case NodeState.Running:
					{
						return NodeState.Running;
					}
				case NodeState.Success:
					{
						return NodeState.Failure;
					}
				case NodeState.Failure:
					{
						return NodeState.Success;
					}
				default:
					{
						return state;
					}
			}
		}

		protected override void OnExit()
		{
			// end logic
		}
	}
}