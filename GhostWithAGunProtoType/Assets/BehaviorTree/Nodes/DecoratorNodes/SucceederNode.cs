namespace BT.Nodes
{
    [NodePath(DECORATOR_NODE_PATH + "Succeeder Node")]
    public class SucceederNode : DecoratorNode
    {
        protected override NodeState OnUpdate()
        {
            return NodeState.Success;
        }
    }
}
