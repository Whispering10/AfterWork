using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class StartAttack : BehaviorAction
{
    private Transform transform;
    private ActiveWeapon activeWeapon; 
    private AttackAbility attackAbility;
    private MoveAbility moveAbility;
    private LookAbility view;

    private Animator animator;

    private Transform target;

    public override NodeState Evaluate()
    {
        Vector3 deltaPosition = target.position - transform.position;
        Vector3 lookDirection = deltaPosition.normalized;
        moveAbility.Direction = 0.0f;
        attackAbility.Attack(activeWeapon.Weapon, lookDirection);

        view.ChangeDirectionLook(lookDirection);

        animator.SetBool("Run", false);
        animator.SetBool("Attack", true);

        return NodeState.Success;
    }

    private void OnChangeVisibility(object s, VisibilityChangedEventArgs e)
    {
        target = e.Target;
    }

    public StartAttack(Transform transform, ActiveWeapon activeWeapon,
        AttackAbility attackAbility, MoveAbility moveAbility, LookAbility view, DetectorPlayerInCircle detector, Animator animator)
    {
        this.transform = transform;
        this.activeWeapon = activeWeapon;
        this.attackAbility = attackAbility;
        this.moveAbility = moveAbility;
        this.view = view;
        this.animator = animator;

        target = detector.PlayerTransform;

        SubscribeToEvent(() => detector.playerVisibilityChanged -= OnChangeVisibility);
        detector.playerVisibilityChanged += OnChangeVisibility;
    }
}
