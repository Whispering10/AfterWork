using System.Collections.Generic;

public class Sequence : Node
{
    public List<Node> children = new List<Node>();

    public override NodeState Evaluate()
    {
        foreach (Node child in children)
        {
            NodeState result = child.Evaluate();
            if (result == NodeState.Failure)
            {
                state = NodeState.Failure;
                return NodeState.Failure;
            }
            else if (result == NodeState.Running)
            {
                state = NodeState.Running;
                return NodeState.Running;
            }
        }
        state = NodeState.Success;
        return NodeState.Success;
    }

    public Sequence(params Node[] children)
    {
        this.children = new List<Node>(children);
    }
}
