using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using _1.Scripts.Entity.Scripts.Common;
using _1.Scripts.Entity.Scripts.NPC.AIBehaviors.BehaviorDesigner.SharedVariables;
using _1.Scripts.Entity.Scripts.NPC.Data.ForRuntime;
using _1.Scripts.Entity.Scripts.Player.Data;
using _1.Scripts.Interfaces;
using _1.Scripts.Interfaces.Common;
using _1.Scripts.Interfaces.NPC;
using _1.Scripts.Manager.Core;
using _1.Scripts.Manager.Subs;
using _1.Scripts.Quests.Core;
using _1.Scripts.Static;
using _1.Scripts.Util;
using _1.Scripts.UI.InGame;
using BehaviorDesigner.Runtime;
using Cysharp.Threading.Tasks;
using UnityEditor;
using UnityEngine;
using UnityEngine.AI;

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
        protected NavMeshAgent agent;
        private Collider[] colliders;
        private Light[] lights;
        
        [Header("Stunned")] 
        private bool isStunned;
        public bool IsStunned => isStunned;
        private CancellationTokenSource stunToken;
        [SerializeField] protected ParticleSystem onStunParticle;
        
        [Header("Hacking_Process")]
        public bool isHacking;
        [SerializeField] private bool canBeHacked = true;
        [SerializeField] protected float hackingDuration = 3f;
        [SerializeField] protected float successChance = 0.7f;
        [SerializeField] protected Transform rootRenderer;
        protected virtual bool CanBeHacked => canBeHacked; // 오버라이드해서 false로 바꾸거나, 인스펙터에서 설정
        private HackingProgressUI hackingProgressUI;
        private Dictionary<Transform, int> originalLayers = new();

        [Header("Hacking_Quest")] // 해킹 성공 시 올려야할 퀘스트 진행도들 
        [SerializeField] private bool shouldCountHackingQuest;
        [SerializeField] private int[] hackingQuestIndex;
        
        [Header("Kill_Quest")] // 사망 시 올려야할 퀘스트 진행도들
        [SerializeField] private bool shouldCountKillQuest;
        [SerializeField] private int[] killQuestIndex;
        
        protected virtual void Awake()
        {
            animator = GetComponent<Animator>();
            behaviorTree = GetComponent<BehaviorTree>();
            agent = GetComponent<NavMeshAgent>();
            lights = GetComponentsInChildren<Light>();
            colliders = GetComponentsInChildren<Collider>();
            IsDead = false;
            
            CacheOriginalLayers(this.transform);
        }
        
        /// <summary>
        /// 풀링 사용하므로 반드시 반환될때마다 초기화 해야함
        /// </summary>
        protected virtual void OnDisable()
        {
            IsDead = false;
            isHacking = false;
            isStunned = false;
            agent.enabled = false;
            
            ResetLayersToOriginal();
        }

        protected abstract void PlayHitAnimation();
        protected abstract void PlayDeathAnimation();
        protected abstract void HackingFailurePenalty();

        public virtual void OnTakeDamage(int damage)
        {
            if (IsDead) return;
            
            float armorRatio = RuntimeStatData.Armor / RuntimeStatData.MaxArmor;
            float reducePercent = Mathf.Clamp01(armorRatio);
            damage = (int)(damage * (1f - reducePercent));

            RuntimeStatData.MaxHealth -= damage;

            if (RuntimeStatData.MaxHealth <= 0) // 사망
            {
                if (shouldCountKillQuest && !RuntimeStatData.IsAlly)
                {
                    foreach (int index in killQuestIndex) GameEventSystem.Instance.RaiseEvent(index);
                }

                foreach (Light objlight in lights) { objlight.enabled = false; }
                foreach (Collider coll in colliders) { coll.enabled = false; }
                
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
            if (IsDead || !CanBeHacked || isHacking || RuntimeStatData.IsAlly) return;

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

            if (shouldCountHackingQuest)
            {
                foreach (int index in hackingQuestIndex) {GameEventSystem.Instance.RaiseEvent(index);}
            }
            
            CoreManager.Instance.gameManager.Player.PlayerCondition.OnRecoverFocusGauge(FocusGainType.Hack);
            hackingProgressUI.OnSuccess();
        }
        
        public void OnStunned(float duration = 3f)
        {
            if (IsDead) return;
            
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
        
        /// <summary>
        /// 첫 레이어 상태들을 재귀적으로 저장
        /// </summary>
        /// <param name="parent"></param>
        private void CacheOriginalLayers(Transform parent = null)
        {
            if (parent == null) parent = transform;
            
            if (!originalLayers.ContainsKey(parent))
            {
                originalLayers.Add(parent, parent.gameObject.layer);
            }

            foreach (Transform child in parent)
            {
                CacheOriginalLayers(child);
            }
        }
        
        /// <summary>
        /// OnEnable 시 레이어 기본으로 되돌림
        /// </summary>
        private void ResetLayersToOriginal()
        {
            foreach (var kvp in originalLayers)
            {
                if (kvp.Key != null) // 혹시 파괴된 오브젝트가 있을 수 있으니 체크
                {
                    kvp.Key.gameObject.layer = kvp.Value;
                }
            }
        }
    }
}