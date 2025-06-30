using System;
using System.Collections;
using System.Collections.Generic;
using _1.Scripts.Entity.Scripts.Common;
using _1.Scripts.Entity.Scripts.NPC.BehaviorTree;
using _1.Scripts.Entity.Scripts.Npc.StatControllers;
using _1.Scripts.Manager.Core;
using UnityEngine;
using UnityEngine.AI;

namespace _1.Scripts.Entity.Scripts.NPC.AIControllers
{
    /// <summary>
    /// Npc 공통 동작 정의
    /// </summary>
    public abstract class BaseNpcAI : MonoBehaviour
    {
        [Header("AI Information")]
        public NavMeshAgent navMeshAgent;
        public BaseNpcStatController statController; // 자신이 소유한 스탯컨트롤러
        public bool shouldLookTarget = false;
        public Transform targetTransform;
        public Vector3 targetPos;
        private Collider _cachedCollider;
        public Vector3 MyPos
        {
            get
            {
                if (_cachedCollider == null)
                {
                    _cachedCollider = GetComponent<Collider>();
                    if (_cachedCollider == null)
                    {
                        Debug.LogWarning($"{name}에 Collider가 없습니다!");
                        return transform.position; // 안전하게 fallback
                    }
                }
                return _cachedCollider.bounds.center;
            }
        }
        
        [Header("Node Information")]
        protected bool currentActionRunning; // 현재 액션 (중첩 방지)
        protected SelectorNode rootNode; // 최상위 셀렉터 노드

        [Header("Animation Information")]
        public Animator animator;
        
        // 각 NPC가 자신의 행동 트리를 정의하도록 강제
        protected abstract void BuildTree();
        
        protected virtual void Awake()
        {
            rootNode = new SelectorNode();
            navMeshAgent = GetComponent<NavMeshAgent>();
            statController = GetComponent<BaseNpcStatController>();
            animator = GetComponent<Animator>();
        }

        protected virtual void Start()
        {
            BuildTree();
        }
        
        protected virtual void Update()
        {
            if (!statController.isDead)
            {
                if (shouldLookTarget)
                {
                    LookTarget();
                }
                
                if (currentActionRunning)
                {
                    return; // 지속실행
                }

                // 아니면 rootNode Evaluate
                rootNode?.Evaluate(this);
            }
        }

        /// <summary>
        /// 현재 실행중인 액션노드 있는지 여부 설정
        /// </summary>
        /// <param name="running"></param>
        public virtual void IsCurrentActionRunning(bool running)
        {
            currentActionRunning = running;
        }
        
        /// <summary>
        /// BaseNpcAI 말고 자식 클래스 기능이 필요할 때 사용할것
        /// </summary>
        /// <param name="result"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public bool TryGetAIController<T>(out T result) where T : BaseNpcAI
        {
            result = null;
        
            result = this as T;
            if (result == null)
            {
                return false;
            }

            return true;
        }
        
        private void LookTarget()
        {
            if (targetTransform != null)
            {
                Vector3 direction = targetTransform.position - transform.position;
                direction.y = 0; // 수평 회전만

                if (direction != Vector3.zero)
                {
                    Quaternion targetRotation = Quaternion.LookRotation(direction);
                    transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 5f);
                }
            }
        }

        public virtual void HackingNpc()
        {
            statController.Hacking();
        }
    }
}