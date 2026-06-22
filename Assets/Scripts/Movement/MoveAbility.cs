using UnityEngine;

public class MoveAbility : MonoBehaviour
{
    private MovementSystem movementSystem;
    protected TagSpeed tagSpeed;                  // »«Ã≈Õ≈ÕŒ: private -> protected
    private StealthCoverHandler stealthHandler;
    private float direction = 0.0f;

    public float Speed => tagSpeed.Speed;
    public float Direction { get => direction; set => direction = value; }

    private void Awake() { enabled = false; }

    public virtual void Init(TagSpeed tagSpeed)   // »«Ã≈Õ≈ÕŒ: ‰Ó·‡‚ÎÂÌ virtual
    {
        this.tagSpeed = tagSpeed;
        movementSystem = GetComponent<MovementSystem>();
        stealthHandler = GetComponent<StealthCoverHandler>();
        enabled = true;
    }

    private void FixedUpdate()
    {
        float currentSpeed = tagSpeed.Speed;
        if (stealthHandler != null && stealthHandler.IsActiveStealth)
            currentSpeed *= stealthHandler.StealthSpeedMultiplier;
        movementSystem.Move = direction * currentSpeed;
    }
}