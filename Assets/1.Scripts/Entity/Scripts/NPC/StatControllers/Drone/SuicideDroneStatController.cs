using System;
using System.Collections;
using System.Collections.Generic;
using _1.Scripts.Entity.Scripts.NPC.AIBehaviors.BehaviorDesigner.SharedVariables;
using _1.Scripts.Entity.Scripts.NPC.Data.AnimationHashData;
using _1.Scripts.Entity.Scripts.NPC.Data.ForRuntime;
using _1.Scripts.Entity.Scripts.NPC.Data.StatDataSO;
using _1.Scripts.Entity.Scripts.Npc.StatControllers.Base;
using _1.Scripts.Interfaces.Common;
using _1.Scripts.Interfaces.NPC;
using _1.Scripts.Manager.Core;
using _1.Scripts.Static;
using _1.Scripts.Util;
using BehaviorDesigner.Runtime;
using UnityEngine;

namespace _1.Scripts.Entity.Scripts.NPC.StatControllers.Drone
{
    public class SuicideDroneStatController : BaseNpcStatController, IDamagable, IStunnable, IHackable
    {
        [Header("StatData")]
        private RuntimeSuicideDroneStatData runtimeSuicideDroneStatData;
        public override RuntimeEntityStatData RuntimeStatData => runtimeSuicideDroneStatData; // 부모에게 자신의 스탯 전송
        
        [Header("Behavior Tree")]
        private BehaviorDesigner.Runtime.BehaviorTree behaviorTree;

        [Header("Stunned")] 
        private bool isStunned;
        public bool IsStunned  => isStunned; 
        private Coroutine stunnedCoroutine;
        [SerializeField] private ParticleSystem onStunParticle;
        
        [Header("Hacking")]
        private Coroutine hackingCoroutine;
        private bool isHacking = false;
        [SerializeField] private float hackingDuration = 3f;
        [SerializeField] private float successChance = 0.7f; // 70% 확률
        
        private void Awake()
        {
            var suicideDroneStatData = CoreManager.Instance.resourceManager.GetAsset<SuicideDroneStatData>("SuicideDroneStatData"); // 자신만의 데이터 가져오기
            runtimeSuicideDroneStatData = new RuntimeSuicideDroneStatData(suicideDroneStatData); // 복사
            animator = GetComponent<Animator>();
            behaviorTree = GetComponent<BehaviorDesigner.Runtime.BehaviorTree>();
        }

        private void Update()
        {
            runtimeSuicideDroneStatData.isAlly = isAlly;
        }

        public void OnTakeDamage(int damage)
        {
            if (!isDead)
            {
                runtimeSuicideDroneStatData.maxHealth -= damage;
                if (runtimeSuicideDroneStatData.maxHealth <= 0)
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
        
        #region 여기부턴 상호작용
        
        public void Hacking()
        {
            if (isHacking || runtimeSuicideDroneStatData.isAlly)
            {
                return;
            }

            if (hackingCoroutine != null)
                StopCoroutine(hackingCoroutine);

            hackingCoroutine = StartCoroutine(HackingProcess());
        }

        private IEnumerator HackingProcess()
        {
            isHacking = true;

            // 1. 드론 멈추기
            float stunDurationOnHacking = hackingDuration + 1f; // 스턴 중 해킹결과가 영향 끼치지 않게 더 길게 설정
            OnStunned(stunDurationOnHacking);

            // 2. 해킹 시도 시간 기다림
            yield return new WaitForSeconds(hackingDuration);

            // 3. 확률 판정
            bool success = UnityEngine.Random.value < successChance;

            if (success)
            {
                // 해킹 성공
                runtimeSuicideDroneStatData.isAlly = true;
                NpcUtil.SetLayerRecursively(this.gameObject, LayerConstants.Ally);
            }
            else
            {
                // 해킹 실패
                // 실패 후 추가 패널티 로직 ㄱㄱ
            }

            // yield return new WaitForSeconds(1f);
            // 움직이기 시작할 때 무언가 추가할거라면 여기에
            
            isHacking = false;
            hackingCoroutine = null;
        }
        
        
        public void OnStunned(float duration = 3f)
        {
            if (isStunned) return;
            
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
