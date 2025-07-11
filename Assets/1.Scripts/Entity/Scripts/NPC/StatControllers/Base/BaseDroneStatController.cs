using System.Collections;
using System.Collections.Generic;
using System.Threading;
using _1.Scripts.Entity.Scripts.NPC.AIBehaviors.BehaviorDesigner.SharedVariables;
using _1.Scripts.Entity.Scripts.NPC.Data.AnimationHashData;
using _1.Scripts.Entity.Scripts.NPC.Data.ForRuntime;
using _1.Scripts.Entity.Scripts.Npc.StatControllers.Base;
using _1.Scripts.Entity.Scripts.Player.Data;
using _1.Scripts.Interfaces.Common;
using _1.Scripts.Interfaces.NPC;
using _1.Scripts.Manager.Core;
using _1.Scripts.Static;
using _1.Scripts.Util;
using BehaviorDesigner.Runtime;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace _1.Scripts.Entity.Scripts.NPC.StatControllers.Base
{
    public abstract class BaseDroneStatController : BaseNpcStatController
    {
        [SerializeField] protected int hackingFailAttackIncrease = 3;
        [SerializeField] protected float hackingFailArmorIncrease = 3f;
        [SerializeField] protected float hackingFailPenaltyDuration = 10f;
        
        protected override void PlayDeathAnimation()
        {
            int[] deathHashes =
            {
                DroneAnimationHashData.Dead1,
                DroneAnimationHashData.Dead2,
                DroneAnimationHashData.Dead3
            };
            animator.SetTrigger(deathHashes[UnityEngine.Random.Range(0, deathHashes.Length)]);
        }

        protected override void PlayHitAnimation()
        {
            int[] hitHashes =
            {
                DroneAnimationHashData.Hit1,
                DroneAnimationHashData.Hit2,
                DroneAnimationHashData.Hit3,
                DroneAnimationHashData.Hit4
            };
            animator.SetTrigger(hitHashes[UnityEngine.Random.Range(0, hitHashes.Length)]);
        }

        protected override void HackingFailurePenalty()
        {
            int baseDamage = runtimeStatData.BaseDamage;
            float baseArmor = runtimeStatData.Armor;
            _= DamageAndArmorIncrease(baseDamage, baseArmor);
            behaviorTree.SetVariableValue("shouldAlertNearBy", true);
        }
        
        private async UniTaskVoid DamageAndArmorIncrease(int baseDamage, float baseArmor)
        {
            runtimeStatData.BaseDamage = baseDamage + hackingFailAttackIncrease;
            runtimeStatData.Armor = baseArmor + hackingFailArmorIncrease;

            await UniTask.WaitForSeconds(hackingFailPenaltyDuration);

            runtimeStatData.BaseDamage = baseDamage;
            runtimeStatData.Armor = baseArmor;
        }
    }
}