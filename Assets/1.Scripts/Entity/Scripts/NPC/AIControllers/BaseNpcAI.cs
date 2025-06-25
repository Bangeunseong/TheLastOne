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
        public EntityStatData statData; // 자신이 소유한 스탯데이터
        
        [Header("Node Information")]
        private bool currentActionRunning; // 현재 액션 노드 (중첩 방지)
        protected SelectorNode rootNode; // 최상위 셀렉터 노드
        
        // 각 NPC가 자신의 행동 트리를 정의하도록 강제
        protected abstract void BuildTree();
        
        protected virtual void Awake()
        {
            rootNode = new SelectorNode();
            navMeshAgent = GetComponent<NavMeshAgent>();
        }

        protected virtual void Start()
        {
            BuildTree();
        }
        
        protected virtual void Update()
        {
            if (currentActionRunning)
            {
                return; // 지속실행
            }

            // 아니면 rootNode Evaluate
            rootNode?.Evaluate(this);
        }

        /// <summary>
        /// 현재 실행중인 액션노드 있는지 여부 설정
        /// </summary>
        /// <param name="running"></param>
        public void IsCurrentActionRunning(bool running)
        {
            currentActionRunning = running;
        }
        
        /// <summary>
        /// 자식 스탯 정보가 필요할 때 인터페이스 기반으로 불러낼 것
        /// </summary>
        /// <param name="result"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public bool TryGetStatInterface<T>(out T result) where T : class
        {
            result = null;
            
            result = statData as T;
            if (result == null)
            {
                Debug.LogWarning($"statData가 {typeof(T).Name}을 구현하지 않음. 컨트롤러: {name} {GetType().Name}");
                return false;
            }
            
            return true;
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
                Debug.LogWarning($"컨트롤러가 {typeof(T).Name} 타입이 아님. 실제 타입: {GetType().Name}");
                return false;
            }

            return true;
        }
        
    }
}