using System;
using System.Collections;
using System.Collections.Generic;
using _1.Scripts.Entity.Scripts.NPC.AIBehaviors.BehaviorDesigner.SharedVariables;
using _1.Scripts.Entity.Scripts.NPC.Shebot_Weapon;
using _1.Scripts.Interfaces.NPC;
using _1.Scripts.Manager.Core;
using _1.Scripts.Manager.Subs;
using _1.Scripts.Quests.Core;
using _1.Scripts.Static;
using _1.Scripts.Util;
using BehaviorDesigner.Runtime;
using UnityEngine;
using UnityEngine.AI;

namespace _1.Scripts.Entity.Scripts.NPC.AIControllers.ForAnimationEvent
{
    public class AIControlWithAnimationEvent : MonoBehaviour
    {
        private BehaviorDesigner.Runtime.BehaviorTree behaviorTree;
        private Animator animator;
        
        private Shebot_Sword sword;
        private Shebot_Shield shield;
        
        private void Awake()
        {
            behaviorTree = GetComponent<BehaviorDesigner.Runtime.BehaviorTree>();
            animator = GetComponent<Animator>();
            sword = GetComponentInChildren<Shebot_Sword>(true);
            shield = GetComponentInChildren<Shebot_Shield>(true);
        }
        
        public void AIOffForAnimationEvent()
        {
            behaviorTree.SetVariableValue(BehaviorNames.CanRun, false);
        }

        public void AIOnForAnimationEvent()
        {
            var statController = behaviorTree.GetVariable(BehaviorNames.StatController) as SharedBaseNpcStatController;

            if (statController != null && statController.Value is IStunnable stunnable)
            {
                if (!stunnable.IsStunned)
                {
                    behaviorTree.SetVariableValue(BehaviorNames.CanRun, true);
                }
            }
            else
            {
                behaviorTree.SetVariableValue(BehaviorNames.CanRun, true);
            }
        }

        public void SetDestinationNullForAnimationEvent()
        {
            if (behaviorTree.GetVariable(BehaviorNames.Agent) is SharedNavMeshAgent sharedAgent && sharedAgent.Value != null)
            {
                sharedAgent.Value.SetDestination(transform.position);
            }
            else
            {
                Debug.LogWarning("SharedNavMeshAgent를 찾을 수 없거나 값이 없습니다.");
            }
        }
        
        public void DestroyObjectForAnimationEvent()
        {
            NpcUtil.DisableNpc(this.gameObject);
        }

        public void EnableApplyRootMotion()
        {
            animator.applyRootMotion = true;
        }

        public void InterruptBehaviorForAnimationEvent()
        {
            behaviorTree.SetVariableValue(BehaviorNames.IsInterrupted, true);
        }

        public void PlaySoundSignalForAnimationEvent()
        {
            CoreManager.Instance.soundManager.PlaySFX(SfxType.Drone, transform.position, index: 1);
        }
        public void PlaySoundBumForAnimationEvent()
        {
            CoreManager.Instance.soundManager.PlaySFX(SfxType.Drone, transform.position, index: 3);
        }
        
        #region Sword전용
        public void SwordEnableHitForAnimationEvent()
        {
            sword?.EnableHit();
        }
        
        public void SwordDisableHitForAnimationEvent()
        {
            sword?.DisableHit();
        }

        public void ShieldEnableForAnimationEvent()
        {
            shield?.EnableShield();
        }
        
        public void ShieldDisableForAnimationEvent()
        {
            shield?.DisableShield();
        }
        #endregion

        #region Sniper전용

        public void FireForAnimationEvent() // 애니메이션 이벤트로 분리해야 원하는 타이밍에 사격가능
        {
            var muzzleTransform = behaviorTree.GetVariable(BehaviorNames.MuzzleTransform) as SharedTransform;
            var targetPos = behaviorTree.GetVariable(BehaviorNames.TargetPos) as SharedVector3;
            var statController = behaviorTree.GetVariable(BehaviorNames.StatController) as SharedBaseNpcStatController;

            if (muzzleTransform != null && targetPos != null && statController != null)
            {
                Vector3 muzzlePosition = muzzleTransform.Value.position;
                Vector3 direction = (targetPos.Value - muzzlePosition).normalized;
                bool isAlly = statController.Value.RuntimeStatData.IsAlly;
                int damage = statController.Value.RuntimeStatData.BaseDamage;
    
                NpcUtil.FireToTarget(muzzlePosition, direction, isAlly, damage);
            }
        }
        #endregion
    }
}
