using System;
using UnityEngine;

[Serializable]
public class TagHealth : EntityComponentDefinition
{
    public bool IsEndless = false;
    public int MaxHealth;
}
