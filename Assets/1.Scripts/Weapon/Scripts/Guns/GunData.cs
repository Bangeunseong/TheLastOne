using System;
using _1.Scripts.Weapon.Scripts.Common;
using UnityEngine;

namespace _1.Scripts.Weapon.Scripts.Guns
{
    [Serializable] public class GunStat : WeaponStat
    {
        [field: Header("Gun Settings")]
        [field: SerializeField] public GunType Type { get; private set; }
        [field: SerializeField] public int MaxAmmoCountInMagazine { get; private set; }
        [field: SerializeField] public float BulletSpeed { get; private set; }
        [field: SerializeField] public float Accuracy { get; private set; }
        [field: SerializeField] public float ReloadTime { get; private set; }
        [field: SerializeField] public float Rpm { get; private set; }
        
        [field: Header("Bullet Prefab Id")]
        [field: SerializeField] public string BulletPrefabId { get; private set; }
    }
    
    [CreateAssetMenu(fileName = "New GunData", menuName = "ScriptableObjects/Gun", order = 0)]
    public class GunData : ScriptableObject
    {
        [field: SerializeField] public GunStat GunStat { get; private set; }
    }
}