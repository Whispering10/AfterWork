using UnityEngine;

public class WeaponInitializer : Initializer
{
    [Header("AutoAdd")]
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private BoxCollider2D boxCollider;
    [SerializeField] private Weapon weapon;

    public override void Init(CMSEntity weaponModel)
    {
        weapon.Init(weaponModel.Get<TagWeaponStats>(), boxCollider);
    }
}
