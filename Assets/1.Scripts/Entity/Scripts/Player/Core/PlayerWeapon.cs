using System.Collections.Generic;
using _1.Scripts.Manager.Core;
using _1.Scripts.Manager.Data;
using _1.Scripts.Weapon.Scripts.Common;
using UnityEngine;

namespace _1.Scripts.Entity.Scripts.Player.Core
{
    public class PlayerWeapon : MonoBehaviour
    {
        [field: Header("Weapons")]
        [field: SerializeField] public GameObject ArmPivot { get; private set; }
        [field: SerializeField] public List<Animator> WeaponAnimators { get; private set; } = new();
        [field: SerializeField] public List<BaseWeapon> Weapons { get; private set; } = new();
        [field: SerializeField] public List<bool> AvailableWeapons { get; private set; } = new();

        private CoreManager coreManager;
        
        private void Awake()
        {
            if (!ArmPivot) ArmPivot = this.TryFindFirstChild("ArmPivot");
            
            if (WeaponAnimators.Count <= 0)
                WeaponAnimators.AddRange(ArmPivot.GetComponentsInChildren<Animator>(true));
        }

        private void Reset()
        {
            if (!ArmPivot) ArmPivot = this.TryFindFirstChild("ArmPivot");
            
            if (WeaponAnimators.Count <= 0)
                WeaponAnimators.AddRange(ArmPivot.GetComponentsInChildren<Animator>(true));
        }

        public void Initialize(DataTransferObject data)
        {
            coreManager = CoreManager.Instance;
            
            // Initialize Weapons
            var listOfGuns = GetComponentsInChildren<BaseWeapon>(true);
            foreach (var weapon in listOfGuns)
            {
                weapon.Initialize(gameObject, data);
                Weapons.Add(weapon);
                AvailableWeapons.Add(false);
            }
            if (AvailableWeapons.Count > 0) AvailableWeapons[0] = true;

            if (data == null) return;
            for (var i = 0; i < data.AvailableWeapons.Length; i++)
                AvailableWeapons[i] = data.AvailableWeapons[i];
        }
    }
}