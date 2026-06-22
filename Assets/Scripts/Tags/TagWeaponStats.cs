using System;
using UnityEngine;

[Serializable]
public class TagWeaponStats : EntityComponentDefinition
{
    public float Damage;
    public float Length;
    public float Width;
    public float AttackDuration;
    public Vector3 attackStages;

    public Vector3 AttackStages
    {
        get
        {
            float sum = attackStages.x + attackStages.y + attackStages.z;
            float x = attackStages.x / sum * AttackDuration;
            float y = attackStages.y / sum * AttackDuration;
            float z = attackStages.z / sum * AttackDuration;
            return new Vector3(x, y, z);
        }
    }
}
