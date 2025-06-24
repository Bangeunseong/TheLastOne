using System;
using System.Collections;
using System.Collections.Generic;
using _1.Scripts.Entity.Scripts.Common;
using _1.Scripts.Entity.Scripts.NPC.BehaviorTree;
using _1.Scripts.Entity.Scripts.Npc.StatControllers;
using _1.Scripts.Manager.Core;
using UnityEngine;

namespace _1.Scripts.Entity.Scripts.NPC.AIControllers
{
    /// <summary>
    /// Npc 공통 동작 정의
    /// </summary>
    public abstract class BaseNpcAI : MonoBehaviour
    {
        [Header("AI Information")]
        private BTContext context; // AI컨토를러 정보
        public EntityStatData statData; // 자신이 소유한 스탯데이터
        
        [Header("Node Information")]
        private bool currentActionRunning; // 현재 액션 노드 (중첩 방지)
        protected SelectorNode rootNode; // 최상위 셀렉터 노드
        
        // 각 NPC가 자신의 행동 트리를 정의하도록 강제
        protected abstract void BuildTree();
        
        protected virtual void Awake()
        {
            if (CoreManager.Instance.gameManager.Player == null)
            {
                Debug.Log("플레이어 없음");
            }
            rootNode = new SelectorNode();
            context = new BTContext();
            context.controller = this;
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