using Runtime;
using UnityEngine;

public class PlayerInitializer : Initializer
{
    [Header("AutoAdd")]
    [SerializeField] private MoveAbility moveAbility;
    [SerializeField] private JumpAbility jumpAbility;
    [SerializeField] private TimeSlowAbility timeSlowAbility;
    [SerializeField] private StaminaController staminaController;
    [SerializeField] private DashAbility dashAbility;
    [SerializeField] private ActiveWeapon activeWeapon;
    [SerializeField] private AttackAbility attackAbility;
    [SerializeField] private HealthController healthController;
    [SerializeField] private AttackHandler attackHandler;
    [SerializeField] private Player player;

    [Header("NotAutoAdd")]
    [SerializeField] private CircleTimer circleTimer;
    [SerializeField] private GameObject look;

    public override void Init(CMSEntity model)
    {
        moveAbility = GetComponent<MoveAbility>();
        jumpAbility = GetComponent<JumpAbility>();
        timeSlowAbility = GetComponent<TimeSlowAbility>();
        staminaController = GetComponent<StaminaController>();
        dashAbility = GetComponent<DashAbility>();
        activeWeapon = GetComponent<ActiveWeapon>();
        attackAbility = GetComponent<AttackAbility>();
        healthController = GetComponent<HealthController>();
        attackHandler = GetComponent<AttackHandler>();
        player = GetComponent<Player>();

        moveAbility.Init(model.Get<TagSpeed>());
        jumpAbility.Init(model.Get<TagJump>());
        timeSlowAbility.Init(model.Get<TagTimeSlow>());
        staminaController.Init(model.Get<TagStamina>());
        dashAbility.Init(model.Get<TagDash>(), staminaController);
        activeWeapon.Init(CMS.Get<CMSEntity>(model.Get<TagWeapon>().Weapon.GetId()));
        attackAbility.Init();
        healthController.Init(model.Get<TagHealth>());
        attackHandler.Init(look.GetComponent<LookAbility>(), healthController); //?
        circleTimer.Init();
        player.Init();
    }
}
