using System.Collections.Generic;

namespace BT.Nodes
{
    public abstract class ActionNode : BaseNode
    {
        protected const string ACTION_NODE_PATH = MAIN_NODE_PATH + "Action Nodes/";

        public override void OnAwake()
        {

        }

        protected override NodeState OnUpdate()
        {
            return OnStart();
        }

        public override List<BaseNode> GetChildren()
        {
            base.GetChildren();
            return new List<BaseNode>();
        }

        public override bool HasChildren()
        {
            return false;
        }

        public override void OnForceStopNode()
        {
            base.OnForceStopNode();

            State = NodeState.Failure;
        }

        public override void Interrupt()
        {
            base.Interrupt();

            OnExit();
        }
    }
}
