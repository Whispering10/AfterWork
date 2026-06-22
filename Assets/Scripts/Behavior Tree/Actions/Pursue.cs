using UnityEngine;

public class Pursue : BehaviorAction
{
    private Transform transform;
    private MoveAbility moveAbility;
    private LookAbility view;

    private Animator animator;

    private Transform target;

    public override NodeState Evaluate()
    {
        Vector3 deltaPosition = target.position - transform.position;
        Vector3 lookDirection = deltaPosition.normalized;

        if (deltaPosition.x > 0)
        {
            moveAbility.Direction = 1.0f;
            transform.localScale = new Vector3(1, 1, 1);
        }
        else
        {
            moveAbility.Direction = -1.0f;
            transform.localScale = new Vector3(-1, 1, 1);
        }

        view.ChangeDirectionLook(lookDirection);

        animator.SetBool("Run", true);

        return NodeState.Success;
    }

    private void OnChangeVisibility(object s, VisibilityChangedEventArgs e)
    {
        target = e.Target;
    }

    public Pursue(Transform transform, MoveAbility moveAbility, LookAbility view, DetectorPlayerInCircle detector, Animator animator)
    {
        this.transform = transform;
        this.moveAbility = moveAbility;
        this.view = view;
        this.animator = animator;

        target = detector.PlayerTransform;

        SubscribeToEvent(() => detector.playerVisibilityChanged -= OnChangeVisibility);
        detector.playerVisibilityChanged += OnChangeVisibility;
    }
}