using System;
using System.Collections;
using System.Collections.Generic;
using _1.Scripts.Entity.Scripts.Player.Data;
using _1.Scripts.Interfaces;
using _1.Scripts.Interfaces.Common;
using _1.Scripts.Manager.Core;
using _1.Scripts.Manager.Data;
using _1.Scripts.Weapon.Scripts;
using _1.Scripts.Weapon.Scripts.Common;
using _1.Scripts.Weapon.Scripts.Grenade;
using _1.Scripts.Weapon.Scripts.Guns;
using JetBrains.Annotations;
using Unity.VisualScripting;
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
        [field: SerializeField] public float Damage { get; private set; }
        [field: SerializeField] public float AttackRate { get; private set; }
        [field: SerializeField] public int Level { get; private set; }
        [field: SerializeField] public int Experience { get; private set; }
        [field: SerializeField] public bool IsPlayerHasControl { get; set; }
        [field: SerializeField] public bool IsDead { get; private set; }
        
        [field: Header("Current Physics Data")]
        [field: SerializeField] public float Speed { get; private set; }
        [field: SerializeField] public float JumpForce { get; private set; }
        [field: SerializeField] public float CrouchSpeedModifier { get; private set; }
        [field: SerializeField] public float WalkSpeedModifier { get; private set; }
        [field: SerializeField] public float RunSpeedModifier { get; private set; }
        [field: SerializeField] public float RotationDamping { get; private set; } = 10f;  // Rotation Speed

        [field: Header("Saved Position & Rotation")]
        [field: SerializeField] public Vector3 LastSavedPosition { get; set; }
        [field: SerializeField] public Quaternion LastSavedRotation { get; set; }
        
        [field: Header("Guns")]
        [field: SerializeField] public List<BaseWeapon> Weapons { get; private set; } = new();
        [field: SerializeField] public List<bool> AvailableWeapons { get; private set; } = new();
        [field: SerializeField] public int EquippedWeaponIndex { get; private set; } = -1;
        [field: SerializeField] public bool IsAttacking { get; set; }
        [field: SerializeField] public bool IsSwitching { get; private set; }
        [field: SerializeField] public bool IsAiming { get; private set; }
        
        [field: Header("Damage Converters")]
        [field: SerializeField] public List<DamageConverter> DamageConverters { get; private set; } = new();
       
        // Coroutine Fields
        private CoreManager coreManager;
        private Player player;
        private Coroutine switchCoroutine;
        private Coroutine aimCoroutine;
        
        // Action events
        [CanBeNull] public event Action OnDamage, OnDeath;

        private void Awake()
        {
            if (DamageConverters.Count <= 0) 
                DamageConverters.AddRange(GetComponentsInChildren<DamageConverter>());
        }

        private void Reset()
        {
            if (DamageConverters.Count <= 0) 
                DamageConverters.AddRange(GetComponentsInChildren<DamageConverter>());
        }

        private void Start()
        {
            coreManager = CoreManager.Instance;
            player = coreManager.gameManager.Player;
            StatData = coreManager.resourceManager.GetAsset<PlayerStatData>("Player");
            
            foreach(var converter in DamageConverters) converter.Initialize(this);
            Initialize(coreManager.gameManager.SaveData);
        }

        public void Initialize(DataTransferObject data)
        {
            var listOfGuns = GetComponentsInChildren<BaseWeapon>(true);
            foreach (var weapon in listOfGuns)
            {
                weapon.Initialize(gameObject);
                Weapons.Add(weapon);
                AvailableWeapons.Add(false);
            }
            
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

        public void OnRecoverHealth(int value)
        {
            if (IsDead) return;
            CurrentHealth = Mathf.Min(CurrentHealth + value, MaxHealth);
        }

        public void OnTakeDamage(int damage)
        {
            if (IsDead) return;
            CurrentHealth -= damage;
            OnDamage?.Invoke();
            
            if (CurrentHealth <= 0) { OnDead(); }
        }

        public void OnRecoverStamina(float stamina)
        {
            if (IsDead) return;
            CurrentStamina = Mathf.Min(CurrentStamina + stamina, MaxStamina);
        }

        public void OnConsumeFocusGauge(float value = 1f)
        {
            if (IsDead) return;
            if (CurrentFocusGauge < value) return;
            CurrentFocusGauge = Mathf.Max(CurrentFocusGauge - value, 0f);
            // TODO: FocusGauge를 썼을 때 Skill 효과 발동
        }

        public void OnRecoverFocusGauge(float value)
        {
            if (IsDead) return;
            CurrentFocusGauge = Mathf.Min(CurrentFocusGauge + value, 1f);
        }

        public void OnConsumeInstinctGauge(float value = 1f)
        {
            if (IsDead) return;
            if (CurrentInstinctGauge < value) return; CurrentInstinctGauge = Mathf.Max(CurrentInstinctGauge - value, 0f);
            // TODO: InstinctGauge를 썼을 때 Skill 효과 발동
        }

        public void OnRecoverInstinctGauge(float value)
        {
            if (IsDead) return;
            CurrentInstinctGauge = Mathf.Min(CurrentInstinctGauge + value, 1f);
        }

        public void OnConsumeStamina(float stamina)
        {
            if (IsDead) return;
            CurrentStamina = Mathf.Max(CurrentStamina - stamina, 0);
        }
        
        public void OnTakeExp(int exp)
        {
            if (IsDead) return;
            Experience += exp;
            if (Experience >= Level * 120) { OnLevelUp(); return; } // 조정 필요
        }

        public void OnAttack()
        {
            if (!IsAttacking || EquippedWeaponIndex < 0) return;
            switch (Weapons[EquippedWeaponIndex])
            {
                case Gun gun:
                    gun.OnShoot();
                    break;
                case GrenadeLauncher grenadeThrower:
                    grenadeThrower.OnShoot();
                    break;
            }
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
            OnDeath?.Invoke();
        }
        
        /* - Aim 관련 메소드 - */
        public void OnAim(bool isAim, float targetFoV, float transitionTime)
        {
            if (aimCoroutine != null){ StopCoroutine(aimCoroutine); }
            aimCoroutine = StartCoroutine(ChangeFoV_Coroutine(isAim, targetFoV, transitionTime));
            IsAiming = isAim;
        }
        
        private IEnumerator ChangeFoV_Coroutine(bool isAim, float targetFoV, float transitionTime)
        {
            Vector3 currentPosition = player.WeaponPivot.localPosition;
            Vector3 targetLocalPosition = isAim
                ? player.WeaponPoints["AimPoint"].localPosition
                : player.WeaponPoints["WieldPoint"].localPosition;
            float currentFoV = player.FirstPersonCamera.m_Lens.FieldOfView;

            var time = 0f;
            while (time < transitionTime)
            {
                time += Time.deltaTime;
                float t = time / transitionTime;
                var value = Mathf.Lerp(currentFoV, targetFoV, t);
                player.FirstPersonCamera.m_Lens.FieldOfView = value;
                if(EquippedWeaponIndex is >= 0 and <= 2)
                    player.WeaponPivot.localPosition = Vector3.Lerp(currentPosition, targetLocalPosition, t);
                yield return null;
            }

            player.FirstPersonCamera.m_Lens.FieldOfView = targetFoV;
            if(EquippedWeaponIndex is >= 0 and <= 2)
                player.WeaponPivot.localPosition = targetLocalPosition;
            aimCoroutine = null;
        }
        /* ----------------- */
        
        /* - Weapon Switch 메소드 - */
        public void OnSwitchWeapon(int currentWeaponIndex, float duration)
        {
            IsAttacking = false;
            int previousWeaponIndex = EquippedWeaponIndex;
            EquippedWeaponIndex = currentWeaponIndex;
            if (switchCoroutine != null){ StopCoroutine(switchCoroutine); }
            switchCoroutine = StartCoroutine(OnSwitchWeapon_Coroutine(previousWeaponIndex, currentWeaponIndex, duration));
        }

        public IEnumerator OnSwitchWeapon_Coroutine(int previousWeaponIndex, int currentWeaponIndex, float duration)
        {
            IsSwitching = true;
            
            Service.Log($"{previousWeaponIndex}, {currentWeaponIndex}");
            
            if (IsAiming) OnAim(false, 67.5f, 0.2f);
            while (IsAiming){}
            
            Vector3 currentWeaponPivotPosition = player.WeaponPivot.localPosition;
            Quaternion currentWeaponPivotRotation = player.WeaponPivot.localRotation;
            Vector3 targetLocalPosition = player.WeaponPoints["SwitchPoint"].localPosition;
            Quaternion targetLocalRotation = player.WeaponPoints["SwitchPoint"].localRotation;
            
            if (previousWeaponIndex >= 0)
            {
                Service.Log("Switch Weapon");
                // 무기를 밑으로 먼저 내리기
                var time = 0f;
                while (time < duration)
                {
                    time += Time.deltaTime;
                    float t = time / duration;
                    player.WeaponPivot.SetLocalPositionAndRotation(
                        Vector3.Lerp(currentWeaponPivotPosition, targetLocalPosition, t), 
                        Quaternion.Lerp(currentWeaponPivotRotation, targetLocalRotation, t));
                    yield return null;
                }

                player.WeaponPivot.transform.SetLocalPositionAndRotation(targetLocalPosition, targetLocalRotation);
                Weapons[previousWeaponIndex].gameObject.SetActive(false);
            }
            
            // 만약 들어온 weaponIndex에 해당하는 무기 혹은 weaponIndex가 0보다 작을 경우 예외처리
            if (currentWeaponIndex < 0 || !AvailableWeapons[currentWeaponIndex])
            {
                switchCoroutine = null;
                IsSwitching = false;
                yield break;
            }
            
            Service.Log("Wield Weapon");
            currentWeaponPivotPosition = player.WeaponPivot.localPosition;
            currentWeaponPivotRotation = player.WeaponPivot.localRotation;
            targetLocalPosition = player.WeaponPoints["WieldPoint"].localPosition;
            targetLocalRotation = player.WeaponPoints["WieldPoint"].localRotation;
            
            Weapons[EquippedWeaponIndex].gameObject.SetActive(true);
            
            float weaponWieldTime = 0f;
            while (weaponWieldTime < duration)
            {
                weaponWieldTime += Time.deltaTime;
                float t = weaponWieldTime / duration;
                player.WeaponPivot.SetLocalPositionAndRotation(
                    Vector3.Lerp(currentWeaponPivotPosition, targetLocalPosition, t), 
                    Quaternion.Lerp(currentWeaponPivotRotation, targetLocalRotation, t));
                yield return null;
            }
            
            player.WeaponPivot.transform.SetLocalPositionAndRotation(targetLocalPosition, targetLocalRotation);
            switchCoroutine = null;
            IsSwitching = false;
        }
        /* --------------------- */
    }
}
