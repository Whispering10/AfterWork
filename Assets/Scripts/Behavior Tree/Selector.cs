using System.Collections.Generic;

public class Selector : Node
{
    public List<Node> children = new List<Node>();

    public override NodeState Evaluate()
    {
        foreach (Node child in children)
        {
            NodeState result = child.Evaluate();
            if (result == NodeState.Success)
            {
                state = NodeState.Success;
                return NodeState.Success;
            }
            else if (result == NodeState.Running)
            {
                state = NodeState.Running;
                return NodeState.Running;
            }
        }
        state = NodeState.Failure;
        return NodeState.Failure;
    }

    public Selector(params Node[] children)
    {
        this.children = new List<Node>(children);
    }
}
