using Runtime;
using System;

[Serializable]
public class TagWeapon : EntityComponentDefinition
{
    [FilterTags(typeof(TagWeaponStats))]
    public CMSEntityPfb Weapon;
}
