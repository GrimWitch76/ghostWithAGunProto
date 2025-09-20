using UnityEngine;

namespace BT.Nodes
{
    [NodePath(ACTION_NODE_PATH + "Random Wait Node")]
    public class RandomWaitNode : ActionNode
    {
        public Vector2 DurationRange = new Vector2(1f, 2f);
        private float _currentDuration;
        private float _elapsedTime;

        protected override NodeState OnStart()
        {
            _elapsedTime = 0f;
            _currentDuration = Random.Range(DurationRange.x, DurationRange.y);

            return NodeState.Running;
        }

        protected override NodeState OnUpdate()
        {
            if (_elapsedTime < _currentDuration)
            {
                _elapsedTime += Time.deltaTime;
                return NodeState.Running;
            }

            return NodeState.Success;
        }

        protected override void OnExit()
        {

        }
    }
}