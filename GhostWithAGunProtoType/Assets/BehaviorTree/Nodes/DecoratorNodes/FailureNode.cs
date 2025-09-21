namespace BT.Nodes
{
    [NodePath(DECORATOR_NODE_PATH + "Failure Node")]
    public class FailureNode : DecoratorNode
    {
        protected override NodeState OnUpdate()
        {
            return NodeState.Failure;
        }
    }
}
