using System.Collections;
using _1.Scripts.Entity.Scripts.Player.Core;
using _1.Scripts.Interfaces.Common;
using _1.Scripts.Interfaces.Weapon;
using _1.Scripts.Manager.Core;
using _1.Scripts.Manager.Data;
using _1.Scripts.Manager.Subs;
using _1.Scripts.Weapon.Scripts.Common;
using UnityEngine;

namespace _1.Scripts.Weapon.Scripts.Guns
{
    public class Gun : BaseWeapon, IReloadable
    {
        [Header("Components")] 
        [SerializeField] private ParticleSystem muzzleFlashParticle;
        [SerializeField] private LightCurves lightCurves;
     
        [field: Header("Gun Data")]
        [field: SerializeField] public GunData GunData { get; protected set; }
        
        [field: Header("Current Weapon Settings")]
        [field: SerializeField] public Transform BulletSpawnPoint { get; private set; }
        [field: SerializeField] public int CurrentAmmoCount { get; private set; }
        [field: SerializeField] public int MaxAmmoCountInMagazine { get; private set; }
        [field: SerializeField] public int CurrentAmmoCountInMagazine { get; private set; }
        [SerializeField] private Transform face;
        [field: SerializeField] public bool IsRayCastGun { get; private set; }
        
        [field: Header("Current Weapon State")]
        [field: SerializeField] public bool IsRecoiling { get; private set; }
        [field: SerializeField] public bool IsEmpty { get; private set; }
        [field: SerializeField] public bool IsReloading { get; set; }
        
        // Fields
        private float timeSinceLastShotFired;
        private CoreManager coreManager;
        
        // Properties
        public bool IsReady => !IsEmpty && !IsReloading && !IsRecoiling;
        public bool IsReadyToReload => MaxAmmoCountInMagazine > CurrentAmmoCountInMagazine && !IsReloading && CurrentAmmoCount > 0;

        private void Awake()
        {
            if (!BulletSpawnPoint) BulletSpawnPoint = this.TryGetChildComponent<Transform>("BulletSpawnPoint");
            if (!muzzleFlashParticle) muzzleFlashParticle = this.TryGetChildComponent<ParticleSystem>("MuzzleFlashParticle");
            if (!lightCurves) lightCurves = this.TryGetChildComponent<LightCurves>("LightCurves");
        }

        private void Reset()
        {
            if (!BulletSpawnPoint) BulletSpawnPoint = this.TryGetChildComponent<Transform>("BulletSpawnPoint");
            if (!muzzleFlashParticle) muzzleFlashParticle = this.TryGetChildComponent<ParticleSystem>("MuzzleFlashParticle");
            if (!lightCurves) lightCurves = this.TryGetChildComponent<LightCurves>("LightCurves");
        }

        private void Update()
        {
            if (!IsRecoiling || coreManager.gameManager.IsGamePaused) return;
            timeSinceLastShotFired += Time.unscaledDeltaTime;
            
            if (!(timeSinceLastShotFired >= 60f / GunData.GunStat.Rpm)) return;
            timeSinceLastShotFired = 0f;
            IsRecoiling = false;
        }

        public override void Initialize(GameObject ownerObj, DataTransferObject dto = null)
        {
            coreManager = CoreManager.Instance;
            timeSinceLastShotFired = 0f;
            IsRecoiling = false;
            MaxAmmoCountInMagazine = GunData.GunStat.MaxAmmoCountInMagazine;
            
            owner = ownerObj;
            if (ownerObj.TryGetComponent(out Player user))
            {
                player = user;
                isOwnedByPlayer = true;
                if (dto != null)
                {
                    var weapon = dto.Weapons[(int)GunData.GunStat.Type];
                    CurrentAmmoCount = weapon.currentAmmoCount;
                    CurrentAmmoCountInMagazine = weapon.currentAmmoCountInMagazine;
                    if (CurrentAmmoCountInMagazine <= 0) IsEmpty = true;
                }
                else
                {
                    CurrentAmmoCount = 0;
                    CurrentAmmoCountInMagazine = GunData.GunStat.MaxAmmoCountInMagazine;
                }
                    
                face = user.CameraPivot;
            }
            // else if (owner.TryGetComponent(out Enemy enemy)) this.enemy = enemy;
        }

        public override bool OnShoot()
        {
            if (!IsReady) return false;
            if (!IsRayCastGun)
            {
                var obj = CoreManager.Instance.objectPoolManager.Get(GunData.GunStat.BulletPrefabId); 
                if (!obj.TryGetComponent(out Bullet bullet)) return false;
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
                        bulletHole.transform.SetPositionAndRotation(hit.point, Quaternion.LookRotation(hit.normal));
                        bulletHole.transform.SetParent(hit.transform);
                    } else if (hit.collider.gameObject.layer.Equals(LayerMask.NameToLayer("Ground")))
                    {
                        var bulletHole = CoreManager.Instance.objectPoolManager.Get("BulletHole_Ground");
                        bulletHole.transform.SetPositionAndRotation(hit.point, Quaternion.LookRotation(hit.normal));
                        bulletHole.transform.SetParent(hit.transform);
                    }
                }
            }
            
            IsRecoiling = true;
            if (player) player.PlayerRecoil.ApplyRecoil(-GunData.GunStat.Recoil * player.PlayerCondition.RecoilMultiplier);
            
            // Play VFX
            if (lightCurves) StartCoroutine(Flicker());
            if (muzzleFlashParticle.isPlaying) muzzleFlashParticle.Stop();
            muzzleFlashParticle.Play();
            
            // Play Randomized Gun Shooting Sound
            CoreManager.Instance.soundManager
                .PlaySFX(GunData.GunStat.Type == WeaponType.Pistol ? SfxType.PistolShoot : SfxType.RifleShoot, 
                BulletSpawnPoint.position, -1);
            
            CurrentAmmoCountInMagazine--;
            if (CurrentAmmoCountInMagazine <= 0)
            {
                IsEmpty = true;
                if (player)
                    player.PlayerCondition.WeaponAnimators[player.PlayerCondition.EquippedWeaponIndex]
                        .SetBool(player.AnimationData.EmptyParameterHash, true);
            }
            if (GunData.GunStat.Type != WeaponType.Pistol) return true;
            if (player) player.PlayerCondition.IsAttacking = false;
            return true;
        }

        public bool OnReload()
        {
            int reloadableAmmoCount;
            if (isOwnedByPlayer)
                reloadableAmmoCount = Mathf.Min(MaxAmmoCountInMagazine - CurrentAmmoCountInMagazine, CurrentAmmoCount);
            else reloadableAmmoCount = MaxAmmoCountInMagazine - CurrentAmmoCount;
            
            if (reloadableAmmoCount <= 0) return false;
            
            CurrentAmmoCount -= reloadableAmmoCount;
            CurrentAmmoCountInMagazine += reloadableAmmoCount;
            IsEmpty = CurrentAmmoCountInMagazine <= 0;
            return true;
        }

        public override bool OnRefillAmmo(int ammo)
        {
            if (CurrentAmmoCount >= GunData.GunStat.MaxAmmoCount) return false;
            CurrentAmmoCount = Mathf.Min(CurrentAmmoCount + ammo, GunData.GunStat.MaxAmmoCount);
            return true;
        }

        private void GetOrthonormalBasis(Vector3 forward, out Vector3 right, out Vector3 up)
        {
            right = Vector3.Cross(forward, Vector3.up);
            if (right.sqrMagnitude < 0.001f)
                right = Vector3.Cross(forward, Vector3.right);
            right.Normalize();
    
            up = Vector3.Cross(right, forward).normalized;
        }
        
        private Vector3 GetDirectionOfBullet()
        {
            Vector3 targetPoint;
            Vector2 randomCirclePoint = Random.insideUnitCircle * GunData.GunStat.Accuracy;
            
            if (Physics.Raycast(face.position, face.forward, out var hit, GunData.GunStat.MaxWeaponRange,
                    HittableLayer))
            {
                GetOrthonormalBasis(hit.normal, out var right, out var up);
                if (isOwnedByPlayer)
                {
                    if (!player!.PlayerCondition.IsAiming)
                    {
                        var distance = Vector3.Distance(hit.point, face.position);
                        randomCirclePoint *= distance / GunData.GunStat.MaxWeaponRange;
                        targetPoint = hit.point + right * randomCirclePoint.x + up * randomCirclePoint.y;
                    } else targetPoint = hit.point;
                }
                else targetPoint = hit.point + right * randomCirclePoint.x + up * randomCirclePoint.y;
            }
            else
            {
                GetOrthonormalBasis(face.forward, out var right, out var up);
                
                if (isOwnedByPlayer)
                {
                    if (!player!.PlayerCondition.IsAiming)
                    {
                        targetPoint = face.position + face.forward * GunData.GunStat.MaxWeaponRange + right * randomCirclePoint.x + up * randomCirclePoint.y;
                    } else targetPoint = face.position + face.forward * GunData.GunStat.MaxWeaponRange;
                }
                else targetPoint = face.position + face.forward * GunData.GunStat.MaxWeaponRange + right * randomCirclePoint.x + up * randomCirclePoint.y;
            }

            return (targetPoint - BulletSpawnPoint.position).normalized;
        }

        private IEnumerator Flicker()
        {
            lightCurves.gameObject.SetActive(true);
            yield return new WaitForSecondsRealtime(lightCurves.GraphTimeMultiplier);
            lightCurves.gameObject.SetActive(false);
        }
    }
}