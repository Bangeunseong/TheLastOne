using System;
using System.Collections;
using System.Collections.Generic;
using _1.Scripts.Entity.Scripts.NPC.AIBehaviors.BehaviorDesigner.SharedVariables;
using _1.Scripts.Entity.Scripts.NPC.Data;
using _1.Scripts.Entity.Scripts.NPC.Data.AnimationHashData;
using _1.Scripts.Entity.Scripts.NPC.Data.ForRuntime;
using _1.Scripts.Entity.Scripts.NPC.Data.StatDataSO;
using _1.Scripts.Entity.Scripts.Npc.StatControllers.Base;
using _1.Scripts.Interfaces;
using _1.Scripts.Interfaces.Common;
using _1.Scripts.Interfaces.NPC;
using _1.Scripts.Manager.Core;
using _1.Scripts.Static;
using _1.Scripts.Util;
using BehaviorDesigner.Runtime;
using UnityEngine;
using Random = System.Random;

namespace _1.Scripts.Entity.Scripts.NPC.StatControllers.Drone
{
    public class ReconDroneStatController : BaseNpcStatController, IDamagable, IStunnable, IHackable
    {
        [Header("StatData")]
        private RuntimeReconDroneStatData runtimeReconDroneStatData;
        public override RuntimeEntityStatData RuntimeStatData => runtimeReconDroneStatData; // 부모에게 자신의 스탯 전송
        
        [Header("Behavior Tree")]
        private BehaviorDesigner.Runtime.BehaviorTree behaviorTree;

        [Header("Stunned")] 
        private bool isStunned;
        public bool IsStunned  => isStunned; 
        private Coroutine stunnedCoroutine;
        [SerializeField] private ParticleSystem onStunParticle;
        
        private void Awake()
        {
            var reconDroneStatData = CoreManager.Instance.resourceManager.GetAsset<ReconDroneStatData>("ReconDroneStatData"); // 자신만의 데이터 가져오기
            runtimeReconDroneStatData = new RuntimeReconDroneStatData(reconDroneStatData); // 복사
            animator = GetComponent<Animator>();
            behaviorTree = GetComponent<BehaviorDesigner.Runtime.BehaviorTree>();
        }

        private void Update()
        {
            runtimeReconDroneStatData.isAlly = isAlly;
        }

        public void OnTakeDamage(int damage)
        {
            if (!isDead)
            {
                runtimeReconDroneStatData.maxHealth -= damage;
                if (runtimeReconDroneStatData.maxHealth <= 0)
                {
                    behaviorTree.SetVariableValue("IsDead", true);
                    
                    int[] deathHashes = new int[]
                    {
                        DroneAnimationHashData.Dead1,
                        DroneAnimationHashData.Dead2,
                        DroneAnimationHashData.Dead3
                    };

                    int randomIndex = UnityEngine.Random.Range(0, deathHashes.Length);
                    animator.SetTrigger(deathHashes[randomIndex]);

                    isDead = true;
                }
                else
                {
                    int[] HitHashes = new int[]
                    {
                        DroneAnimationHashData.Hit1,
                        DroneAnimationHashData.Hit2,
                        DroneAnimationHashData.Hit3,
                        DroneAnimationHashData.Hit4
                    };

                    int randomIndex = UnityEngine.Random.Range(0, HitHashes.Length);
                    animator.SetTrigger(HitHashes[randomIndex]);
                }
            }
        }

        public override void ModifySpeed(float percent)
        {
            runtimeReconDroneStatData.moveSpeed *= percent;
        }

        public void DestroyObjectForAnimationEvent()
        {
            Destroy(gameObject);
        }

        #region 여기부턴 상호작용
        public void Hacking()
        {
            if (!runtimeReconDroneStatData.isAlly)
            {
                runtimeReconDroneStatData.isAlly = true;
                NpcUtil.SetLayerRecursively(this.gameObject, LayerConstants.Ally);
                ResetAIState();
            }
        }

        public void OnStunned(float duration = 3f)
        {
            isStunned = true;
            behaviorTree.SetVariableValue("CanRun", false);
            ResetAIState();
            if (stunnedCoroutine != null)
            {
                StopCoroutine(stunnedCoroutine);
            }
            stunnedCoroutine = StartCoroutine(Stunned(duration));
        }

        private IEnumerator Stunned(float duration)
        {
            onStunParticle.Play();
            yield return new WaitForSeconds(duration); // 원하는 시간만큼 유지

            // main.duration = tempDuration;
            if (onStunParticle != null && onStunParticle.IsAlive())
            {
                onStunParticle.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            }

            isStunned = false;
            behaviorTree.SetVariableValue("CanRun", true);
            stunnedCoroutine = null;
        }

        private void ResetAIState()
        {
            behaviorTree.SetVariableValue("target_Transform", null);
            behaviorTree.SetVariableValue("target_Pos", Vector3.zero);
            behaviorTree.SetVariableValue("shouldLookTarget", false);
            behaviorTree.SetVariableValue("IsAlerted", false);
            behaviorTree.SetVariableValue("timer", 0f);
            
            var enemyLight = behaviorTree.GetVariable("Enemy_Light") as SharedLight;
            var allyLight = behaviorTree.GetVariable("Ally_Light") as SharedLight;
            var agent = behaviorTree.GetVariable("agent") as SharedNavMeshAgent;
            var selfTransform = behaviorTree.GetVariable("self_Transform") as SharedTransform;
            
            if (enemyLight != null && enemyLight.Value != null)
            {
                enemyLight.Value.enabled = false;
            }

            if (allyLight != null && allyLight.Value != null)
            {
                allyLight.Value.enabled = false;
            }

            if (agent != null && agent.Value != null && selfTransform != null && selfTransform.Value != null)
            {
                agent.Value.SetDestination(selfTransform.Value.position);
            }
        }
        #endregion
    }
}
