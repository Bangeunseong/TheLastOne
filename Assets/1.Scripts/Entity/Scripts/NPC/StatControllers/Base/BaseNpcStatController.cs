using System;
using System.Collections;
using _1.Scripts.Entity.Scripts.Common;
using _1.Scripts.Entity.Scripts.NPC.AIBehaviors.BehaviorDesigner.SharedVariables;
using _1.Scripts.Entity.Scripts.NPC.Data.ForRuntime;
using _1.Scripts.Entity.Scripts.Player.Data;
using _1.Scripts.Interfaces;
using _1.Scripts.Interfaces.Common;
using _1.Scripts.Interfaces.NPC;
using _1.Scripts.Manager.Core;
using _1.Scripts.Static;
using _1.Scripts.Util;
using BehaviorDesigner.Runtime;
using UnityEngine;

namespace _1.Scripts.Entity.Scripts.Npc.StatControllers.Base
{
    public enum EnemyType
    {
        ReconDrone,
        SuicideDrone
    }
    
    /// <summary>
    /// Npc 스텟 공통로직 정의
    /// </summary>
    public abstract class BaseNpcStatController : MonoBehaviour, IDamagable, IStunnable, IHackable
    {
        /// <summary>
        /// 자식마다 들고있는 런타임 스탯을 부모가 가지고 있도록 함
        /// </summary>
        [Header("StatData")]
        protected RuntimeEntityStatData runtimeStatData;
        public RuntimeEntityStatData RuntimeStatData => runtimeStatData;
        public bool IsDead { get; private set; }
        
        [Header("Components")]
        protected Animator animator;
        protected BehaviorTree behaviorTree;

        [Header("Stunned")] 
        private bool isStunned;
        public bool IsStunned => isStunned;
        private Coroutine stunnedCoroutine;
        [SerializeField] protected ParticleSystem onStunParticle;
        
        [Header("Hacking")]
        [SerializeField] private bool canBeHacked = true;
        [SerializeField] protected float hackingDuration = 3f;
        [SerializeField] protected float successChance = 0.7f;
        [SerializeField] protected GameObject rootRenderer;
        protected virtual bool CanBeHacked => canBeHacked; // 오버라이드해서 false로 바꾸거나, 인스펙터에서 설정
        private Coroutine hackingCoroutine;
        private bool isHacking;
        
        protected abstract void PlayHitAnimation();
        protected abstract void PlayDeathAnimation();
        protected abstract void HackingFailurePenalty();
        
        protected virtual void Awake()
        {
            animator = GetComponent<Animator>();
            behaviorTree = GetComponent<BehaviorTree>();
            rootRenderer = this.TryGetChildComponent<GameObject>("body"); // 다를 시 body로 체인지해서 묶을 것
            IsDead = false;
        }

        public virtual void OnTakeDamage(int damage)
        {
            if (IsDead) return;

            float armorRatio = RuntimeStatData.Armor / RuntimeStatData.MaxArmor;
            float reducePercent = Mathf.Clamp01(armorRatio);
            damage = (int)(damage * (1f - reducePercent));

            RuntimeStatData.MaxHealth -= damage;

            if (RuntimeStatData.MaxHealth <= 0)
            {
                behaviorTree.SetVariableValue("IsDead", true);
                PlayDeathAnimation();
                IsDead = true;
            }
            else
            {
                PlayHitAnimation();
            }
        }
        
        #region 상호작용
        public void Hacking()
        {
            if (!CanBeHacked) return;
            if (isHacking || RuntimeStatData.IsAlly) return;

            if (isStunned)
            {
                HackingSuccess();
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
                HackingSuccess();
            }
            else
            {
                HackingFailurePenalty();
            }

            isHacking = false;
            hackingCoroutine = null;
        }
        
        private void HackingSuccess()
        {
            RuntimeStatData.IsAlly = true;

            if (rootRenderer.layer == LayerConstants.StencilEnemy)
            {
                NpcUtil.SetLayerRecursively(rootRenderer, LayerConstants.StencilAlly);
            }
            else
            {
                NpcUtil.SetLayerRecursively(gameObject, LayerConstants.Ally);
            }

            CoreManager.Instance.gameManager.Player.PlayerCondition.OnRecoverFocusGauge(FocusGainType.Hack);
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
        
        /// <summary>
        /// 자식마다 들고있는 런타임 스탯에 특정 인터페이스가 있는지 검사 후, 그 인터페이스를 반환
        /// </summary>
        /// <param name="result"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public bool TryGetRuntimeStatInterface<T>(out T result) where T : class
        {
            result = null;

            result = RuntimeStatData as T;
            if (result == null)
            {
                Debug.LogWarning($"{GetType().Name}의 RuntimeStatData에 {typeof(T).Name} 인터페이스가 없음");
                return false;
            }

            return true;
        }

        /// <summary>
        /// 외부에서 사망 처리 해야한다면 사용
        /// </summary>
        public void Dead()
        {
            IsDead = true;
        }
    }
}