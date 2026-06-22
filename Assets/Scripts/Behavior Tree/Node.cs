using UnityEngine;

public abstract class Node
{
    public enum NodeState
    {
        Success,
        Failure,
        Running
    }

    public NodeState state;

    public abstract NodeState Evaluate();

    public virtual string GetNodeName() => this.GetType().Name;
}
