using System;
using System.Collections;
using System.Collections.Generic;
using _1.Scripts.Entity.Scripts.NPC.BehaviorTree;
using UnityEngine;

namespace _1.Scripts.Entity.Scripts.NPC.AIControllers
{
    /// <summary>
    /// Npc 공통 동작 정의
    /// </summary>
    public abstract class BaseNpcAI : MonoBehaviour
    {
        private bool currentActionRunning; // 현재 액션 노드 (중첩 방지)
        private BTContext context; // AI컨토를러 정보
        protected SelectorNode rootNode; // 최상위 셀렉터 노드
        // 스탯 정보 SO 필요
        
        // 각 몬스터가 자신의 행동 트리를 정의하도록 강제
        protected abstract void BuildTree();
        
        protected virtual void Awake()
        {
            rootNode = new SelectorNode();
            // context = new BTContext(this, 플레이어 위치, transform.position);
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
            rootNode?.Evaluate(context);
        }

        /// <summary>
        /// 현재 실행중인 액션노드 있는지 여부 설정
        /// </summary>
        /// <param name="running"></param>
        public void IsCurrentActionRunning(bool running)
        {
            currentActionRunning = running;
        }
    }
}