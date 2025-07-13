using System;
using System.Collections;
using System.Threading;
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
using _1.Scripts.UI.InGame;
using BehaviorDesigner.Runtime;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace _1.Scripts.Entity.Scripts.Npc.StatControllers.Base
{
    public enum EnemyType
    {
        ReconDrone,
        BattleRoomReconDrone,
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
        private CancellationTokenSource stunToken;
        [SerializeField] protected ParticleSystem onStunParticle;
        
        [Header("Hacking")]
        public bool isHacking;
        [SerializeField] private bool canBeHacked = true;
        [SerializeField] protected float hackingDuration = 3f;
        [SerializeField] protected float successChance = 0.7f;
        [SerializeField] protected Transform rootRenderer;
        protected virtual bool CanBeHacked => canBeHacked; // 오버라이드해서 false로 바꾸거나, 인스펙터에서 설정
        private HackingProgressUI hackingProgressUI;
        
        protected abstract void PlayHitAnimation();
        protected abstract void PlayDeathAnimation();

        protected abstract void HackingFailurePenalty();

        
        protected virtual void Awake()
        {
            animator = GetComponent<Animator>();
            behaviorTree = GetComponent<BehaviorTree>();
            rootRenderer = this.TryGetChildComponent<Transform>("DronBot"); 
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
            if (!CanBeHacked || isHacking || RuntimeStatData.IsAlly) return;

            var obj = CoreManager.Instance.objectPoolManager.Get("HackingProgressUI");
            hackingProgressUI = obj.GetComponent<HackingProgressUI>();
            hackingProgressUI.SetTarget(transform);
            hackingProgressUI.gameObject.SetActive(true);
            hackingProgressUI.SetProgress(0f);
            
            if (isStunned)
            {
                hackingProgressUI.SetProgress(1f);
                HackingSuccess();
                return;
            }

            _ = HackingProcessAsync();
        }

        private async UniTaskVoid HackingProcessAsync()
        {
            isHacking = true;
            
            // 1. 드론 멈추기
            float stunDurationOnHacking = hackingDuration + 1f; // 스턴 중 해킹결과가 영향 끼치지 않게 더 길게 설정
            OnStunned(stunDurationOnHacking);

            // 2. 해킹 시도 시간 기다림
            float time = 0f;
            while (time < hackingDuration)
            {
                time += Time.deltaTime;
                hackingProgressUI.SetProgress(time / hackingDuration);
                await UniTask.Yield(PlayerLoopTiming.Update);
            }

            // 3. 확률 판정
            bool success = UnityEngine.Random.value < successChance;

            if (success)
            {
                HackingSuccess();
            }
            else
            {
                hackingProgressUI.OnFail();
                HackingFailurePenalty();
            }

            isHacking = false;
        }
        
        private void HackingSuccess()
        {
            RuntimeStatData.IsAlly = true;

            if (rootRenderer.gameObject.layer == LayerConstants.StencilEnemy)
            {
                NpcUtil.SetLayerRecursively(gameObject, LayerConstants.Ally);
                NpcUtil.SetLayerRecursively(rootRenderer.gameObject, LayerConstants.StencilAlly);
            }
            else
            {
                NpcUtil.SetLayerRecursively(gameObject, LayerConstants.Ally);
            }

            CoreManager.Instance.gameManager.Player.PlayerCondition.OnRecoverFocusGauge(FocusGainType.Hack);
            hackingProgressUI.OnSuccess();
        }
        
        public void OnStunned(float duration = 3f)
        {   
            stunToken?.Cancel();
            stunToken?.Dispose();
            stunToken = new CancellationTokenSource();
            _ = OnStunnedAsync(duration, stunToken.Token);
        }
        
        private async UniTaskVoid OnStunnedAsync(float duration, CancellationToken token)
        {
            isStunned = true;
            behaviorTree.SetVariableValue("CanRun", false);
            ResetAIState();
            
            onStunParticle.Play();
            await UniTask.WaitForSeconds(duration, cancellationToken:token); // 원하는 시간만큼 유지

            if (onStunParticle != null && onStunParticle.IsAlive())
            {
                onStunParticle.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            }

            isStunned = false;
            behaviorTree.SetVariableValue("CanRun", true);
        }
        
        protected virtual void ResetAIState()
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