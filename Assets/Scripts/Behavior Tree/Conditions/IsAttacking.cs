using UnityEngine;
using static UnityEngine.RuleTile.TilingRuleOutput;

public class IsAttacking : BehaviorCondition
{
    private bool isAttacking = false;

    public override NodeState Evaluate()
    {
        return isAttacking ? NodeState.Success : NodeState.Failure;
    }

    private void OnAttackingChanged(object o, ValueChangedEventArgs<bool> e)
    {
        isAttacking = e.Value;
    }

    public IsAttacking(ActiveWeapon activeWeapon)
    {
        isAttacking = activeWeapon.Weapon.IsAttacking;

        SubscribeToEvent(() => activeWeapon.Weapon.AttackingChanged -= OnAttackingChanged);
        activeWeapon.Weapon.AttackingChanged += OnAttackingChanged;
    }
}
