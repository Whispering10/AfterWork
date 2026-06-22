using UnityEngine;

public static class PrefabPaths
{
    public const string WeaponPrefab = "Prefabs/Weapon";
    public const string EnemyPrefab = "Prefabs/Enemy";

    public static string[] GetAllPaths()
    {
        return new string[] { WeaponPrefab, EnemyPrefab };
    }
}
