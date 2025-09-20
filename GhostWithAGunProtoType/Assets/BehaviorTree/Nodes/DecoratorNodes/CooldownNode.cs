using UnityEngine;
using System.Collections;

namespace BT.Nodes
{
    using Utilities;

    [NodePath(DECORATOR_NODE_PATH + "Cooldown Node")]
    public class CooldownNode : DecoratorNode
    {
        [SerializeField] private TimeMode _timeMode = TimeMode.DeltaTime;
        [SerializeField] private float _timeoutDuration = 1.0f;

        private bool _isOnCooldown = false;

        protected override NodeState OnUpdate()
        {
            if (_isOnCooldown)
            {
                return NodeState.Failure;
            }

            NodeState childState = Child.Update();

            if(childState is NodeState.Success)
            {
                StartCooldown();
            }

            return childState;
        }

        private void StartCooldown()
        {
            _isOnCooldown = true;
            GetAgent<MonoBehaviour>().StartCoroutine(CooldownRoutine());
        }

        private IEnumerator CooldownRoutine()
        {
            float timeElapsed = 0f;

            while (timeElapsed < _timeoutDuration)
            {
                if (_timeMode == TimeMode.DeltaTime)
                {
                    timeElapsed += Time.deltaTime;
                }
                else
                {
                    timeElapsed += Time.unscaledDeltaTime;
                }

                yield return null;
            }

            _isOnCooldown = false;
        }
    }
}
