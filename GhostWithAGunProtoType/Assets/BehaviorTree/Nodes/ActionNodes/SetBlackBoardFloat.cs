using UnityEngine;

namespace BT.Nodes
{
	[NodeDesciption("Sets the value of a float Blackboard value.\nSucceeds if the blackboard value exists and fails otherwise.")]
    [NodePath(ACTION_NODE_PATH + "Set Blackboard Float")]
    public class SetBlackBoardFloat : ActionNode
	{
		public BlackboardKey FloatBlackboardKey;

		[SerializeField] private float _value;

		protected override NodeState OnStart()
		{
			if(Blackboard.HasValue(FloatBlackboardKey))
			{
				Blackboard.SetValue<float>(FloatBlackboardKey, _value);
				return NodeState.Success;
			}

			Debug.LogError($"No blackboard key with value {FloatBlackboardKey.Value}");
			return NodeState.Failure;
		}

		protected override NodeState OnUpdate()
		{
			return NodeState.Success;
		}

		protected override void OnExit()
		{
		}
	}
}