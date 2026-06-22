using System;
using UnityEngine;
using static UnityEngine.RuleTile.TilingRuleOutput;

public class HasBeenAttacked : BehaviorCondition
{
    private bool hasBeenAttacked = false;

    public override NodeState Evaluate()
    {
        return hasBeenAttacked ? NodeState.Success : NodeState.Failure;
    }

    private void OnHasBeenAttacked(object o, ValueChangedEventArgs<bool> e)
    {
        hasBeenAttacked = e.Value;
    }

    public HasBeenAttacked(AttackHandler attackHandler)
    {
        SubscribeToEvent(() => attackHandler.hasBeenAttacked -= OnHasBeenAttacked);
        attackHandler.hasBeenAttacked += OnHasBeenAttacked;
    }
}
