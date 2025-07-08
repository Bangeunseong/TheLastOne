using System;
using System.Collections;
using System.Collections.Generic;
using _1.Scripts.Entity.Scripts.NPC.AIBehaviors.BehaviorDesigner.SharedVariables;
using _1.Scripts.Entity.Scripts.NPC.Data;
using _1.Scripts.Entity.Scripts.NPC.Data.AnimationHashData;
using _1.Scripts.Entity.Scripts.NPC.Data.ForRuntime;
using _1.Scripts.Entity.Scripts.NPC.Data.StatDataSO;
using _1.Scripts.Entity.Scripts.Npc.StatControllers.Base;
using _1.Scripts.Entity.Scripts.Player.Data;
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
        
        [Header("Hacking")]
        [SerializeField] private float hackingDuration = 3f;
        [SerializeField] private float successChance = 0.7f; // 70% 확률
        [SerializeField] private int hackingFailAttackIncrease = 3;
        [SerializeField] private float hackingFailArmorIncrease = 3f;
        [SerializeField] private float hackingFailPenaltyDuration = 10f;
        private Coroutine hackingCoroutine;
        private bool isHacking = false;
        
        private void Awake()
        {
            var reconDroneStatData = CoreManager.Instance.resourceManager.GetAsset<ReconDroneStatData>("ReconDroneStatData"); // 자신만의 데이터 가져오기
            runtimeReconDroneStatData = new RuntimeReconDroneStatData(reconDroneStatData); // 복사
            animator = GetComponent<Animator>();
            behaviorTree = GetComponent<BehaviorDesigner.Runtime.BehaviorTree>();
        }

        private void Update()
        {
            runtimeReconDroneStatData.IsAlly = isAlly;
        }

        public void OnTakeDamage(int damage)
        {
            if (!isDead)
            {
                float armorRatio = runtimeReconDroneStatData.Armor / runtimeReconDroneStatData.MaxArmor;
                float reducePercent = Mathf.Clamp01(armorRatio); // 0.0 ~ 1.0 사이
                damage = (int)(damage * (1f - reducePercent));

                runtimeReconDroneStatData.MaxHealth -= damage;
                if (runtimeReconDroneStatData.MaxHealth <= 0)
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
            runtimeReconDroneStatData.MoveSpeed *= percent;
        }

        #region 여기부턴 상호작용
        public void Hacking()
        {
            if (isHacking || runtimeReconDroneStatData.IsAlly)
            {
                return;
            }

            if (hackingCoroutine != null)
            {
                StopCoroutine(hackingCoroutine);
            }

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
                runtimeReconDroneStatData.IsAlly = true;
                NpcUtil.SetLayerRecursively(this.gameObject, LayerConstants.Ally);
                CoreManager.Instance.gameManager.Player.PlayerCondition.OnRecoverFocusGauge(FocusGainType.Hack);
            }
            else
            {
                // 해킹 실패
                // 실패 후 추가 패널티 로직 ㄱㄱ
                
                // 공격력 및 방어력이 10% 증가
                int baseDamage = runtimeReconDroneStatData.BaseDamage;
                float baseArmor = runtimeReconDroneStatData.Armor;
                StartCoroutine(DamageAndArmorIncrease(baseDamage, baseArmor));
                behaviorTree.SetVariableValue("shouldAlertNearBy", false);
            }

            // yield return new WaitForSeconds(1f);
            // 움직이기 시작할 때 무언가 추가할거라면 여기에
            
            isHacking = false;
            hackingCoroutine = null;
        }
        
        private IEnumerator DamageAndArmorIncrease(int baseDamage, float baseArmor)
        {
            runtimeReconDroneStatData.BaseDamage = baseDamage + hackingFailAttackIncrease;
            runtimeReconDroneStatData.Armor = baseArmor + hackingFailArmorIncrease;
            
            yield return new WaitForSeconds(hackingFailPenaltyDuration);
            
            runtimeReconDroneStatData.BaseDamage = baseDamage;
            runtimeReconDroneStatData.Armor = baseArmor;
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
