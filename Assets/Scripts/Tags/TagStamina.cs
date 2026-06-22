using System;

[Serializable]
public class TagStamina : EntityComponentDefinition
{
    public bool IsEndless = false;
    public float MaxValue;
    public float RegenRate;
}
