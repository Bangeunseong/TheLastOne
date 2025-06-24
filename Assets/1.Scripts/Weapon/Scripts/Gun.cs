using System;
using _1.Scripts.Entity.Scripts.Player.Core;
using _1.Scripts.Interfaces;
using _1.Scripts.Manager.Core;
using UnityEngine;

namespace _1.Scripts.Weapon.Scripts
{
    public class Gun : MonoBehaviour, IShootable, IReloadable, IInteractable
    {
        [field: Header("Weapon Data")]
        [field: SerializeField] public WeaponData WeaponData { get; private set; }
        
        [field: Header("Current Weapon Settings")]
        [field: SerializeField] public Transform BulletSpawnPoint { get; private set; }
        [field: SerializeField] public int CurrentAmmoCount { get; private set; }
        [field: SerializeField] public int MaxAmmoCountInMagazine { get; private set; }
        [field: SerializeField] public int CurrentAmmoCountInMagazine { get; private set; }
        [field: SerializeField] public LayerMask HittableLayer { get; private set; }

        [Header("Current Weapon State")]
        [SerializeField] private bool isEmpty;
        [SerializeField] private bool isReloading;
        [SerializeField] private bool isRecoiling;

        [Header("Owner")] 
        [SerializeField] private GameObject owner;

        private float timeSinceLastShotFired;
        private Camera cam;
        private Player player;
        // private Enemy enemy;
        
        public bool IsReady => !isEmpty && !isReloading && !isRecoiling;

        private void Start()
        {
            timeSinceLastShotFired = WeaponData.WeaponStat.Recoil;
            cam = Camera.main;
        }

        private void Update()
        {
            if (!isRecoiling) return;
            if (timeSinceLastShotFired < WeaponData.WeaponStat.Recoil) timeSinceLastShotFired += Time.deltaTime;
            else isRecoiling = false;
        }

        public void Initialize(GameObject owner)
        {
            this.owner = owner;
            if (owner.TryGetComponent(out Player player)) this.player = player;
            // else if (owner.TryGetComponent(out Enemy enemy)) this.enemy = enemy;
        }

        public void OnShoot()
        {
            if (!IsReady) return;
            
            var obj = CoreManager.Instance.objectPoolManager.Get("Bullet");
            if (!obj.TryGetComponent(out Bullet bullet)) return;
            
            isRecoiling = true;
            timeSinceLastShotFired = 0f;
            bullet.Initialize(BulletSpawnPoint.position, GetDirectionOfBullet(), 
                WeaponData.WeaponStat.MaxWeaponRange,
                WeaponData.WeaponStat.BulletSpeed, 
                WeaponData.WeaponStat.Damage, HittableLayer);
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

        private Vector3 GetDirectionOfBullet()
        {
            Vector3 targetPoint;
            if (Physics.Raycast(cam.transform.position, cam.transform.forward, out var hit, WeaponData.WeaponStat.MaxWeaponRange,
                    HittableLayer))
            {
                targetPoint = hit.point;
            } else targetPoint = cam.transform.position + cam.transform.forward * WeaponData.WeaponStat.MaxWeaponRange;

            return (targetPoint - BulletSpawnPoint.position).normalized;
        }

        public void OnInteract()
        {
            
        }
    }
}