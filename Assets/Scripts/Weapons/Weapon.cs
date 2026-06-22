using System;
using UnityEngine;

public class Weapon : MonoBehaviour
{
    private TagWeaponStats weaponStats;
    private BoxCollider2D attackCollider;
    private Attack attack;
    private Vector3 attackStages;
    private bool isAttacking = false;
    private float attackTime = 0.0f;
    private bool hasAttacked = false;

    public event EventHandler<ValueChangedEventArgs<bool>> AttackingChanged;

    public bool IsAttacking
    {
        get
        {
            return isAttacking;
        }
        private set
        {
            isAttacking = value;
            AttackingChanged?.Invoke(this, new ValueChangedEventArgs<bool>(isAttacking));
        }
    }

    public TagWeaponStats WeaponStats
    {
        get
        {
            return weaponStats;
        }
        set
        {
            weaponStats = value;
            if (weaponStats == null)
            {
                gameObject.SetActive(false);
                return;
            }
            UpdateWeapon();
            gameObject.SetActive(true);
        }
    }

    private void Awake()
    {
        enabled = false;
    }
    public void Init(TagWeaponStats tagWeaponStats, BoxCollider2D boxCollider)
    {
        weaponStats = tagWeaponStats;
        attackCollider = boxCollider;

        attackCollider.size = Vector2.zero;
        attackCollider.isTrigger = true;

        attack = gameObject.AddComponent<Attack>();
        attack.Init(attackCollider);
        attack.enabled = false;
        attack.AttackHit += OnAttackHit;

        UpdateWeapon();

        enabled = true;
    }

    private void Update()
    {
        if (isAttacking)
        {
            attackTime += Time.deltaTime;

            if (attackTime >= weaponStats.AttackDuration)
            {
                attack.enabled = false;
                IsAttacking = false;

                return;
            }

            if (attackTime >= attackStages.x + attackStages.y)
            {
                attack.enabled = false;
            }
            else if (attackTime >= attackStages.x && !hasAttacked)
            {
                attack.enabled = true;
            }
        }
    }

    private void UpdateWeapon()
    {
        attack.Damage = weaponStats.Damage;
        attackStages = weaponStats.AttackStages;
        attackCollider.size = new Vector2(weaponStats.Width, weaponStats.Length);
        attackCollider.offset = new Vector2(0, weaponStats.Length / 2);
    }

    private void OnAttackHit(object sender, AttackHitEventsArgs e)
    {
        attack.enabled = false;      
        hasAttacked = true;
    }

    public bool Attack(Vector2 direction)
    {
        if (weaponStats == null) return false;
        if (isAttacking) return false;

        float newZ = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg - 90.0f;
        transform.eulerAngles = new Vector3(0, 0, newZ);

        hasAttacked = false;
        IsAttacking = true;
        attackTime = 0.0f;

        return true;
    }
}
