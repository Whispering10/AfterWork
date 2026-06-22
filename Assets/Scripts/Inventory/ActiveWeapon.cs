using System;
using UnityEngine;

public class ActiveWeapon : MonoBehaviour
{
    private Weapon weapon;
    private GameObject weaponObject;

    public Weapon Weapon 
    {
        get
        {
            return weapon;
        }
    }
    public GameObject WeaponObject
    {
        get
        {
            return weaponObject;
        }
    }

    private void Awake()
    {
        enabled = false;
    }
    public void Init(CMSEntity weaponModel)
    {
        GameObject weaponObject = Factory.Create(Resources.Load<GameObject>("CMS/Prefabs/GameObjects/Weapon"), weaponModel);
        weapon = weaponObject.GetComponent<Weapon>();
        weaponObject.transform.parent = transform;
        weaponObject.transform.localPosition = Vector3.zero;

        enabled = true;
    }

    public bool SetWeapon(CMSEntity weaponModel)
    {
        TagWeaponStats stats = weaponModel.Get<TagWeaponStats>();
        if (stats == null) return false;

        weapon.WeaponStats = stats;
        return true;
    }
}
