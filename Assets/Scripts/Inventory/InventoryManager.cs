using NUnit.Framework;
using NUnit.Framework.Internal;
using System;
using System.Collections.Generic;
using UnityEngine;

public class InventoryManager : MonoBehaviour
{
    private List<Weapon> weapons = new List<Weapon>();
    private ActiveWeapon activeWeapon;

    private void Awake()
    {
        activeWeapon = gameObject.AddComponent<ActiveWeapon>();
    }

    private bool HasWeaponType(Weapon weapon)
    {
        Type weaponType = weapon.GetType();
        return weapons.Exists(w => w.GetType() == weaponType);
    }

    public bool AddWeapon(GameObject weaponCandidate)
    {
        if (weaponCandidate == null) return false;

        if (!weaponCandidate.TryGetComponent<Weapon>(out Weapon weapon)) return false;

        if (HasWeaponType(weapon)) return false;

        GameObject weaponInstance = Instantiate(weaponCandidate);
        weaponInstance.name = weapon.GetType().Name;

        weapons.Add(weaponInstance.GetComponent<Weapon>());

        return true;
    }

    public void OnAddWeapon(GameObject weaponCandidate)
    {
        AddWeapon(weaponCandidate);
    }
}
