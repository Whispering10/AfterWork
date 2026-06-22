using UnityEngine;

public class Dying : BehaviorAction
{
    private Animator animator;

    public override NodeState Evaluate()
    {
        animator.SetTrigger("Death");

        return NodeState.Success;
    }

    public Dying(Animator animator)
    {
        this.animator = animator;
    }
}
