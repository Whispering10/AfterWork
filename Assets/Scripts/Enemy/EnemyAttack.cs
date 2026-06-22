using System;
using UnityEngine;

public class EnemyAttack : MonoBehaviour
{
    public event EventHandler<AttackHitEventsArgs> AttackHit;

    private Collider2D attackCollider;
    protected float damage = 0;

    public float Damage
    {
        get
        {
            return damage;
        }
        set
        {
            damage = value;
        }
    }

    private void Awake()
    {
        attackCollider = GetComponent<Collider2D>();
    }

    private void OnEnable()
    {
        attackCollider.enabled = true;
    }

    private void OnDisable()
    {
        attackCollider.enabled = false;
    }

    protected virtual void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject == gameObject) return;
        if (!collision.gameObject.CompareTag("Player")) return;
        if (collision.gameObject.TryGetComponent<AttackHandler>(out AttackHandler attackHandler))
        {
            attackHandler.TryProcessAttack(damage, transform);
            AttackHit?.Invoke(this, new AttackHitEventsArgs(collision.gameObject));
        }
    }
}
