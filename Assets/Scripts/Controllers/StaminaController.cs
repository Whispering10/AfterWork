using UnityEngine;

public class StaminaController : Controller
{
    private TagStamina tagStamina;

    private bool isRegenActive = true;

    public override float MaxValue { get { return tagStamina.MaxValue; } }

    public void Init(TagStamina tagStamina)
    {
        this.tagStamina = tagStamina;
        value = tagStamina.MaxValue;

        enabled = true;
    }

    private void FixedUpdate()
    {
        if (isRegenActive)
        {
            if (value < tagStamina.MaxValue)
            {
                if (value + tagStamina.RegenRate * Time.fixedDeltaTime <= tagStamina.MaxValue)
                {
                    value += tagStamina.RegenRate * Time.fixedDeltaTime;
                }
                else
                {
                    value = tagStamina.MaxValue;
                }
            }            
        }
        else
        {
            isRegenActive = true;
        }         
    }

    public override bool TryConsume(float amount)
    {
        if (tagStamina.IsEndless) return true;

        isRegenActive = false;
        if (value <= amount) return false;
        value -= amount;
        return true;
    }
}
