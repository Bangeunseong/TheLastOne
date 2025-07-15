using System;
using System.Collections.Generic;
using System.Threading;
using _1.Scripts.Entity.Scripts.Player.Data;
using _1.Scripts.Item.Common;
using _1.Scripts.Manager.Core;
using _1.Scripts.Manager.Data;
using _1.Scripts.Manager.Subs;
using _1.Scripts.Sound;
using _1.Scripts.Weapon.Scripts.Common;
using _1.Scripts.Weapon.Scripts.Grenade;
using _1.Scripts.Weapon.Scripts.Guns;
using _1.Scripts.Weapon.Scripts.Hack;
using Cysharp.Threading.Tasks;
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
        [field: SerializeField] public int MaxShield { get; private set; }
        [field: SerializeField] public int CurrentShield { get; private set; }
        [field: SerializeField] public float CurrentFocusGauge { get; private set; }
        [field: SerializeField] public float CurrentInstinctGauge { get; private set; }
        [field: SerializeField] public float SkillSpeedMultiplier { get; private set; } = 1f;
        [field: SerializeField] public float ItemSpeedMultiplier { get; private set; } = 1f;
        [field: SerializeField] public float Damage { get; private set; }
        [field: SerializeField] public float AttackRate { get; private set; }
        [field: SerializeField] public int Level { get; private set; }
        [field: SerializeField] public int Experience { get; private set; }
        [field: SerializeField] public bool IsCrouching { get; set; }
        [field: SerializeField] public bool IsUsingFocus { get; private set; }
        [field: SerializeField] public bool IsUsingInstinct { get; private set; }
        [field: SerializeField] public bool IsPlayerHasControl { get; set; } = true;
        [field: SerializeField] public bool IsDead { get; private set; }
        
        [field: Header("Current Physics Data")] 
        [field: SerializeField] public float Speed { get; private set; }
        [field: SerializeField] public float JumpForce { get; private set; }
        [field: SerializeField] public float RotationDamping { get; private set; } = 10f;  // Rotation Speed

        [field: Header("Speed Modifiers")]
        [field: SerializeField] public float CrouchSpeedModifier { get; private set; }
        [field: SerializeField] public float WalkSpeedModifier { get; private set; }
        [field: SerializeField] public float RunSpeedModifier { get; private set; }
        
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
        [field: SerializeField] public float RecoilMultiplier { get; private set; } = 1f;
        [field: SerializeField] public bool IsAttacking { get; set; }
        [field: SerializeField] public bool IsSwitching { get; private set; }
        [field: SerializeField] public bool IsAiming { get; private set; }
        [field: SerializeField] public bool IsReloading { get; private set; }
        
        // Fields
        private CoreManager coreManager;
        private Player player;
        private SoundPlayer reloadPlayer;

        private CancellationTokenSource aimCTS;
        private CancellationTokenSource switchCTS;
        private CancellationTokenSource reloadCTS;
        private CancellationTokenSource itemCTS;
        private CancellationTokenSource focusCTS;
        private CancellationTokenSource instinctCTS;
        private CancellationTokenSource instinctRecoveryCTS;
        
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

        /// <summary>
        /// Initialize Player Stat., using Saved data if exists.
        /// </summary>
        /// <param name="data">DataTransferObject of Saved Data</param>
        public void Initialize(DataTransferObject data)
        {
            coreManager = CoreManager.Instance;
            player = coreManager.gameManager.Player;
            StatData = coreManager.resourceManager.GetAsset<PlayerStatData>("Player");
            
            // Initialize Weapons
            var listOfGuns = GetComponentsInChildren<BaseWeapon>(true);
            foreach (var weapon in listOfGuns)
            {
                weapon.Initialize(gameObject, data);
                Weapons.Add(weapon);
                AvailableWeapons.Add(false);
            }
            if (AvailableWeapons.Count > 0) AvailableWeapons[0] = true;

            // Initialize Damage Converters
            foreach (var converter in DamageConverters) converter.Initialize(this);
            
            if (data == null)
            {
                Service.Log("DataTransferObject is null");
                MaxHealth = StatData.maxHealth;
                CurrentHealth = 10;
                MaxStamina = CurrentStamina = StatData.maxStamina;
                MaxShield = (int)StatData.maxArmor; CurrentShield = 0;
                Damage = StatData.baseDamage;
                AttackRate = StatData.baseAttackRate;
                CurrentFocusGauge = CurrentInstinctGauge = 0f;
                Level = 1;
                Experience = 0;
            }
            else
            {
                Service.Log("DataTransferObject is not null");
                Level = data.characterInfo.level; Experience = data.characterInfo.experience;
                MaxHealth = data.characterInfo.maxHealth; CurrentHealth = data.characterInfo.health;
                MaxStamina = data.characterInfo.maxStamina; CurrentStamina = data.characterInfo.stamina;
                MaxShield = data.characterInfo.maxShield; CurrentShield = data.characterInfo.shield;
                AttackRate = data.characterInfo.attackRate; Damage = data.characterInfo.damage;
                LastSavedPosition = data.currentCharacterPosition.ToVector3();
                LastSavedRotation = data.currentCharacterRotation.ToQuaternion();
                CurrentFocusGauge = data.characterInfo.focusGauge;
                CurrentInstinctGauge = data.characterInfo.instinctGauge;
                
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

            OnInstinctRecover_Idle();
            player.Controller.enabled = true;
        }

        public void UpdateLastSavedTransform()
        {
            LastSavedPosition = player.transform.position;
            LastSavedRotation = player.transform.rotation;
        }
        
        private void OnDestroy()
        {
            StopAllUniTasks();
        }

        /// <summary>
        /// Reduce Health Point, Can customize event when player got damage using 'OnDamage' event
        /// </summary>
        /// <param name="damage">Value of damage</param>
        public void OnTakeDamage(int damage)
        {
            if (IsDead) return;
            if (CurrentShield <= 0)
            {
                CurrentHealth = Mathf.Max(CurrentHealth - damage, 0);
                if (itemCTS != null) CancelItemUsage();
                OnRecoverInstinctGauge(InstinctGainType.Hit);
            }
            else
            {
                if (CurrentShield < damage)
                {
                    CurrentHealth = Mathf.Max(CurrentHealth + CurrentShield - damage, 0);
                    if (itemCTS != null) CancelItemUsage();
                    OnRecoverInstinctGauge(InstinctGainType.Hit);
                }
                CurrentShield = Mathf.Max(CurrentShield - damage, 0);
            }
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
        /// Recover Shield Point
        /// </summary>
        /// <param name="value">Value of shield to recover</param>
        public void OnRecoverShield(int value)
        {
            if (IsDead) return;
            CurrentShield = Mathf.Min(CurrentShield + value, MaxShield);
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
                FocusGainType.Debug => Mathf.Min(CurrentFocusGauge + 1f, 1f),
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
            if (IsDead || CurrentInstinctGauge < value || IsUsingInstinct || CurrentHealth >= MaxHealth * 0.5f) return false;
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
                InstinctGainType.Hit => Mathf.Min(CurrentInstinctGauge + StatData.instinctGaugeRefillRate_OnHit, 1f),
                InstinctGainType.Debug => Mathf.Min(CurrentInstinctGauge + 1f, 1f),
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
            IsPlayerHasControl = true;
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
                case Crossbow hackingGun:
                    if(hackingGun.OnShoot())
                        WeaponAnimators[EquippedWeaponIndex].SetTrigger(player.AnimationData.ShootParameterHash);
                    break;
            }
        }

        public void OnEnablePlayerMovement()
        {
            IsPlayerHasControl = true;
            player.InputProvider.enabled = true;
        }

        public void OnDisablePlayerMovement()
        {
            IsPlayerHasControl = false;
            player.Pov.m_HorizontalAxis.Reset();
            player.Pov.m_VerticalAxis.Reset();
            player.InputProvider.enabled = false;
        }

        private void StopAllUniTasks()
        {
            aimCTS?.Cancel(); aimCTS?.Dispose(); aimCTS = null;
            switchCTS?.Cancel(); switchCTS?.Dispose();  switchCTS = null;
            reloadCTS?.Cancel(); reloadCTS?.Dispose(); reloadCTS = null;
            focusCTS?.Cancel(); focusCTS?.Dispose(); focusCTS = null;
            instinctCTS?.Cancel(); instinctCTS?.Dispose(); instinctCTS = null;
            instinctRecoveryCTS?.Cancel(); instinctRecoveryCTS?.Dispose(); instinctRecoveryCTS = null;
            itemCTS?.Cancel(); itemCTS?.Dispose(); itemCTS = null;
        }
        
        /* - Aim 관련 메소드 - */
        public void OnAim(bool isAim, float targetFoV, float transitionTime)
        {
            aimCTS?.Cancel(); aimCTS?.Dispose();
            aimCTS = new CancellationTokenSource();
            _ = AimAsync(isAim, targetFoV, transitionTime, aimCTS.Token);
        }
        private async UniTaskVoid AimAsync(bool isAim, float targetFoV, float transitionTime, CancellationToken token)
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
                await UniTask.Yield(PlayerLoopTiming.Update, cancellationToken: token, cancelImmediately: true);
            }
            
            IsAiming = isAim;
            player.FirstPersonCamera.m_Lens.FieldOfView = targetFoV;
            aimCTS.Dispose(); aimCTS = null;
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
                    if (reloadCTS != null)
                    {
                        reloadCTS?.Cancel(); reloadCTS?.Dispose();
                        gun.IsReloading = false;
                        IsReloading = false;
                        WeaponAnimators[EquippedWeaponIndex].SetBool(player.AnimationData.ReloadParameterHash, false);
                    }
                    
                    // Play Reload AudioClip
                    float reloadTime = gun.GunData.GunStat.ReloadTime;
                    reloadPlayer = coreManager.soundManager.PlayUISFX(
                        gun.GunData.GunStat.Type == WeaponType.Pistol ? SfxType.PistolReload : SfxType.RifleReload, reloadTime);
                    
                    // Start Reload Coroutine
                    reloadCTS = new CancellationTokenSource();
                    _ = ReloadAsync(reloadTime, reloadCTS.Token);
                    break;
                }
                case GrenadeLauncher { IsReadyToReload: false }:
                    return false;
                case GrenadeLauncher grenadeLauncher:
                {
                    if (reloadCTS != null)
                    {
                        reloadCTS?.Cancel(); reloadCTS?.Dispose();
                        grenadeLauncher.IsReloading = false;
                        IsReloading = false;
                        WeaponAnimators[EquippedWeaponIndex].SetBool(player.AnimationData.ReloadParameterHash, false);
                    }

                    // Player Reload AudioClip
                    float reloadTime = grenadeLauncher.GrenadeData.GrenadeStat.ReloadTime;
                    reloadPlayer = coreManager.soundManager.PlayUISFX(SfxType.GrenadeLauncherReload, reloadTime);
                    
                    // Start Reload Coroutine
                    reloadCTS = new CancellationTokenSource();
                    _ = ReloadAsync(reloadTime,  reloadCTS.Token);
                    break;
                }
                case Crossbow {IsReadyToReload: false}:
                    return false;
                case Crossbow crossbow:
                {
                    if (reloadCTS != null)
                    {
                        reloadCTS?.Cancel(); reloadCTS?.Dispose();
                        crossbow.IsReloading = false;
                        IsReloading = false;
                        WeaponAnimators[EquippedWeaponIndex].SetBool(player.AnimationData.ReloadParameterHash, false);
                    }
                    
                    // Player Reload AudioClip
                    float reloadTime = crossbow.HackData.HackStat.ReloadTime;
                    reloadPlayer = coreManager.soundManager.PlayUISFX(SfxType.CrossbowReload, reloadTime);
                    
                    // Start Reload Coroutine
                    reloadCTS = new CancellationTokenSource();
                    _ = ReloadAsync(reloadTime,  reloadCTS.Token);
                    break;
                }
            }

            return true;
        }
        public bool TryCancelReload()
        {
            if (IsDead || EquippedWeaponIndex <= 0) return false;
            
            reloadCTS?.Cancel(); reloadCTS?.Dispose(); reloadCTS = null;
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
                case Crossbow crossbow:
                    crossbow.IsReloading = false;
                    WeaponAnimators[EquippedWeaponIndex].SetBool(player.AnimationData.ReloadParameterHash, false);
                    break;
            }
            WeaponAnimators[EquippedWeaponIndex].SetFloat(player.AnimationData.AniSpeedMultiplierHash, 1f);
            
            if (reloadPlayer) reloadPlayer.Stop();
            reloadPlayer = null;
            IsReloading = false;
            return true;
        }
        private async UniTaskVoid ReloadAsync(float interval, CancellationToken token)
        {
            var currentAnimator = WeaponAnimators[EquippedWeaponIndex];
            
            if (Weapons[EquippedWeaponIndex] is Gun gun)
            {
                if (gun.CurrentAmmoCount <= 0 || gun.CurrentAmmoCountInMagazine == gun.MaxAmmoCountInMagazine) return;

                // Animation Control (Reload Start)
                currentAnimator.SetBool(player.AnimationData.ReloadParameterHash, true);
                var animationSpeed = gun.GunData.GunStat.Type == WeaponType.Pistol
                    ? player.AnimationData.PistolReloadClipTime / gun.GunData.GunStat.ReloadTime
                    : player.AnimationData.RifleReloadClipTime / gun.GunData.GunStat.ReloadTime;
                currentAnimator.SetFloat(player.AnimationData.AniSpeedMultiplierHash, animationSpeed);
                
                gun.IsReloading = true;
                IsReloading = true;

                var t = 0f;
                while (t < interval)
                {
                    if (coreManager.gameManager.IsGamePaused)
                    {
                        if(currentAnimator.GetFloat(player.AnimationData.AniSpeedMultiplierHash) != 0f)
                            currentAnimator.SetFloat(player.AnimationData.AniSpeedMultiplierHash, 0f);
                    }
                    else
                    {
                        if (!Mathf.Approximately(currentAnimator.GetFloat(player.AnimationData.AniSpeedMultiplierHash), animationSpeed))
                            currentAnimator.SetFloat(player.AnimationData.AniSpeedMultiplierHash, animationSpeed);
                        t += Time.unscaledDeltaTime;
                    }
                    await UniTask.Yield(PlayerLoopTiming.Update, cancellationToken: token, cancelImmediately: true);
                }
                
                Service.Log("Gun reloaded");
                gun.OnReload();
                IsReloading = false;
                gun.IsReloading = false;
                
                // Animation Control (Reload End)
                currentAnimator.SetFloat(player.AnimationData.AniSpeedMultiplierHash, 1f);
                currentAnimator.SetBool(player.AnimationData.ReloadParameterHash, false);
                currentAnimator.SetBool(player.AnimationData.EmptyParameterHash, false);
            } else if (Weapons[EquippedWeaponIndex] is GrenadeLauncher grenadeLauncher)
            {
                if (grenadeLauncher.CurrentAmmoCount <= 0 || grenadeLauncher.CurrentAmmoCountInMagazine ==
                    grenadeLauncher.MaxAmmoCountInMagazine)
                    return;

                // Animation Control (Reload Start)
                currentAnimator.SetBool(player.AnimationData.ReloadParameterHash, true);
                var animationSpeed = player.AnimationData.GrenadeLauncherReloadClipTime /
                                     grenadeLauncher.GrenadeData.GrenadeStat.ReloadTime;
                currentAnimator.SetFloat(player.AnimationData.AniSpeedMultiplierHash, animationSpeed);
                
                grenadeLauncher.IsReloading = true;
                IsReloading = true;
                
                while (true)
                {
                    if (coreManager.gameManager.IsGamePaused)
                    {
                        if(currentAnimator.GetFloat(player.AnimationData.AniSpeedMultiplierHash) != 0f)
                            currentAnimator.SetFloat(player.AnimationData.AniSpeedMultiplierHash, 0f);
                        await UniTask.Yield(PlayerLoopTiming.Update, cancellationToken: token, cancelImmediately: true);
                    }
                    else
                    {
                        if (!Mathf.Approximately(currentAnimator.GetFloat(player.AnimationData.AniSpeedMultiplierHash), animationSpeed))
                            currentAnimator.SetFloat(player.AnimationData.AniSpeedMultiplierHash, animationSpeed);
                        await UniTask.WaitForSeconds(interval / grenadeLauncher.MaxAmmoCountInMagazine, 
                                                true, cancellationToken: token, cancelImmediately: true);
                        if (grenadeLauncher.OnReload()) continue;
                        reloadPlayer.Stop(); break;
                    }
                }
                
                Service.Log("GL reloaded");
                IsReloading = false;
                grenadeLauncher.IsReloading = false;
                
                // Animation Control (Reload End)
                currentAnimator.SetFloat(player.AnimationData.AniSpeedMultiplierHash, 1f);
                currentAnimator.SetBool(player.AnimationData.ReloadParameterHash, false);
                currentAnimator.SetBool(player.AnimationData.EmptyParameterHash, false);
            } else if (Weapons[EquippedWeaponIndex] is Crossbow crossbow)
            {
                if (crossbow.CurrentAmmoCount <= 0 ||
                    crossbow.CurrentAmmoCountInMagazine == crossbow.MaxAmmoCountInMagazine)
                    return;
                
                // Animation Control (Reload Start)
                currentAnimator.SetBool(player.AnimationData.ReloadParameterHash, true);
                var animationSpeed = player.AnimationData.CrossbowReloadClipTime /
                                     crossbow.HackData.HackStat.ReloadTime;
                currentAnimator.SetFloat(player.AnimationData.AniSpeedMultiplierHash, animationSpeed);
                
                crossbow.IsReloading = true;
                IsReloading = true;
                var t = 0f;
                while (t < interval)
                {
                    if (coreManager.gameManager.IsGamePaused)
                    {
                        if(currentAnimator.GetFloat(player.AnimationData.AniSpeedMultiplierHash) != 0f)
                            currentAnimator.SetFloat(player.AnimationData.AniSpeedMultiplierHash, 0f);
                    }
                    else
                    {
                        if (!Mathf.Approximately(currentAnimator.GetFloat(player.AnimationData.AniSpeedMultiplierHash), animationSpeed))
                            currentAnimator.SetFloat(player.AnimationData.AniSpeedMultiplierHash, animationSpeed);
                        t += Time.unscaledDeltaTime;
                    }
                    await UniTask.Yield(PlayerLoopTiming.Update, cancellationToken: token, cancelImmediately: true);
                }
                
                Service.Log("Crossbow reloaded");
                crossbow.OnReload();
                IsReloading = false;
                crossbow.IsReloading = false;
                
                // Animation Control (Reload End)
                currentAnimator.SetFloat(player.AnimationData.AniSpeedMultiplierHash, 1f);
                currentAnimator.SetBool(player.AnimationData.ReloadParameterHash, false);
                currentAnimator.SetBool(player.AnimationData.EmptyParameterHash, false);
            }
            reloadPlayer = null;
            reloadCTS.Dispose(); reloadCTS = null;
        }
        /* --------------------- */
        
        /* - Weapon Switch 메소드 - */
        public void OnSwitchWeapon(int currentWeaponIndex, float duration)
        {
            IsAttacking = false;
            if (IsReloading) TryCancelReload();
            int previousWeaponIndex = EquippedWeaponIndex;
            EquippedWeaponIndex = currentWeaponIndex;
            
            if (switchCTS != null)
            {
                switchCTS?.Cancel(); switchCTS?.Dispose();
                IsSwitching = false;
            }

            switchCTS = new CancellationTokenSource();
            _ = SwitchAsync(previousWeaponIndex, currentWeaponIndex, duration, switchCTS.Token);
        }
        private async UniTaskVoid SwitchAsync(int previousWeaponIndex, int currentWeaponIndex, float duration, CancellationToken token)
        {
            if (previousWeaponIndex == currentWeaponIndex) { switchCTS = null; return; }
            IsSwitching = true;
            
            Service.Log($"Switching from {previousWeaponIndex} to {currentWeaponIndex}");
            if (IsAiming) OnAim(false, 67.5f, 0.2f);
            
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
                case 4: WeaponAnimators[previousWeaponIndex].SetFloat(
                    player.AnimationData.AniSpeedMultiplierHash,
                    player.AnimationData.CrossbowToOtherWeaponClipTime / duration); break;
            }
            
            Service.Log("Switch Weapon");
            WeaponAnimators[previousWeaponIndex].SetTrigger(player.AnimationData.HideParameterHash);
            await UniTask.WaitForSeconds(duration, true, cancellationToken: token, cancelImmediately: true);
            WeaponAnimators[previousWeaponIndex].SetFloat(player.AnimationData.AniSpeedMultiplierHash, 1f);
            Weapons[previousWeaponIndex].gameObject.SetActive(false);
            
            Service.Log("Wield Weapon");
            Weapons[EquippedWeaponIndex].gameObject.SetActive(true);
            await UniTask.DelayFrame(1, cancellationToken: token, cancelImmediately: true);
            WeaponAnimators[EquippedWeaponIndex].SetFloat(player.AnimationData.AniSpeedMultiplierHash, 1f);
            await UniTask.WaitForSeconds(duration, true, cancellationToken: token, cancelImmediately: true);
            IsSwitching = false;
            switchCTS = null;
        }
        /* --------------------- */
        
        /* - Skill 관련 메소드 - */
        private void OnFocusEngaged()
        {
            focusCTS?.Cancel(); focusCTS?.Dispose();
            focusCTS = new CancellationTokenSource();
            _ = FocusAsync(StatData.focusSkillTime, focusCTS.Token);
        }
        private async UniTaskVoid FocusAsync(float duration, CancellationToken token)
        {
            IsUsingFocus = true;
            coreManager.timeScaleManager.ChangeTimeScale(0.5f);
            RecoilMultiplier = 0.5f;
            var t = 0f;
            while (t < duration)
            {
                if (!coreManager.gameManager.IsGamePaused) t += Time.unscaledDeltaTime;
                await UniTask.Yield(PlayerLoopTiming.Update, cancellationToken: token, cancelImmediately: true);
            }
            RecoilMultiplier = 1f;
            coreManager.timeScaleManager.ChangeTimeScale(1f);
            IsUsingFocus = false;
            focusCTS.Dispose(); focusCTS = null;
        }
        private void OnInstinctEngaged()
        {
            instinctCTS?.Cancel(); instinctCTS?.Dispose();
            instinctCTS = new CancellationTokenSource();
            _ = InstinctAsync(StatData.instinctSkillTime, instinctCTS.Token);
        }
        private async UniTaskVoid InstinctAsync(float duration, CancellationToken token)
        {
            IsUsingInstinct = true;
            coreManager.spawnManager.ChangeStencilLayerAllNpc(true);
            coreManager.spawnManager.ChangeLayerOfWeaponsAndItems(true);
            
            SkillSpeedMultiplier = StatData.instinctSkillMultiplier;
            var t = 0f;
            while (t < duration)
            {
                if (token.IsCancellationRequested)
                {
                    coreManager.spawnManager.ChangeStencilLayerAllNpc(false);
                    coreManager.spawnManager.ChangeLayerOfWeaponsAndItems(false);
                    return;
                }
                if (!coreManager.gameManager.IsGamePaused) t += Time.unscaledDeltaTime;
                await UniTask.Yield(PlayerLoopTiming.Update, cancellationToken: token);
            }
            SkillSpeedMultiplier = 1f;
            
            coreManager.spawnManager.ChangeStencilLayerAllNpc(false);
            coreManager.spawnManager.ChangeLayerOfWeaponsAndItems(false);
            IsUsingInstinct = false;
            instinctCTS.Dispose(); instinctCTS = null;
        }
        private void OnInstinctRecover_Idle()
        {
            instinctRecoveryCTS?.Cancel(); instinctRecoveryCTS?.Dispose();
            instinctRecoveryCTS = new CancellationTokenSource();
            _ = InstinctRecover_Async(1, instinctRecoveryCTS.Token);
        }
        private async UniTaskVoid InstinctRecover_Async(float delay, CancellationToken token)
        {
            while (!IsDead)
            {
                if (coreManager.gameManager.IsGamePaused) { await UniTask.Yield(PlayerLoopTiming.Update, cancellationToken: token, true); }
                else
                {
                    await UniTask.WaitForSeconds(delay, true, cancellationToken: token, cancelImmediately: true);
                    OnRecoverInstinctGauge(InstinctGainType.Idle);
                }
            }
        }
        /* -------------------- */
        
        /* - Item 관련 메소드 - */
        public void OnItemUsed(BaseItem usedItem)
        {
            if (itemCTS != null) { CancelItemUsage(); return; }
            
            itemCTS = new CancellationTokenSource();
            _ = Item_Async(usedItem.ItemData, itemCTS.Token);
        }
        private void CancelItemUsage()
        {
            var inGameUI = coreManager.uiManager.InGameUI;
            inGameUI.HideItemProgress(); ItemSpeedMultiplier = 1f;
            itemCTS?.Cancel(); itemCTS?.Dispose(); itemCTS = null;
        }
        private async UniTaskVoid Item_Async(ItemData itemData, CancellationToken token)
        {
            if (!itemData.IsPlayerMovable) ItemSpeedMultiplier = 0f;
            var t = 0f;
            var inGameUI = coreManager.uiManager.InGameUI;
            inGameUI.ShowItemProgress();
            inGameUI.UpdateItemProgress(0f);
            while (t < itemData.Delay)
            {
                if (token.IsCancellationRequested)
                {
                    inGameUI.HideItemProgress(); ItemSpeedMultiplier = 1f;
                    itemCTS.Dispose(); itemCTS = null; return;
                }
                if (!coreManager.gameManager.IsGamePaused)
                {
                    t += Time.unscaledDeltaTime;
                    inGameUI.UpdateItemProgress(t / itemData.Delay);
                }
                await UniTask.Yield(PlayerLoopTiming.Update, cancellationToken: token, cancelImmediately: true);
            }
            
            inGameUI.UpdateItemProgress(1f);
            inGameUI.HideItemProgress();
            
            switch (itemData.ItemType)
            {
                case ItemType.Medkit: 
                case ItemType.NanoAmple: OnRecoverHealth(itemData.Value); break;
                case ItemType.EnergyBar: OnRecoverStamina(itemData.Value); break;
                case ItemType.Shield: OnRecoverShield(itemData.Value); break;
                default: throw new ArgumentOutOfRangeException();
            }
            ItemSpeedMultiplier = 1f;
            itemCTS.Dispose(); itemCTS = null;
        }
        /* ------------------- */
    }
}
