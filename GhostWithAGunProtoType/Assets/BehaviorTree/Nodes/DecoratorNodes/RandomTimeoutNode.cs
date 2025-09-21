using UnityEngine;

namespace BT.Nodes
{
    using Utilities;

    [NodePath(DECORATOR_NODE_PATH + "Random Timeout Node")]
    public class RandomTimeoutNode : DecoratorNode
    {
        [SerializeField] private TimeMode _timeMode = TimeMode.DeltaTime;
        [SerializeField] private InterruptionMode _interruptionMode = InterruptionMode.Reactive;
        [SerializeField] private Vector2 _timeoutDurationRange = new Vector2(0.5f, 2.0f);

        private float _timeoutDuration;
        private float _timeElapsed;

        public override void OnAwake()
        {
            base.OnAwake();

            if (_timeoutDurationRange.x < 0f || _timeoutDurationRange.y < 0f)
            {
                _timeoutDurationRange = Vector2.zero;
                Debug.LogError($"Timeout duration at node {ViewDetails.Name} cannot be negative. Setting it to 0.");
            }
        }

        protected override NodeState OnStart()
        {
            base.OnStart();

            _timeoutDuration = Random.Range(_timeoutDurationRange.x, _timeoutDurationRange.y);

            _timeElapsed = 0.0f;

            return NodeState.Running;
        }

        protected override NodeState OnUpdate()
        {
            NodeState childState = Child.Update();

            if (_timeElapsed >= _timeoutDuration)
            {
                return GetStateOnTimeout(childState);
            }

            UpdateTimer();

            if(_interruptionMode == InterruptionMode.Reactive)
            {
                return childState;
            }
            else
            {
                return NodeState.Running;
            }
        }

        private NodeState GetStateOnTimeout(NodeState childState)
        {
            return NodeState.Success;
        }

        private void UpdateTimer()
        {
            if (_timeMode == TimeMode.DeltaTime)
            {
                UpdateTimerWithDeltaTime();
            }
            else
            {
                UpdateTimerWithUnscaledDeltaTime();
            }
        }

        private void UpdateTimerWithUnscaledDeltaTime()
        {
            _timeElapsed += Time.unscaledDeltaTime;
        }

        private void UpdateTimerWithDeltaTime()
        {
            _timeElapsed += Time.deltaTime;
        }
    }
}
