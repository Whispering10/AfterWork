using UnityEngine;

public class IsPlayerInAttackRange : BehaviorCondition
{
    private float attackRange = 0.0f;
    private Transform transform;

    private Transform player;

    public override NodeState Evaluate()
    {
        if (transform == null || player == null) return NodeState.Failure;

        return Vector3.Distance(transform.position, player.position) <= attackRange
            ? NodeState.Success : NodeState.Failure;
    }

    private void OnChangeVisibility(object s, VisibilityChangedEventArgs e)
    {
        player = e.Target;
    }

    public IsPlayerInAttackRange(Transform transform, ActiveWeapon activeWeapon, DetectorPlayerInCircle detector)
    {
        this.transform = transform;
        attackRange = activeWeapon.Weapon.WeaponStats.Length * 0.8f;

        player = detector.PlayerTransform;

        SubscribeToEvent(() => detector.playerVisibilityChanged -= OnChangeVisibility);
        detector.playerVisibilityChanged += OnChangeVisibility;
    }
}
