using System.Collections;
using _1.Scripts.Entity.Scripts.Player.Core;
using _1.Scripts.Interfaces.NPC;
using _1.Scripts.Interfaces.Weapon;
using _1.Scripts.Manager.Core;
using _1.Scripts.Manager.Data;
using _1.Scripts.Manager.Subs;
using _1.Scripts.UI.InGame;
using _1.Scripts.Weapon.Scripts.Common;
using UnityEngine;

namespace _1.Scripts.Weapon.Scripts.Hack
{
    public class HackGun : BaseWeapon, IReloadable
    {
        [Header("Components")] 
        [SerializeField] private ParticleSystem muzzleFlashParticle;
        [SerializeField] private LightCurves lightCurves;
        
        [field: Header("HackGun Data")]
        [field: SerializeField] public HackData HackData { get; private set; }
        
        [field: Header("Current HackGun Settings")]
        [SerializeField] private Transform face;
        [field: SerializeField] public Transform BulletSpawnPoint { get; private set; }
        [field: SerializeField] public int CurrentAmmoCount { get; private set; }
        [field: SerializeField] public int MaxAmmoCountInMagazine { get; private set; }
        [field: SerializeField] public int CurrentAmmoCountInMagazine { get; private set; }
        
        [field: Header("Current Weapon State")]
        [field: SerializeField] public bool IsRecoiling { get; private set; }
        [field: SerializeField] public bool IsEmpty { get; private set; }
        [field: SerializeField] public bool IsReloading { get; set; }
        
        // Fields
        private float timeSinceLastShotFired;
        private CoreManager coreManager;
        public bool IsAlreadyPlayedEmpty;

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
            
            if (!(timeSinceLastShotFired >= 60f / HackData.HackStat.Rpm)) return;
            timeSinceLastShotFired = 0f;
            IsRecoiling = false;
        }

        public override void Initialize(GameObject ownerObj, DataTransferObject dto = null)
        {
            coreManager = CoreManager.Instance;
            timeSinceLastShotFired = 0f;
            IsRecoiling = false;
            MaxAmmoCountInMagazine = HackData.HackStat.MaxAmmoCountInMagazine;
            
            owner = ownerObj;
            if (!ownerObj.TryGetComponent(out Player user)) return;
            player = user;
            isOwnedByPlayer = true;
            if (dto != null)
            {
                var weapon = dto.Weapons[(int)HackData.HackStat.Type];
                CurrentAmmoCount = weapon.currentAmmoCount;
                CurrentAmmoCountInMagazine = weapon.currentAmmoCountInMagazine;
                if (CurrentAmmoCountInMagazine <= 0) IsEmpty = true;
            }
            else
            {
                CurrentAmmoCount = 0;
                CurrentAmmoCountInMagazine = HackData.HackStat.MaxAmmoCountInMagazine;
            }
            face = user.CameraPivot;
        }

        public override bool OnShoot()
        {
            if (!IsReady)
            {
                if (!IsEmpty || IsAlreadyPlayedEmpty) return false;
                IsAlreadyPlayedEmpty = true;
                CoreManager.Instance.soundManager.PlaySFX(SfxType.HackGunEmpty, BulletSpawnPoint.position);
                return false;
            }
            if (Physics.Raycast(BulletSpawnPoint.position, GetDirectionOfBullet(), out var hit, HackData.HackStat.MaxWeaponRange, HittableLayer))
            {
                if (hit.collider.TryGetComponent(out IHackable hackable))
                {
                    var distance = Vector3.Distance(BulletSpawnPoint.position, hit.point);
                    hackable.Hacking(CalculateChance(distance));
                }
            }
            
            IsRecoiling = true;
            if (player) player.PlayerRecoil.ApplyRecoil(-HackData.HackStat.Recoil * player.PlayerCondition.RecoilMultiplier);
            
            // Play VFX
            if (lightCurves) StartCoroutine(Flicker());
            if (muzzleFlashParticle.isPlaying) muzzleFlashParticle.Stop();
            muzzleFlashParticle.Play();
            
            // Play Randomized Gun Shooting Sound
            CoreManager.Instance.soundManager
                .PlaySFX(SfxType.HackGunShoot, BulletSpawnPoint.position);
            
            CurrentAmmoCountInMagazine--;
            if (CurrentAmmoCountInMagazine <= 0)
            {
                IsEmpty = true;
                if (player)
                    player.PlayerCondition.WeaponAnimators[player.PlayerCondition.EquippedWeaponIndex]
                        .SetBool(player.AnimationData.EmptyParameterHash, true);
            }
            
            if (IsAlreadyPlayedEmpty) IsAlreadyPlayedEmpty = false;
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
            coreManager.uiManager.GetUI<WeaponUI>()?.Refresh(false);
            return true;
        }

        public override bool OnRefillAmmo(int ammo)
        {
            if (CurrentAmmoCount >= HackData.HackStat.MaxAmmoCount) return false;
            CurrentAmmoCount = Mathf.Min(CurrentAmmoCount + ammo, HackData.HackStat.MaxAmmoCount);
            return true;
        }
        
        private Vector3 GetDirectionOfBullet()
        {
            Vector3 targetPoint;
            
            if (Physics.Raycast(face.position, face.forward, out var hit, HackData.HackStat.MaxWeaponRange,
                    HittableLayer)) { targetPoint = hit.point; }
            else { targetPoint = face.position + face.forward * HackData.HackStat.MaxWeaponRange; }

            return (targetPoint - BulletSpawnPoint.position).normalized;
        }

        private float CalculateChance(float distance)
        {
            if (distance >= HackData.HackStat.MaxDistance) return 0f;
            if (distance >= HackData.HackStat.MinDistance)
            {
                var distanceRatio = 1 - (distance - HackData.HackStat.MinDistance) /
                    (HackData.HackStat.MaxDistance - HackData.HackStat.MinDistance);
                return HackData.HackStat.MinChance + 
                       (HackData.HackStat.MaxChance - HackData.HackStat.MinChance) * distanceRatio;
            }
            return HackData.HackStat.MaxChance;
        }
        
        private IEnumerator Flicker()
        {
            lightCurves.gameObject.SetActive(true);
            yield return new WaitForSecondsRealtime(lightCurves.GraphTimeMultiplier);
            lightCurves.gameObject.SetActive(false);
        }
    }
}