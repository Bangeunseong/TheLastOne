using System;
using UnityEngine;

namespace _1.Scripts.Weapon.Scripts
{
    [Serializable] public class WeaponStat
    {
        [field: Header("Weapon SetUp")]
        [field: SerializeField] public int MaxAmmoCount { get; private set; }
        [field: SerializeField] public float MaxWeaponRange { get; private set; }
        [field: SerializeField] public float BulletSpeed { get; private set; }
        [field: SerializeField] public float Recoil { get; private set; }
        [field: SerializeField] public float Accuracy { get; private set; }
        [field: SerializeField] public float ReloadTime { get; private set; }
        [field: SerializeField] public float Rpm { get; private set; }
        [field: SerializeField] public int Damage { get; private set; }
        
        [field: Header("Knockback SetUp")]
        [field: SerializeField] public bool IsKnockbackAvailable { get; private set; }
        
        [field: Header("Bullet Prefab Id")]
        [field: SerializeField] public string BulletPrefabId { get; private set; }
    }
    
    [CreateAssetMenu(fileName = "New WeaponData", menuName = "ScriptableObjects/Weapon", order = 0)]
    public class WeaponData : ScriptableObject
    {
        [field: Header("Weapon Settings")]
        [field: SerializeField] public WeaponStat WeaponStat { get; private set; }
    }
}