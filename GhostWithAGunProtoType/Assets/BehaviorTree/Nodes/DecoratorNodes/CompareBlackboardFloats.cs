using BT.Utilities;
using UnityEngine;

namespace BT.Nodes
{
	[NodeDesciption("Compares the value of a Blackboard float with the given value.\nSucceeds if the condition is met")]
	[NodePath(DECORATOR_NODE_PATH + "Compare Blackboard Floats")]
	public class CompareBlackboardFloats : ConditionalCheckNode
	{
		public BlackboardKey FloatBlackboardKey;

		[SerializeField] private float _value;
		[SerializeField,Tooltip("Greater than is blackboardValue > _value")] private ComparisonMode _comparisonMode = ComparisonMode.Equals;

		protected override bool IsTrue()
		{
            if (!Blackboard.HasValue(FloatBlackboardKey))
            {
                Debug.LogError("Blackboard key does not exist");
                return false;
            }

            float blackboardValue = Blackboard.GetValue<float>(FloatBlackboardKey);

            // condition
            switch (_comparisonMode)
            {
                case ComparisonMode.None:
                    return false;
                case ComparisonMode.Equals:
                    return blackboardValue == _value;
                case ComparisonMode.NotEquals:
                    return blackboardValue != _value;
                case ComparisonMode.GreaterThan:
                    return blackboardValue > _value;
                case ComparisonMode.LessThan:
                    return blackboardValue < _value;
            }
            return true;
		}
	}
}