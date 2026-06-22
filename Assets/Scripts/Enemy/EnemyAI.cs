using System;
using System.Collections;
using UnityEngine;

public class EnemyAI : MonoBehaviour
{
    [SerializeField] private MoveAbility moveAbility;
    [SerializeField] private DetectorPlayerInCircle detector;
    [SerializeField] private LookAbility lookAbility;
    [SerializeField] private ActiveWeapon activeWeapon;
    [SerializeField] private AttackAbility attackAbility;
    [SerializeField] private HealthController healthController;
    [SerializeField] private AttackHandler attackHandler;
    [SerializeField] private Animator animator;
    [SerializeField] private SpriteRenderer spriteRenderer;

    [Header("Stealth Detection")]
    [SerializeField] private GameObject playerObject;

    private Node tree;
    private StealthCoverHandler stealthHandler;

    private void Awake()
    {
        enabled = false;
    }

    public void Init()
    {
        moveAbility = GetComponent<MoveAbility>();
        attackAbility = GetComponent<AttackAbility>();
        activeWeapon = GetComponent<ActiveWeapon>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        healthController = GetComponent<HealthController>();
        attackHandler = GetComponent<AttackHandler>();
        animator = GetComponent<Animator>();

        healthController.HealthIsNull += Dead;

        // Получаем компонент скрытности игрока
        if (playerObject == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
                playerObject = playerObj;
        }
        if (playerObject != null)
        {
            stealthHandler = playerObject.GetComponent<StealthCoverHandler>();
            if (stealthHandler == null)
                Debug.LogWarning("StealthCoverHandler not found on player!");
        }
        else
        {
            Debug.LogWarning("Player not assigned or found!");
        }

        CreateBehaviourTree();
        enabled = true;
    }

    private void CreateBehaviourTree()
    {
        tree = new Selector(
            new Sequence(
                new IsAttacking(activeWeapon)
            ),
            new Sequence(
                new IsPlayerVisibleWithStealth(detector, stealthHandler), // ← новый узел
                new Selector(
                    new Sequence(
                        new IsPlayerInAttackRange(transform, activeWeapon, detector),
                        new StartAttack(transform, activeWeapon, attackAbility, moveAbility, lookAbility, detector, animator)
                    ),
                    new Pursue(transform, moveAbility, lookAbility, detector, animator)
                )
            ),
            new Idle(moveAbility, animator)
        );
    }

    void Update()
    {
        if (tree != null)
            tree.Evaluate();
    }

    private void Dead(object s, EventArgs e)
    {
        StartCoroutine(Dying());
    }

    private IEnumerator Dying()
    {
        enabled = false;
        animator.SetTrigger("Death");
        yield return new WaitForSeconds(1.0f);
        Destroy(gameObject);
    }

    // ========== НОВЫЙ УЗЕЛ (исправленный) ==========
    public class IsPlayerVisibleWithStealth : Node
    {
        private IsPlayerVisible originalNode;
        private StealthCoverHandler stealth;

        public IsPlayerVisibleWithStealth(DetectorPlayerInCircle detector, StealthCoverHandler stealth)
        {
            originalNode = new IsPlayerVisible(detector);
            this.stealth = stealth;
        }

        public override NodeState Evaluate()
        {
            // Если игрок скрыт – невидим
            if (stealth != null && stealth.IsHidden)
                return NodeState.Failure;

            // Иначе используем оригинальную логику видимости
            return originalNode.Evaluate();
        }
    }
}