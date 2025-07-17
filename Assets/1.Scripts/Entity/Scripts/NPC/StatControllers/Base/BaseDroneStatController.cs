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
using RaycastPro;
using UnityEngine;

namespace _1.Scripts.Entity.Scripts.NPC.StatControllers.Base
{
    public abstract class BaseDroneStatController : BaseNpcStatController
    {
        [SerializeField] protected int hackingFailAttackIncrease = 3;
        [SerializeField] protected float hackingFailArmorIncrease = 3f;
        [SerializeField] protected float hackingFailPenaltyDuration = 10f;
        public SerializableDictionary<Transform, (Vector3 localPos, Quaternion localRot)> originalTransforms = new();
        private CancellationTokenSource penaltyToken;

        protected override void Awake()
        {
            base.Awake();
            if (originalTransforms.Count == 0) CacheOriginalTransforms();
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            
            animator.Rebind();
            animator.Update(0f);
            ResetTransformsToOriginal();
        }

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
            
            penaltyToken?.Cancel();
            penaltyToken?.Dispose();
            penaltyToken = new CancellationTokenSource();
            
            _= DamageAndArmorIncrease(baseDamage, baseArmor, penaltyToken.Token);
            behaviorTree.SetVariableValue("shouldAlertNearBy", true);
        }
        
        private async UniTaskVoid DamageAndArmorIncrease(int baseDamage, float baseArmor, CancellationToken token)
        {
            runtimeStatData.BaseDamage = baseDamage + hackingFailAttackIncrease;
            runtimeStatData.Armor = baseArmor + hackingFailArmorIncrease;

            await UniTask.WaitForSeconds(hackingFailPenaltyDuration, cancellationToken:token);

            runtimeStatData.BaseDamage = baseDamage;
            runtimeStatData.Armor = baseArmor;
        }

        protected override void ResetAIState()
        {
            base.ResetAIState();
            
            AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
            if (!stateInfo.IsName("DroneBot_Hit1") &&
                !stateInfo.IsName("DroneBot_Hit2") &&
                !stateInfo.IsName("DroneBot_Hit3") &&
                !stateInfo.IsName("DroneBot_Hit4") &&
                !stateInfo.IsName("DroneBot_Idle1"))
            {
                animator.SetTrigger(DroneAnimationHashData.Idle1);
            }
        }
        
        private void CacheOriginalTransforms(Transform parent = null)
        {
            if (parent == null) parent = this.transform;
            
            if (!originalTransforms.ContainsKey(parent))
            {
                originalTransforms.Add(parent, (parent.localPosition, parent.localRotation));
            }

            foreach (Transform child in parent)
            {
                CacheOriginalTransforms(child);
            }
        }

        private void ResetTransformsToOriginal()
        {
            foreach (var kvp in originalTransforms)
            {
                if (kvp.Key != null)
                {
                    kvp.Key.localPosition = kvp.Value.localPos;
                    kvp.Key.localRotation = kvp.Value.localRot;
                }
            }
        }
    }
}