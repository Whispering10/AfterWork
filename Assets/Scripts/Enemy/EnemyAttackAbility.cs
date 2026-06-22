using UnityEngine;

public class EnemyAttackAbility : MonoBehaviour
{
    [SerializeField] private float attackRange;
    [SerializeField] private float delayAttack;
    [SerializeField] private float cooldownAttack;
    [SerializeField] private float damage;

    GameObject attackObject;
    EnemyAttack attack;
    private float attackTime = 0.0f;
    private bool isAttacking = false;

    public float AttackRange
    {
        get
        {
            return attackRange;
        }
    }

    private void Awake()
    {
        attackObject = new GameObject();
        attackObject.transform.parent = transform;
        attackObject.name = "AttackObject";
        attack = attackObject.AddComponent<EnemyAttack>();
        attack.Damage = damage;
        attackObject.SetActive(false);

        attack.AttackHit += OnAttackHit;
    }

    private void Update()
    {
        if (isAttacking)
        {
            attackTime += Time.deltaTime;
            if (attackTime >= delayAttack)
            {
                attackObject.SetActive(true);
                isAttacking = false;
            }
        }
    }

    public void Attack(Transform target)
    {
        isAttacking = true;

        if ((transform.position - target.position).magnitude > attackRange) return;

        if (target.TryGetComponent<AttackHandler>(out AttackHandler handler))
        {
            handler.TryProcessAttack(damage, transform);
        }
    }

    private void OnAttackHit(object o, AttackHitEventsArgs e)
    {
        attack.enabled = false;
        Debug.Log("HitOnPlayer");
        
    }
}
