using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using static UnityEngine.Rendering.DebugUI;

public class HealthController : Controller
{
    private TagHealth tagHealth;

    public override float MaxValue { get { return tagHealth.MaxHealth; } }

    public EventHandler<EventArgs> HealthIsNull;

    public void Init(TagHealth tagHealth)
    {
        this.tagHealth = tagHealth;
        value = tagHealth.MaxHealth;

        enabled = true;
    }

    public override bool TryConsume(float amount)
    {
        if (tagHealth.IsEndless) return true;

        value -= amount;
        if (value <= 0)
        {
            HealthIsNull.Invoke(this, new EventArgs());
        }
        return true;
    }

    public void RestoreFullHealth()
    {
        value = MaxValue;
    }
}
