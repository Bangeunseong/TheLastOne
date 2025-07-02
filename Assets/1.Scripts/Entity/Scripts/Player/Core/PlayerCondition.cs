using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using _1.Scripts.Entity.Scripts.Player.Data;
using _1.Scripts.Manager.Core;
using _1.Scripts.Manager.Data;
using _1.Scripts.Manager.Subs;
using _1.Scripts.Sound;
using _1.Scripts.Weapon.Scripts.Common;
using _1.Scripts.Weapon.Scripts.Grenade;
using _1.Scripts.Weapon.Scripts.Guns;
using JetBrains.Annotations;
using UnityEngine;

namespace _1.Scripts.Entity.Scripts.Player.Core
{
    public class PlayerCondition : MonoBehaviour
    {
        [field: Header("Base Condition Data")]
        [field: SerializeField] public PlayerStatData StatData { get; private set; }
        
        [field: Header("Current Condition Data")]
        [field: SerializeField] public int MaxHealth { get; private set; }
        [field: SerializeField] public int CurrentHealth { get; private set; }
        [field: SerializeField] public float MaxStamina { get; private set; }
        [field: SerializeField] public float CurrentStamina { get; private set; }
        [field: SerializeField] public float CurrentFocusGauge { get; private set; }
        [field: SerializeField] public float CurrentInstinctGauge { get; private set; }
        [field: SerializeField] public float CurrentSpeedMultiplier { get; private set; } = 1f;
        [field: SerializeField] public float Damage { get; private set; }
        [field: SerializeField] public float AttackRate { get; private set; }
        [field: SerializeField] public int Level { get; private set; }
        [field: SerializeField] public int Experience { get; private set; }
        [field: SerializeField] public bool IsUsingFocus { get; set; }
        [field: SerializeField] public bool IsUsingInstinct { get; set; }
        [field: SerializeField] public bool IsPlayerHasControl { get; set; } = true;
        [field: SerializeField] public bool IsDead { get; private set; }
        
        [field: Header("Current Physics Data")]
        [field: SerializeField] public float Speed { get; private set; }
        [field: SerializeField] public float JumpForce { get; private set; }
        [field: SerializeField] public float CrouchSpeedModifier { get; private set; }
        [field: SerializeField] public float WalkSpeedModifier { get; private set; }
        [field: SerializeField] public float RunSpeedModifier { get; private set; }
        [field: SerializeField] public float RotationDamping { get; private set; } = 10f;  // Rotation Speed
        
        [field: Header("Damage Converters")]
        [field: SerializeField] public List<DamageConverter> DamageConverters { get; private set; } = new();
       
        [field: Header("Saved Position & Rotation")]
        [field: SerializeField] public Vector3 LastSavedPosition { get; set; }
        [field: SerializeField] public Quaternion LastSavedRotation { get; set; }

        [field: Header("Weapons")]
        [field: SerializeField] public GameObject ArmPivot { get; private set; }
        [field: SerializeField] public List<Animator> WeaponAnimators { get; private set; } = new();
        [field: SerializeField] public List<BaseWeapon> Weapons { get; private set; } = new();
        [field: SerializeField] public List<bool> AvailableWeapons { get; private set; } = new();

        [field: Header("Weapon States")]
        [field: SerializeField] public int EquippedWeaponIndex { get; private set; }
        [field: SerializeField] public bool IsAttacking { get; set; }
        [field: SerializeField] public bool IsSwitching { get; private set; }
        [field: SerializeField] public bool IsAiming { get; private set; }
        [field: SerializeField] public bool IsReloading { get; private set; }
        
        // Coroutine Fields
        private CoreManager coreManager;
        private Player player;
        private SoundPlayer reloadPlayer;
        
        private Coroutine switchCoroutine; 
        private Coroutine aimCoroutine; 
        private Coroutine reloadCoroutine;
        
        // Action events
        [CanBeNull] public event Action OnDamage, OnDeath;

        private void Awake()
        {
            if (!ArmPivot) ArmPivot = this.TryFindFirstChild("ArmPivot");
            
            if (DamageConverters.Count <= 0) 
                DamageConverters.AddRange(GetComponentsInChildren<DamageConverter>(true));
            if (WeaponAnimators.Count <= 0)
                WeaponAnimators.AddRange(ArmPivot.GetComponentsInChildren<Animator>(true));
        }

        private void Reset()
        {
            if (!ArmPivot) ArmPivot = this.TryFindFirstChild("ArmPivot");
            
            if (DamageConverters.Count <= 0) 
                DamageConverters.AddRange(GetComponentsInChildren<DamageConverter>(true));
            if (WeaponAnimators.Count <= 0)
                WeaponAnimators.AddRange(ArmPivot.GetComponentsInChildren<Animator>(true));
        }

        private void Start()
        {
            coreManager = CoreManager.Instance;
            player = coreManager.gameManager.Player;
            StatData = coreManager.resourceManager.GetAsset<PlayerStatData>("Player");
            
            foreach(var converter in DamageConverters) converter.Initialize(this);
            Initialize(coreManager.gameManager.SaveData);
        }

        /// <summary>
        /// Initialize Player Stat., using Saved data if exists.
        /// </summary>
        /// <param name="data">DataTransferObject of Saved Data</param>
        public void Initialize(DataTransferObject data)
        {
            var listOfGuns = GetComponentsInChildren<BaseWeapon>(true);
            foreach (var weapon in listOfGuns)
            {
                weapon.Initialize(gameObject);
                Weapons.Add(weapon);
                AvailableWeapons.Add(false);
            }
            if (AvailableWeapons.Count > 0) AvailableWeapons[0] = true;
            
            if (data == null)
            {
                Service.Log("DataTransferObject is null");
                MaxHealth = CurrentHealth = StatData.maxHealth;
                MaxStamina = CurrentStamina = StatData.maxStamina;
                Damage = StatData.baseDamage;
                AttackRate = StatData.baseAttackRate;
                CurrentFocusGauge = 0f;
                CurrentInstinctGauge = 0f;
                Level = 1;
                Experience = 0;
            }
            else
            {
                Service.Log("DataTransferObject is not null");
                Level = data.characterInfo.level; Experience = data.characterInfo.experience;
                MaxHealth = data.characterInfo.maxHealth; CurrentHealth = data.characterInfo.health;
                MaxStamina = data.characterInfo.maxStamina; CurrentStamina = data.characterInfo.stamina;
                AttackRate = data.characterInfo.attackRate; Damage = data.characterInfo.damage;
                LastSavedPosition = data.currentCharacterPosition.ToVector3();
                LastSavedRotation = data.currentCharacterRotation.ToQuaternion();
                
                Service.Log(LastSavedPosition + "," +  LastSavedRotation);
                transform.SetPositionAndRotation(LastSavedPosition, LastSavedRotation);
                
                for (var i = 0; i < data.AvailableWeapons.Length; i++)
                    AvailableWeapons[i] = data.AvailableWeapons[i];
            }

            Speed = StatData.moveSpeed;
            JumpForce = StatData.jumpHeight;
            CrouchSpeedModifier = StatData.crouchMultiplier;
            WalkSpeedModifier = StatData.walkMultiplier;
            RunSpeedModifier = StatData.runMultiplier;
            
            coreManager.gameManager.Player.Controller.enabled = true;
        }

        /// <summary>
        /// Reduce Health Point, Can customize event when player got damage using 'OnDamage' event
        /// </summary>
        /// <param name="damage">Value of damage</param>
        public void OnTakeDamage(int damage)
        {
            if (IsDead) return;
            CurrentHealth -= damage;
            OnDamage?.Invoke();
            
            if (CurrentHealth <= 0) { OnDead(); }
        }

        /// <summary>
        /// Recover Health Point
        /// </summary>
        /// <param name="value">Value of hp to recover</param>
        public void OnRecoverHealth(int value)
        {
            if (IsDead) return;
            CurrentHealth = Mathf.Min(CurrentHealth + value, MaxHealth);
        }

        /// <summary>
        /// Consume Stamina Point
        /// </summary>
        /// <param name="stamina">Value to consume from player stamina point</param>
        public void OnConsumeStamina(float stamina) 
        { 
            if (IsDead) return; 
            CurrentStamina = Mathf.Max(CurrentStamina - stamina, 0);
        }

        /// <summary>
        /// Recover Stamina Point
        /// </summary>
        /// <param name="stamina">Value of stamina to recover</param>
        public void OnRecoverStamina(float stamina)
        {
            if (IsDead) return;
            CurrentStamina = Mathf.Min(CurrentStamina + stamina, MaxStamina);
        }
        
        /// <summary>
        /// Consume Focus Gauge
        /// </summary>
        /// <param name="value">Value to consume focus</param>
        /// <returns>Returns true, if there are enough points to consume. If not, return false.</returns>
        public bool OnConsumeFocusGauge(float value = 1f)
        {
            if (IsDead || CurrentFocusGauge < value || IsUsingFocus) return false;
            CurrentFocusGauge = Mathf.Max(CurrentFocusGauge - value, 0f);
            OnFocusEngaged();
            return true;
        }

        /// <summary>
        /// Recover Focus Point
        /// </summary>
        /// <param name="value">Value to recover focus</param>
        public void OnRecoverFocusGauge(FocusGainType value)
        {
            if (IsDead || IsUsingFocus || IsUsingInstinct) return;
            
            CurrentFocusGauge = value switch
            {
                FocusGainType.Kill => Mathf.Min(CurrentFocusGauge + StatData.focusGaugeRefillRate_OnKill, 1f),
                FocusGainType.HeadShot => Mathf.Min(CurrentFocusGauge + StatData.focusGaugeRefillRate_OnHeadShot, 1f),
                FocusGainType.Hack => Mathf.Min(CurrentFocusGauge + StatData.focusGaugeRefillRate_OnHacked, 1f),
                _ => throw new ArgumentOutOfRangeException(nameof(value), value, null)
            };
        }

        /// <summary>
        /// Consume Instinct Gauge
        /// </summary>
        /// <param name="value">Value to consume instinct</param>
        /// <returns>Returns true, if there are enough points to consume. If not, return false.</returns>
        public bool OnConsumeInstinctGauge(float value = 1f)
        {
            if (IsDead || CurrentInstinctGauge < value || IsUsingInstinct) return false;
            CurrentInstinctGauge = Mathf.Max(CurrentInstinctGauge - value, 0f);
            OnInstinctEngaged();
            return true;
        }
        
        /// <summary>
        /// Recover Instinct Point
        /// </summary>
        /// <param name="value">Value to recover instinct</param>
        public void OnRecoverInstinctGauge(InstinctGainType value)
        {
            if (IsDead || IsUsingInstinct || IsUsingFocus) return;

            CurrentInstinctGauge = value switch
            {
                InstinctGainType.Idle => Mathf.Min(CurrentInstinctGauge + StatData.instinctGaugeRefillRate_OnIdle, 1f),
                InstinctGainType.Hit => Mathf.Min(CurrentFocusGauge + StatData.instinctGaugeRefillRate_OnHit, 1f),
                _ => throw new ArgumentOutOfRangeException(nameof(value), value, null)
            };
        }
        
        public void OnTakeExp(int exp)
        {
            if (IsDead) return;
            Experience += exp;
            if (Experience >= Level * 120) { OnLevelUp(); return; } // 조정 필요
        }

        private void OnLevelUp()
        {
            if (IsDead) return;
            Experience -= Level * 120;
            Level++;
            CoreManager.Instance.SaveData_QueuedAsync();
        }

        private void OnDead()
        {
            IsDead = true;
            player.Pov.m_HorizontalAxis.Reset();
            player.Pov.m_VerticalAxis.Reset();
            player.InputProvider.enabled = false;
            player.PlayerInput.enabled = false;
            
            OnDeath?.Invoke();
        }

        public void OnReset()
        {
            IsDead = false;
            CurrentHealth = MaxHealth;
            CurrentStamina = MaxStamina;
            player.InputProvider.enabled = true;
            player.PlayerInput.enabled = true;
        }

        public void OnAttack()
        {
            if (!IsAttacking) return;
            switch (Weapons[EquippedWeaponIndex])
            {
                case Gun gun:
                    if (gun.OnShoot())
                        WeaponAnimators[EquippedWeaponIndex].SetTrigger(player.AnimationData.ShootParameterHash);
                    break;
                case GrenadeLauncher grenadeThrower:
                    if (grenadeThrower.OnShoot())
                        WeaponAnimators[EquippedWeaponIndex].SetTrigger(player.AnimationData.ShootParameterHash);
                    break;
            }
        }
        
        /* - Aim 관련 메소드 - */
        public void OnAim(bool isAim, float targetFoV, float transitionTime)
        {
            if (aimCoroutine != null){ StopCoroutine(aimCoroutine); IsAiming = !isAim; }
            aimCoroutine = StartCoroutine(ChangeFoV_Coroutine(isAim, targetFoV, transitionTime));
            
        }
        private IEnumerator ChangeFoV_Coroutine(bool isAim, float targetFoV, float transitionTime)
        {
            float currentFoV = player.FirstPersonCamera.m_Lens.FieldOfView;
            WeaponAnimators[EquippedWeaponIndex].SetBool(player.AnimationData.AimParameterHash, isAim);
            
            var time = 0f;
            while (time < transitionTime)
            {
                time += Time.unscaledDeltaTime;
                float t = time / transitionTime;
                var value = Mathf.Lerp(currentFoV, targetFoV, t);
                player.FirstPersonCamera.m_Lens.FieldOfView = value;
                yield return null;
            }
            
            IsAiming = isAim;
            player.FirstPersonCamera.m_Lens.FieldOfView = targetFoV;
            aimCoroutine = null;
        }
        /* ----------------- */
        
        /* - Reload 관련 메소드 - */
        public bool TryStartReload()
        {
            if (IsDead || IsSwitching || EquippedWeaponIndex <= 0) return false;
            
            switch (Weapons[EquippedWeaponIndex])
            {
                case Gun { IsReadyToReload: false }:
                    return false;
                case Gun gun:
                {
                    if (reloadCoroutine != null)
                    {
                        StopCoroutine(reloadCoroutine); 
                        gun.IsReloading = false;
                        IsReloading = false;
                        WeaponAnimators[EquippedWeaponIndex].SetBool(player.AnimationData.ReloadParameterHash, false);
                    }
                    
                    // Play Reload AudioClip
                    float clipLength = gun.GunData.GunStat.ReloadTime;
                    reloadPlayer = coreManager.soundManager.PlayUISFX(
                        gun.GunData.GunStat.Type == WeaponType.Pistol ? SfxType.PistolReload : SfxType.RifleReload, clipLength);
                    
                    // Start Reload Coroutine
                    reloadCoroutine = StartCoroutine(Reload_Coroutine(gun.GunData.GunStat.ReloadTime + 0.1f));
                    break;
                }
                case GrenadeLauncher { IsReadyToReload: false }:
                    return false;
                case GrenadeLauncher grenadeLauncher:
                {
                    if (reloadCoroutine != null)
                    {
                        StopCoroutine(reloadCoroutine);
                        grenadeLauncher.IsReloading = false;
                        IsReloading = false;
                        WeaponAnimators[EquippedWeaponIndex].SetBool(player.AnimationData.ReloadParameterHash, false);
                    }

                    // Player Reload AudioClip
                    float clipLength = grenadeLauncher.GrenadeData.GrenadeStat.ReloadTime;
                    reloadPlayer = coreManager.soundManager.PlayUISFX(SfxType.GrenadeLauncherReload, clipLength);
                    
                    // Start Reload Coroutine
                    reloadCoroutine = StartCoroutine(Reload_Coroutine(grenadeLauncher.GrenadeData.GrenadeStat.ReloadTime + 0.3f));
                    break;
                }
            }

            return true;
        }
        public bool TryCancelReload()
        {
            if (IsDead || EquippedWeaponIndex <= 0 || reloadCoroutine == null) return false;
            
            StopCoroutine(reloadCoroutine);
            switch (Weapons[EquippedWeaponIndex])
            {
                case Gun gun:
                    gun.IsReloading = false;
                    WeaponAnimators[EquippedWeaponIndex].SetBool(player.AnimationData.ReloadParameterHash, false);
                    break;
                case GrenadeLauncher grenadeLauncher:
                    grenadeLauncher.IsReloading = false;
                    WeaponAnimators[EquippedWeaponIndex].SetBool(player.AnimationData.ReloadParameterHash, false);
                    break;
            }
            WeaponAnimators[EquippedWeaponIndex].SetFloat(player.AnimationData.AniSpeedMultiplierHash, 1f);
            
            if (reloadPlayer) reloadPlayer.Stop();
            reloadPlayer = null;
            reloadCoroutine = null;
            IsReloading = false;
            return true;
        }
        private IEnumerator Reload_Coroutine(float interval)
        {
            var currentAnimator = WeaponAnimators[EquippedWeaponIndex];
            
            if (Weapons[EquippedWeaponIndex] is Gun gun)
            {
                if (gun.CurrentAmmoCount <= 0 || gun.CurrentAmmoCountInMagazine == gun.MaxAmmoCountInMagazine)
                    yield break;

                // Animation Control (Reload Start)
                currentAnimator.SetBool(player.AnimationData.ReloadParameterHash, true);
                currentAnimator.SetFloat(player.AnimationData.AniSpeedMultiplierHash,
                        gun.GunData.GunStat.Type == WeaponType.Pistol
                            ? player.AnimationData.PistolReloadClipTime / gun.GunData.GunStat.ReloadTime
                            : player.AnimationData.RifleReloadClipTime / gun.GunData.GunStat.ReloadTime);
                
                gun.IsReloading = true;
                IsReloading = true;
                yield return new WaitForSecondsRealtime(interval);
                gun.OnReload();
                IsReloading = false;
                gun.IsReloading = false;
                
                // Animation Control (Reload End)
                currentAnimator.SetFloat(player.AnimationData.AniSpeedMultiplierHash, 1f);
                currentAnimator.SetBool(player.AnimationData.ReloadParameterHash, false);
                currentAnimator.SetBool(player.AnimationData.EmptyParameterHash, false);
            } else if (Weapons[EquippedWeaponIndex] is GrenadeLauncher grenadeLauncher)
            {
                if (grenadeLauncher.CurrentAmmoCount <= 0 || grenadeLauncher.CurrentAmmoCountInMagazine == grenadeLauncher.MaxAmmoCountInMagazine) 
                    yield break;

                // Animation Control (Reload Start)
                currentAnimator.SetBool(player.AnimationData.ReloadParameterHash, true);
                currentAnimator.SetFloat(player.AnimationData.AniSpeedMultiplierHash,
                    player.AnimationData.GrenadeLauncherReloadClipTime /
                    grenadeLauncher.GrenadeData.GrenadeStat.ReloadTime);
                
                grenadeLauncher.IsReloading = true;
                IsReloading = true;
                yield return new WaitForSecondsRealtime(interval);
                grenadeLauncher.OnReload();
                IsReloading = false;
                grenadeLauncher.IsReloading = false;
                
                // Animation Control (Reload End)
                currentAnimator.SetFloat(player.AnimationData.AniSpeedMultiplierHash, 1f);
                currentAnimator.SetBool(player.AnimationData.ReloadParameterHash, false);
                currentAnimator.SetBool(player.AnimationData.EmptyParameterHash, false);
            }
            reloadPlayer = null;
            reloadCoroutine = null;
        }
        /* --------------------- */
        
        /* - Weapon Switch 메소드 - */
        public void OnSwitchWeapon(int currentWeaponIndex, float duration)
        {
            IsAttacking = false;
            if (IsReloading) TryCancelReload();
            int previousWeaponIndex = EquippedWeaponIndex;
            EquippedWeaponIndex = currentWeaponIndex;
            
            if (switchCoroutine != null){ StopCoroutine(switchCoroutine); IsSwitching = false; }
            switchCoroutine = StartCoroutine(OnSwitchWeapon_Coroutine(previousWeaponIndex, currentWeaponIndex, duration));
        }
        private IEnumerator OnSwitchWeapon_Coroutine(int previousWeaponIndex, int currentWeaponIndex, float duration)
        {
            if (previousWeaponIndex == currentWeaponIndex) { switchCoroutine = null; yield break; }
            IsSwitching = true;
            
            Service.Log($"{previousWeaponIndex}, {currentWeaponIndex}");
            if (IsAiming) OnAim(false, 67.5f, 0.2f);
            
            Service.Log("Switch Weapon");
            // 무기를 밑으로 먼저 내리기

            switch (previousWeaponIndex)
            {
                case 0: WeaponAnimators[previousWeaponIndex].SetFloat(
                    player.AnimationData.AniSpeedMultiplierHash, 
                    player.AnimationData.HandToOtherWeaponClipTime / duration); break; 
                case 1: WeaponAnimators[previousWeaponIndex].SetFloat(
                    player.AnimationData.AniSpeedMultiplierHash, 
                    player.AnimationData.PistolToOtherWeaponClipTime / duration); break; 
                case 2: WeaponAnimators[previousWeaponIndex].SetFloat(
                    player.AnimationData.AniSpeedMultiplierHash, 
                    player.AnimationData.RifleToOtherWeaponClipTime / duration); break;
                case 3: WeaponAnimators[previousWeaponIndex].SetFloat(
                    player.AnimationData.AniSpeedMultiplierHash, 
                    player.AnimationData.GrenadeLauncherToOtherWeaponClipTime / duration); break;
            }
            
            WeaponAnimators[previousWeaponIndex].SetTrigger(player.AnimationData.HideParameterHash);
            yield return new WaitForSecondsRealtime(duration);
            WeaponAnimators[previousWeaponIndex].SetFloat(player.AnimationData.AniSpeedMultiplierHash, 1f);
            Weapons[previousWeaponIndex].gameObject.SetActive(false);
            
            Service.Log("Wield Weapon");
            Weapons[EquippedWeaponIndex].gameObject.SetActive(true);
            yield return new WaitForEndOfFrame();
            WeaponAnimators[EquippedWeaponIndex].SetFloat(player.AnimationData.AniSpeedMultiplierHash, 1f);
            yield return new WaitForSecondsRealtime(duration);
            switchCoroutine = null;
            IsSwitching = false;
        }
        /* --------------------- */
        
        /* - Skill 관련 메소드 - */
        private void OnFocusEngaged()
        {
            StartCoroutine(Focus_Coroutine(StatData.focusSkillTime));
        }
        private IEnumerator Focus_Coroutine(float duration)
        {
            IsUsingFocus = true;
            
            yield return new WaitForSecondsRealtime(duration);
            
            IsUsingFocus = false;
        }
        private void OnInstinctEngaged()
        {
            StartCoroutine(Instinct_Coroutine(StatData.instinctSkillTime));
        }
        private IEnumerator Instinct_Coroutine(float duration)
        {
            IsUsingInstinct = true;
            
            // TODO: Turn On Enemy Silhouette (By using spawn manager)
            CurrentSpeedMultiplier = StatData.instinctSkillMultiplier;
            yield return new WaitForSecondsRealtime(duration);
            CurrentSpeedMultiplier = 1f;
            // TODO: Turn Off Enemy Silhouette
            
            IsUsingInstinct = false;
        }
        /* -------------------- */
    }
}
