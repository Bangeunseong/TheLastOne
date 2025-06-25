using System;
using System.Collections;
using _1.Scripts.Entity.Scripts.Player.Core;
using _1.Scripts.Interfaces;
using _1.Scripts.Manager.Core;
using JetBrains.Annotations;
using UnityEngine;

namespace _1.Scripts.Weapon.Scripts
{
    public enum GunType
    {
        Pistol,
        Rifle,
    }
    
    public class Gun : MonoBehaviour, IShootable, IReloadable
    {
        [Header("Components")] 
        [SerializeField] private ParticleSystem shellParticles;
        [SerializeField] private Light gunShotLight;
        
        [field: Header("Weapon Data")]
        [field: SerializeField] public WeaponData WeaponData { get; private set; }
        
        [field: Header("Current Weapon Settings")]
        [field: SerializeField] public Transform BulletSpawnPoint { get; private set; }
        [field: SerializeField] public int CurrentAmmoCount { get; private set; }
        [field: SerializeField] public int MaxAmmoCountInMagazine { get; private set; }
        [field: SerializeField] public int CurrentAmmoCountInMagazine { get; private set; }
        [field: SerializeField] public LayerMask HittableLayer { get; private set; }
        [SerializeField] private Transform face;
        
        [field: Header("Current Weapon State")]
        [SerializeField] private bool isEmpty;
        [field: SerializeField] public bool IsReloading { get; set; }
        [SerializeField] private bool isRecoiling;

        [Header("Owner")] 
        [SerializeField] private GameObject owner;

        private float timeSinceLastShotFired;
        
        [CanBeNull] private Player player;
        // private Enemy enemy;
        
        public bool IsReady => !isEmpty && !IsReloading && !isRecoiling;
        public bool IsReadyToReload => MaxAmmoCountInMagazine > CurrentAmmoCountInMagazine && !IsReloading;

        private void Awake()
        {
            if (!BulletSpawnPoint) BulletSpawnPoint = this.TryGetChildComponent<Transform>("BulletSpawnPoint");
            if (!shellParticles) shellParticles = this.TryGetChildComponent<ParticleSystem>("MuzzleFlashParticle");
            if (!gunShotLight) gunShotLight = GetComponentInChildren<Light>(true);
        }

        private void Reset()
        {
            if (!BulletSpawnPoint) BulletSpawnPoint = this.TryGetChildComponent<Transform>("BulletSpawnPoint");
            if (!shellParticles) shellParticles = this.TryGetChildComponent<ParticleSystem>("MuzzleFlashParticle");
            if (!gunShotLight) gunShotLight = GetComponentInChildren<Light>(true);
        }

        private void Start()
        {
            timeSinceLastShotFired = WeaponData.WeaponStat.Recoil;
            MaxAmmoCountInMagazine = CurrentAmmoCountInMagazine = WeaponData.WeaponStat.MaxAmmoCountInMagazine;
        }

        private void Update()
        {
            if (!isRecoiling) return;
            if (timeSinceLastShotFired < WeaponData.WeaponStat.Rpm / 3600f) timeSinceLastShotFired += Time.deltaTime;
            else { timeSinceLastShotFired = 0f; isRecoiling = false;}
        }

        public void Initialize(GameObject ownerObj)
        {
            owner = ownerObj;
            if (ownerObj.TryGetComponent(out Player user))
            {
                player = user;
                face = user.CameraPivot;
                HittableLayer &= ~(1 << user.gameObject.layer);
            }
            // else if (owner.TryGetComponent(out Enemy enemy)) this.enemy = enemy;
        }

        public void OnShoot()
        {
            if (!IsReady) return;
            
            var obj = CoreManager.Instance.objectPoolManager.Get(WeaponData.WeaponStat.BulletPrefabId);
            if (!obj.TryGetComponent(out Bullet bullet)) return;
            
            isRecoiling = true;
            Debug.Log("Fire Bullet");
            bullet.Initialize(BulletSpawnPoint.position, GetDirectionOfBullet(),
                WeaponData.WeaponStat.MaxWeaponRange,
                WeaponData.WeaponStat.BulletSpeed, 
                WeaponData.WeaponStat.Damage, HittableLayer);
            
            if(shellParticles.isPlaying) shellParticles.Stop();
            shellParticles.Play();
            StartCoroutine(Flicker());
            
            CurrentAmmoCountInMagazine--;
            if (CurrentAmmoCountInMagazine <= 0) isEmpty = true;
            if (WeaponData.WeaponStat.Type != GunType.Pistol) return;
            if (player != null) player.IsAttacking = false;
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
            if (Physics.Raycast(face.position, face.forward, out var hit, WeaponData.WeaponStat.MaxWeaponRange,
                    HittableLayer))
            {
                targetPoint = hit.point;
            } else targetPoint = face.position + face.forward * WeaponData.WeaponStat.MaxWeaponRange;

            return (targetPoint - BulletSpawnPoint.position).z < 0 ? BulletSpawnPoint.forward : (targetPoint - BulletSpawnPoint.position).normalized;
        }

        private IEnumerator Flicker()
        {
            gunShotLight.enabled = true;
            yield return new WaitForSeconds(0.08f);
            gunShotLight.enabled = false;
        }
    }
}