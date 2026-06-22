using System;
using UnityEngine;

public class AttackHitEventsArgs : EventArgs
{
    public GameObject Target { get; }

    public AttackHitEventsArgs(GameObject target)
    {
        Target = target;
    }
}

public class Attack : MonoBehaviour
{
    public event EventHandler<AttackHitEventsArgs> AttackHit;

    private Collider2D attackCollider;
    private float damage = 0;

    public float Damage
    {
        get => damage;
        set => damage = value;
    }

    private void Awake()
    {
        enabled = false;
    }

    public void Init(Collider2D c)
    {
        attackCollider = c;
        attackCollider.enabled = false;
        enabled = true;
    }

    private void OnEnable()
    {
        if (attackCollider != null)
            attackCollider.enabled = true;
    }

    private void OnDisable()
    {
        if (attackCollider == null) return;
        attackCollider.enabled = false;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject == gameObject) return;

        // === РАЗРУШЕНИЕ ТАЙЛОВ (добавлено) ===
        BreakableTilemap breakable = collision.GetComponent<BreakableTilemap>();
        if (breakable != null)
        {
            // Используем ближайшую точку столкновения для точного попадания
            Vector3 contactPoint = collision.ClosestPoint(transform.position);
            if (breakable.DestroyTileAtWorldPos(contactPoint))
            {
                Debug.Log("Атака разрушила тайл!");
            }
            // Не прерываем выполнение, чтобы также обработать возможный AttackHandler (если он есть на тайле – маловероятно)
        }

        // === СУЩЕСТВУЮЩАЯ ЛОГИКА УРОНА ===
        if (collision.gameObject.TryGetComponent<AttackHandler>(out AttackHandler attackHandler))
        {
            attackHandler.TryProcessAttack(damage, transform);
            AttackHit?.Invoke(this, new AttackHitEventsArgs(collision.gameObject));
        }
    }
}