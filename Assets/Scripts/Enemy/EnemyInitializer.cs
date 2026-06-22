using Unity.VisualScripting;
using UnityEngine;

public class EnemyInitializer : Initializer
{
    [Header("AutoAdd")]
    [SerializeField] private MoveAbility moveAbility;
    [SerializeField] private LookAbility lookAbility;
    [SerializeField] private AttackAbility attackAbility;
    [SerializeField] private ActiveWeapon activeWeapon;
    [SerializeField] private HealthController healthController;
    [SerializeField] private AttackHandler attackHandler;
    [SerializeField] private EnemyAI enemyAI;

    [Header("NotAutoAdd")]
    [SerializeField] private DetectorPlayerInCircle detector;

    public override void Init(CMSEntity model)
    {
        moveAbility = GetComponent<MoveAbility>();
        healthController = GetComponent<HealthController>();
        attackHandler = GetComponent<AttackHandler>();
        attackAbility = GetComponent<AttackAbility>();
        activeWeapon = GetComponent<ActiveWeapon>();
        lookAbility = GetComponent<LookAbility>();
        enemyAI = GetComponent<EnemyAI>();

        moveAbility.Init(model.Get<TagSpeed>());
        attackAbility.Init();
        activeWeapon.Init(CMS.Get<CMSEntity>(model.Get<TagWeapon>().Weapon.GetId()));
        healthController.Init(model.Get<TagHealth>());
        attackHandler.Init(lookAbility, healthController);
        enemyAI.Init();
    }
}
