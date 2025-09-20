using UnityEngine;

namespace BT
{
    public class BehaviorTreeRunner : MonoBehaviour
    {
        public BehaviorTree Tree;

        [SerializeField] private bool _runOnStart = false;
        [SerializeField] private MonoBehaviour _agent;

        private bool _isRunning = false;
        private bool _isPaused = false;

        private void Start()
        {
            if (_runOnStart)
            {
                Run(_agent);
            }
        }

        public void Run(MonoBehaviour agent = null)
        {
            if (_agent == null && agent == null)
            {
                Debug.LogError($"No agent found on {gameObject.name}");
                return;
            }

            if (agent == null)
            {
                agent = _agent;
            }

            if (!Tree.IsInitiated)
            {
                string name = Tree.name;
                Tree = Tree.Clone();
                Tree.name = name + " Cloned";
                Tree.Bind(agent);
                Tree.AwakeBT();
            }

            Tree.StartBT();
            _isRunning = true;
        }

        public void Stop()
        {
            _isRunning = false;
            Tree.StopBT();
        }

        private void Update()
        {
            if (!_isRunning || _isPaused)
            {
                return;
            }

            Tree.UpdateBT();
        }

        public void Pause()
        {
            _isPaused = true;
        }

        public void Resume()
        {
            _isPaused = false;
        }
    }
}