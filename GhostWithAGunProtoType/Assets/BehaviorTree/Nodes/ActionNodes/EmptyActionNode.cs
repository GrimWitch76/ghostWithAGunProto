using UnityEngine;

namespace BT.Nodes
{

    [NodePath(ACTION_NODE_PATH + "Empty Action Node")]
    public class EmptyActionNode : ActionNode
    {
        [SerializeField] private NodeState _nodeState = NodeState.Success;

        protected override NodeState OnStart()
        {
            return _nodeState;
        }

        protected override NodeState OnUpdate()
        {
            return _nodeState;
        }

        protected override void OnExit()
        {

        }
    }
}