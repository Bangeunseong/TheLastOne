using System;
using _1.Scripts.Weapon.Scripts.Common;
using _1.Scripts.Weapon.Scripts.Guns;
using UnityEngine;

namespace _1.Scripts.Weapon.Scripts.Grenade
{
    [Serializable] public class GrenadeStat : WeaponStat
    {
        [field: Header("Grenade Settings")]
        [field: SerializeField] public WeaponType Type { get; private set; }
        [field: SerializeField] public int MaxAmmoCountInMagazine { get; private set; }
        [field: SerializeField] public float Accuracy { get; private set; }
        [field: SerializeField] public float ReloadTime { get; private set; }
        [field: SerializeField] public float ThrowForce { get; private set; }
        [field: SerializeField] public float Force { get; private set; } // 던지는 힘
        [field: SerializeField] public float Radius { get; private set; }
        [field: SerializeField] public float Delay { get; private set; } // 수류탄이 터지기 전 딜레이
        [field: SerializeField] public float Rpm { get; private set; } // 다음 수류탄을 던지는데 걸리는 딜레이
        
        [field: Header("Grenade Prefab Id")]
        [field: SerializeField] public string GrenadePrefabId { get; private set; }
    }
    
    [CreateAssetMenu(fileName = "New GrenadeData", menuName = "ScriptableObjects/Grenade", order = 0)]
    public class GrenadeData : ScriptableObject
    {
        [field: SerializeField] public GrenadeStat GrenadeStat { get; private set; }
    }
}