using System.Collections;
using _1.Scripts.Entity.Scripts.Player.Core;
using _1.Scripts.Interfaces;
using _1.Scripts.Manager.Core;
using _1.Scripts.Manager.Subs;
using _1.Scripts.Weapon.Scripts.Common;
using UnityEngine;

namespace _1.Scripts.Weapon.Scripts.Guns
{
    public enum GunType
    {
        Pistol,
        Rifle,
    }
    
    public class Gun : BaseWeapon, IShootable, IReloadable
    {
        [Header("Components")] 
        [SerializeField] private ParticleSystem shellParticles;
        [SerializeField] private Light gunShotLight;
     
        [field: Header("Gun Data")]
        [field: SerializeField] public GunData GunData { get; protected set; }
        
        [field: Header("Current Weapon Settings")]
        [field: SerializeField] public Transform BulletSpawnPoint { get; private set; }
        [field: SerializeField] public int CurrentAmmoCount { get; private set; }
        [field: SerializeField] public int MaxAmmoCountInMagazine { get; private set; }
        [field: SerializeField] public int CurrentAmmoCountInMagazine { get; private set; }
        [SerializeField] private Transform face;
        
        [field: Header("Current Weapon State")]
        [SerializeField] private bool isEmpty;
        [SerializeField] private bool isRecoiling;
        [field: SerializeField] public bool IsReloading { get; set; }
        
        // Fields
        private float timeSinceLastShotFired;
        
        // Properties
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
            timeSinceLastShotFired = GunData.GunStat.Rpm / 3600f;
            MaxAmmoCountInMagazine = GunData.GunStat.MaxAmmoCountInMagazine;
        }

        private void Update()
        {
            if (!isRecoiling) return;
            if (timeSinceLastShotFired < GunData.GunStat.Rpm / 3600f) timeSinceLastShotFired += Time.deltaTime;
            else { timeSinceLastShotFired = 0f; isRecoiling = false;}
        }

        public override void Initialize(GameObject ownerObj)
        {
            owner = ownerObj;
            if (ownerObj.TryGetComponent(out Player user))
            {
                player = user;
                isOwnedByPlayer = true;
                if (CoreManager.Instance.gameManager.SaveData != null)
                {
                    var weapon = CoreManager.Instance.gameManager.SaveData.Weapons[(int)GunData.GunStat.Type];
                    CurrentAmmoCount = weapon.currentAmmoCount;
                    CurrentAmmoCountInMagazine = weapon.currentAmmoCountInMagazine;
                    if (CurrentAmmoCountInMagazine <= 0) isEmpty = true;
                }
                else
                {
                    CurrentAmmoCount = 12;
                    CurrentAmmoCountInMagazine = GunData.GunStat.MaxAmmoCountInMagazine;
                }
                    
                face = user.CameraPivot;
                HittableLayer &= ~(1 << user.gameObject.layer);
            }
            // else if (owner.TryGetComponent(out Enemy enemy)) this.enemy = enemy;
        }

        public void OnShoot()
        {
            if (!IsReady) return;
            
            var obj = CoreManager.Instance.objectPoolManager.Get(GunData.GunStat.BulletPrefabId);
            if (!obj.TryGetComponent(out Bullet bullet)) return;
            
            isRecoiling = true;
            Debug.Log("Fire Bullet");

            if (!isOwnedByPlayer)
            {
                bullet.Initialize(BulletSpawnPoint.position, GetDirectionOfBullet(),
                    GunData.GunStat.MaxWeaponRange,
                    GunData.GunStat.BulletSpeed,
                    GunData.GunStat.Damage, HittableLayer);
            }
            else
            {
                if (Physics.Raycast(BulletSpawnPoint.position, GetDirectionOfBullet(), out var hit, float.MaxValue, HittableLayer))
                {
                    if (hit.collider.TryGetComponent(out IDamagable damagable))
                    {
                        damagable.OnTakeDamage(GunData.GunStat.Damage);
                    } else if(hit.collider.gameObject.layer.Equals(LayerMask.NameToLayer("Wall")))
                    {
                        var bulletHole = CoreManager.Instance.objectPoolManager.Get("BulletHole_Wall");
                        bulletHole.transform.position = hit.point;
                        bulletHole.transform.rotation = Quaternion.LookRotation(hit.normal);
                    } else if (hit.collider.gameObject.layer.Equals(LayerMask.NameToLayer("Ground")))
                    {
                        var bulletHole = CoreManager.Instance.objectPoolManager.Get("BulletHole_Ground");
                        bulletHole.transform.position = hit.point;
                        bulletHole.transform.rotation = Quaternion.LookRotation(hit.normal);
                    }
                }
            }
            
            // Play VFX
            if (gunShotLight != null) StartCoroutine(Flicker());
            if (shellParticles.isPlaying) shellParticles.Stop();
            shellParticles.Play();
            
            // Play Randomized Gun Shooting Sound
            CoreManager.Instance.soundManager.PlaySFX(SfxType.PlayerAttack, BulletSpawnPoint.position, -1);
            
            CurrentAmmoCountInMagazine--;
            if (CurrentAmmoCountInMagazine <= 0) isEmpty = true;
            if (GunData.GunStat.Type != GunType.Pistol) return;
            if (player != null) player.PlayerCondition.IsAttacking = false;
        }

        public void OnReload()
        {
            var reloadableAmmoCount = Mathf.Min(MaxAmmoCountInMagazine - CurrentAmmoCountInMagazine, CurrentAmmoCount);
            if (reloadableAmmoCount <= 0) return;
            
            CurrentAmmoCount -= reloadableAmmoCount;
            CurrentAmmoCountInMagazine += reloadableAmmoCount;
            isEmpty = CurrentAmmoCountInMagazine <= 0;
        }

        public void OnRefillAmmo(int ammo)
        {
            CurrentAmmoCount = Mathf.Min(CurrentAmmoCount + ammo, GunData.GunStat.MaxAmmoCount);
        }

        private Vector3 GetDirectionOfBullet()
        {
            Vector3 targetPoint;
            if (Physics.Raycast(face.position, face.forward, out var hit, GunData.GunStat.MaxWeaponRange,
                    HittableLayer))
            {
                targetPoint = hit.point;
            } else targetPoint = face.position + face.forward * GunData.GunStat.MaxWeaponRange;

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