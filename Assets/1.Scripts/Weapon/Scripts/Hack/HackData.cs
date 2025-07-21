using System;
using _1.Scripts.Weapon.Scripts.Common;
using UnityEngine;

namespace _1.Scripts.Weapon.Scripts.Hack
{
    [Serializable] public class HackStat : WeaponStat
    {
        [field: Header("Hack Gun Settings")]
        [field: SerializeField] public WeaponType Type { get; private set; }
        [field: SerializeField] public int MaxAmmoCountInMagazine { get; private set; }
        [field: SerializeField] public float ReloadTime { get; private set; }
        [field: SerializeField] public float Rpm { get; private set; }
    }
    
    [CreateAssetMenu(fileName = "New HackData", menuName = "ScriptableObjects/Weapon/Create New HackData", order = 0)]
    public class HackData : ScriptableObject
    {
        [field: SerializeField] public HackStat HackStat { get; private set; }
    }
}