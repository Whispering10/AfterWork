using UnityEngine;

public class Idle : BehaviorAction
{
    private MoveAbility moveAbility;

    private Animator animator;

    public override NodeState Evaluate()
    {
        moveAbility.Direction = 0.0f;

        animator.SetBool("Run", false);
        animator.SetBool("Attack", false);

        return NodeState.Success;
    }

    public Idle(MoveAbility moveAbility, Animator animator)
    {
        this.moveAbility = moveAbility;
        this.animator = animator;
    }
}
