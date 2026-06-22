using System;
using UnityEngine;

public class IDead : BehaviorCondition
{
    private bool iDead = false;

    public override NodeState Evaluate()
    {
        return iDead ? NodeState.Success : NodeState.Failure;
    }

    private void Dead(object s, EventArgs e)
    {
        iDead = true;
    }

    public IDead(HealthController healthController)
    {
        SubscribeToEvent(() => healthController.HealthIsNull -= Dead);
        healthController.HealthIsNull += Dead;
    }
}
