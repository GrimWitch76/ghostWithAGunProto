using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BT.Nodes
{
    [NodeDesciption("Increment the value of a float Blackboard value.\nSucceeds if the blackboard value exists and fails otherwise.")]
    [NodePath(ACTION_NODE_PATH + "Add Blackboard Float")]
    public class AddBlackboardFloat : ActionNode
    {
		public BlackboardKey FloatBlackboardKey;

		[SerializeField] private float _value;

		protected override NodeState OnStart()
		{
			if (Blackboard.HasValue(FloatBlackboardKey))
			{
				float currentValue = Blackboard.GetValue<float>(FloatBlackboardKey);

				Blackboard.SetValue<float>(FloatBlackboardKey, currentValue + _value);
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
