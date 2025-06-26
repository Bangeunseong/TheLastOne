using System;
using _1.Scripts.Entity.Scripts.Common;
using _1.Scripts.Entity.Scripts.Player.Data;
using _1.Scripts.Interfaces;
using _1.Scripts.Manager.Core;
using _1.Scripts.Manager.Data;
using _1.Scripts.Manager.Subs;
using JetBrains.Annotations;
using UnityEngine;

namespace _1.Scripts.Entity.Scripts.Player.Core
{
    public class PlayerCondition : MonoBehaviour, IDamagable
    {
        [field: Header("Base Condition Data")]
        [field: SerializeField] public PlayerStatData StatData { get; private set; }
        
        [field: Header("Current Condition Data")]
        [field: SerializeField] public int MaxHealth { get; private set; }
        [field: SerializeField] public int CurrentHealth { get; private set; }
        [field: SerializeField] public float MaxStamina { get; private set; }
        [field: SerializeField] public float CurrentStamina { get; private set; }
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
        
        private CoreManager coreManager;
        
        // Action events
        [CanBeNull] public event Action OnDamage, OnDeath;

        private void Start()
        {
            coreManager = CoreManager.Instance;
            StatData = coreManager.resourceManager.GetAsset<PlayerStatData>("Player");
            
            Initialize(coreManager.gameManager.SaveData);
        }

        public void Initialize(DataTransferObject data)
        {
            if (data == null)
            {
                Service.Log("DataTransferObject is null");
                MaxHealth = CurrentHealth = StatData.maxHealth;
                MaxStamina = CurrentStamina = StatData.maxStamina;
                Damage = StatData.baseDamage;
                AttackRate = StatData.baseAttackRate;
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
    }
}
