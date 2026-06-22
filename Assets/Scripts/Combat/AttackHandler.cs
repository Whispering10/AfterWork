using System;
using UnityEngine;

public class AttackHandler : MonoBehaviour
{
    private HealthController healthController;

    private bool attacked = false;
    private Vector2 viewDirection = Vector2.zero;

    public event EventHandler<ValueChangedEventArgs<bool>> hasBeenAttacked;

    private void Awake()
    {
        enabled = false;
    }
    public void Init(LookAbility lookAbility, HealthController healthController)
    {
        this.healthController = healthController;

        viewDirection = lookAbility.Direction;
        lookAbility.directionChanged += OnChangeDirectionLook;

        enabled = true;
    }

    public void TryProcessAttack(float damage, Transform attackTransform)
    {
        float scal = Vector2.Dot(viewDirection, (attackTransform.position - transform.position).normalized);
        float angleThreshold = Mathf.Cos(22.5f * Mathf.Deg2Rad);
        if (scal >= angleThreshold)
        {
            Debug.Log("Parry");
        }

        healthController?.TryConsume(damage);
        attacked = true;

        hasBeenAttacked?.Invoke(this, new ValueChangedEventArgs<bool>(true));
    }

    private void OnChangeDirectionLook(object s, ValueChangedEventArgs<Vector2> e)
    {
        viewDirection = e.Value;
    }
}