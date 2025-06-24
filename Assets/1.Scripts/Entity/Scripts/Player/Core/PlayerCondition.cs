using System;
using _1.Scripts.Entity.Scripts.Common;
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
        [field: SerializeField] public EntityStatData StatData { get; private set; }
        
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

        private CoreManager coreManager;
        
        // Action events
        [CanBeNull] public event Action OnDamage, OnDeath;

        private void Start()
        {
            coreManager = CoreManager.Instance;
            StatData = coreManager.resourceManager.GetAsset<EntityStatData>("Player");
            
            InitializeStat(coreManager.gameManager.SaveData);
        }

        public void InitializeStat(DataTransferObject data)
        {
            if (data == null)
            {
                MaxHealth = CurrentHealth = StatData.maxHealth;
                MaxStamina = CurrentStamina = StatData.maxStamina;
                Damage = StatData.baseDamage;
                AttackRate = StatData.baseAttackRate;
                Level = 1;
                Experience = 0;
            }
            else
            {
                Level = data.level;
                Experience = data.experience;
                MaxHealth = data.maxHealth; CurrentHealth = data.health;
                MaxStamina = data.maxStamina; CurrentStamina = data.stamina;
                AttackRate = data.attackRate; 
            }

            Speed = StatData.moveSpeed;
            JumpForce = StatData.jumpHeight;
            CrouchSpeedModifier = StatData.crouchMultiplier;
            WalkSpeedModifier = StatData.walkMultiplier;
            RunSpeedModifier = StatData.runMultiplier;
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
