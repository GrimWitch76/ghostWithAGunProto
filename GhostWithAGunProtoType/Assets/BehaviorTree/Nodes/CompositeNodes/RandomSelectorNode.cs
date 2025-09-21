namespace BT.Nodes
{
    using Utilities;

    [NodePath(COMPOSITE_NODE_PATH + "Random Selector Node")]
    public class RandomSelectorNode : SelectorNode
    {
        protected override NodeState OnStart()
        {
            base.OnStart();

            BTHelper.ShuffleList(Children);

            return NodeState.Running;
        }
    }
}