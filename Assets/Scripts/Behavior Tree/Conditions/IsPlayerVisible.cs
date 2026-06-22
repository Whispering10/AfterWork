using UnityEngine;

public class IsPlayerVisible : BehaviorCondition
{
    private bool isPlayerVisible = false;

    public override NodeState Evaluate()
    {
        return isPlayerVisible ? NodeState.Success : NodeState.Failure;
    }

    private void OnChangeVisibility(object s, VisibilityChangedEventArgs e)
    {
        isPlayerVisible = e.IsVisible;
    }

    public IsPlayerVisible(DetectorPlayerInCircle detector)
    {
        isPlayerVisible = detector.IsPlayerInSight;

        SubscribeToEvent(() => detector.playerVisibilityChanged -= OnChangeVisibility);
        detector.playerVisibilityChanged += OnChangeVisibility;
    }
}
