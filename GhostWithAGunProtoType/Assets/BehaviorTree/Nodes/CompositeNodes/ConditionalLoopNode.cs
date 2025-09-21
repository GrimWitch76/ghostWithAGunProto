using UnityEngine;

namespace BT.Nodes
{
	using Utilities;

	public abstract class ConditionalLoopNode : DecoratorNode
    {
        [SerializeField] private LoopMode _loopMode = LoopMode.Finite;
        [SerializeField] private int _loopsToComplete = 3;
        [Tooltip("The state returned should the condition not be met.")]
        [SerializeField] private NodeState _returnState = NodeState.Failure;

        private int _loopsCompleted;

        /// <summary>
        /// When false, the loop breaks.
        /// </summary>
        /// <returns></returns>
        protected abstract bool IsConditionBroken();

		protected override NodeState OnStart()
		{
			_loopsToComplete = Mathf.Max(1, _loopsToComplete);

			_loopsCompleted = 0;

            return NodeState.Running;
        }

		protected sealed override NodeState OnUpdate()
		{
            if (!IsConditionBroken())
            {
                Child.Interrupt();
                return _returnState;
            }

            NodeState childState = Child.Update();

            if (_loopMode == LoopMode.Finite)
            {
                if (childState is NodeState.Failure or NodeState.Success or NodeState.Interrupted)
                {
                    _loopsCompleted++;
                }

                if (_loopsCompleted >= _loopsToComplete)
                {
                    return NodeState.Success;
                }
            }

            return NodeState.Running;
        }

		protected override void OnExit()
		{

		}
	}
}
