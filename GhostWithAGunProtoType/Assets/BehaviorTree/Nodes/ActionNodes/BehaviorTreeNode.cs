using UnityEngine;

namespace BT.Nodes
{
    [NodePath(ACTION_NODE_PATH + "Behavior Tree Node")]
    public class BehaviorTreeNode : ActionNode
	{
		[SerializeField] private BehaviorTree _behaviorTree;
        [HideInInspector] public BehaviorTree BehaviorTreeInstance;
		[HideInInspector]
		public RootNode Root;

        public override void OnAwake()
        {
            base.OnAwake();

            //BehaviorTreeNode btNodeClone = Instantiate(this);
            BehaviorTreeInstance = _behaviorTree.Clone();
            BehaviorTreeInstance.Bind(Agent);
            BehaviorTreeInstance.AwakeBT();
            Root = BehaviorTreeInstance.RootNode as RootNode;
        }

        protected override NodeState OnStart()
		{
			return NodeState.Running;
		}

		protected override NodeState OnUpdate()
		{
			NodeState state = Root.Update();
			return state;
		}

		protected override void OnExit()
		{

		}

        public override void Interrupt()
        {
            base.Interrupt();

            Root.Interrupt();
        }

        public BehaviorTree GetBehaviorTreeReference()
        {
			return _behaviorTree;
        }
	}
}