using _1.Scripts.Interfaces;
using UnityEngine;

namespace _1.Scripts.Weapon.Scripts
{
    public class Gun : MonoBehaviour, IShootable, IReloadable
    {
        [field: Header("Weapon Data")]
        [field: SerializeField] public WeaponData WeaponData { get; private set; }
        
        [field: Header("Current Weapon Settings")]
        [field: SerializeField] public int CurrentAmmoCount { get; private set; }
        [field: SerializeField] public int MaxAmmoCountInMagazine { get; private set; }
        [field: SerializeField] public int CurrentAmmoCountInMagazine { get; private set; }

        [Header("Current Weapon State")]
        [SerializeField] private bool isEmpty;
        [SerializeField] private bool isReloading;

        public bool IsReady => !isEmpty && !isReloading;
        
        public void OnShoot()
        {
            if (!IsReady) return;
            
            
        }

        public void OnReload()
        {
            var reloadableAmmoCount = Mathf.Min(MaxAmmoCountInMagazine - CurrentAmmoCountInMagazine, CurrentAmmoCount);
            if (reloadableAmmoCount <= 0) return;
            
            CurrentAmmoCount -= reloadableAmmoCount;
            CurrentAmmoCountInMagazine += reloadableAmmoCount;
        }

        public void OnRefillAmmo(int ammo)
        {
            CurrentAmmoCount = Mathf.Min(CurrentAmmoCount + ammo, WeaponData.WeaponStat.MaxAmmoCount);
        }
    }
}