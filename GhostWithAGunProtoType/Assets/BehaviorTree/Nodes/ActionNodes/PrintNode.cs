using UnityEngine;

namespace BT.Nodes
{
    [NodePath(ACTION_NODE_PATH + "Print Node")]
    public class PrintNode : ActionNode
    {
        public enum DebugType
        {
            Log = 0, LogWarning = 1, LogError = 2
        }

        [TextArea(2, 4)]
        [SerializeField] private string _message;
        [SerializeField] private DebugType _printType = DebugType.Log;
        [SerializeField] private bool _display = true;
        [SerializeField] private bool _displayTime = false;

        protected override NodeState OnStart()
        {
            return NodeState.Running;
        }

        protected override void OnExit()
        {

        }

        protected override NodeState OnUpdate()
        {
            Print();
            return NodeState.Success;
        }

        private void Print()
        {
            if (!_display)
            {
                return;
            }

            string message = $"{_message}" + (_displayTime ? Time.time.ToString() : "");

            switch (_printType)
            {
                case DebugType.Log:
                    Debug.Log(message);
                    break;
                case DebugType.LogWarning:
                    Debug.LogWarning(message);
                    break;
                case DebugType.LogError:
                    Debug.LogErrorFormat(message);
                    break;
            }
        }
    }
}
