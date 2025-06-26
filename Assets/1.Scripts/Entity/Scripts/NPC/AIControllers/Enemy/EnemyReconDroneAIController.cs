using System;
using System.Collections;
using System.Collections.Generic;
using _1.Scripts.Entity.Scripts.NPC.AIBehaviors;
using _1.Scripts.Entity.Scripts.NPC.AIBehaviors.Drone;
using _1.Scripts.Entity.Scripts.NPC.AIBehaviors.Drone.ReconDrone;
using _1.Scripts.Entity.Scripts.NPC.AIControllers;
using _1.Scripts.Entity.Scripts.NPC.BehaviorTree;
using _1.Scripts.Entity.Scripts.NPC.Data;
using _1.Scripts.Manager.Core;
using UnityEngine;

namespace _1.Scripts.Entity.Scripts.NPC.AIControllers.Enemy
{
    /// <summary>
    /// 드론(Recon Drone) 
    /// </summary>
    public class EnemyReconDroneAIController : BaseNpcAI
    {
        [Header("Alert")]
        private bool isAlerted = false;

        [Header("Timer")] 
        private Coroutine timerCoroutine = null;
        private float timer = 0f;
        
        protected override void Start()
        {
            // 액션노드 추가해서 등장연출 가능
            base.Start();
        }
        
        protected override void BuildTree()
        {
            // 1-1 공격 시퀀스 등록
            var attackSequenceNode = new SequenceNode();
            
            // 1-2 공격사거리 내인지 감지 및 공격
            var isPlayerInAttackRange = new ActionNode(new IsPlayerInAttackRange().Evaluate);
            var reconDroneAttack = new ActionNode(new ReconDroneAttacking().Evaluate);
            attackSequenceNode.Add(isPlayerInAttackRange);
            attackSequenceNode.Add(reconDroneAttack);
            
            
            // 2-1 추적 시퀀스 등록
            var chaseSequenceNode = new SequenceNode();
            
            // 2-2 추적상태인지 판단하는 셀렉터, 추적하는 셀렉터 등록
            var chaseCheckSelectorNode = new SelectorNode();
            var chasingPlayer = new ActionNode(new SetDestinationToPlayer().Evaluate);
            chaseSequenceNode.Add(chaseCheckSelectorNode);
            chaseSequenceNode.Add(chasingPlayer);
            
            // 2-3 chaseCheckSelectorNode -> 알람상태인지 검사하는 시퀀스 노드, 탐지거리 내인지 검사하는 시퀀스 노드 등록
            // chasingSelectorNode -> 추적중인 상태인지 판단하는 시퀀스 노드, 맹목적 RUN 반환 액션노드 등록
            var checkAlertSequenceNode = new SequenceNode();
            var checkPlayerInDetectRangeSequenceNode = new SequenceNode();
            chaseCheckSelectorNode.Add(checkAlertSequenceNode);
            chaseCheckSelectorNode.Add(checkPlayerInDetectRangeSequenceNode);
            
            // 2-4 checkAlertSequenceNode -> 알림상태인지 판단 액션노드, 타이머 실행 액션노드, 타이머 관리 셀렉터 등록
            // checkPlayerInDetectRangeSequenceNode -> 탐지거리 내인지 체크 액션노드, 주변 알람 활성화 액션노드 등록
            var isDroneAlerted = new ActionNode(new IsDroneAlerted().Evaluate);
            var timerStart = new ActionNode(new AlertTimerStart().Evaluate);
            var timerControlSelectorNode = new SelectorNode();
            checkAlertSequenceNode.Add(isDroneAlerted);
            checkAlertSequenceNode.Add(timerStart);
            checkAlertSequenceNode.Add(timerControlSelectorNode);
            var isPlayerInDetectRange = new ActionNode(new IsPlayerInDetectRange().Evaluate);
            var alertNearByDrones = new ActionNode(new AlertNearbyDrones().Evaluate);
            checkPlayerInDetectRangeSequenceNode.Add(isPlayerInDetectRange);
            checkPlayerInDetectRangeSequenceNode.Add(alertNearByDrones);
            
            // 2-5 timerControlSelectorNode -> 타이머 리셋하는 시퀀스 노드, 타이머 지날 시 어그로 풀리는 시퀀스 노드, 맹목적 Success반환 액션노드 등록
            var timerResetSequenceNode = new SequenceNode();
            var lostPlayerSequenceNode = new SequenceNode();
            var returnSuccess = new ActionNode(new ReturnSuccess().Evaluate);
            timerControlSelectorNode.Add(timerResetSequenceNode);
            timerControlSelectorNode.Add(lostPlayerSequenceNode);
            timerControlSelectorNode.Add(returnSuccess);
            
            // 2-6 timerResetSequenceNode -> 플레이어가 시야각 내 있는지 검사하는 액션노드, 타이머 초기화하는 액션노드 등록
            // lostPlayerInFOVSequenceNode -> 타이머가 일정시간 이상 흘렀는지 검사하는 액션노드, 알람상태 전부 초기화하는 액션노드 등록
            var isPlayerInFOV = new ActionNode(new IsPlayerInFOV().Evaluate);
            var alertTimerReset = new ActionNode(new AlertTimerReset().Evaluate);
            timerResetSequenceNode.Add(isPlayerInFOV);
            timerResetSequenceNode.Add(alertTimerReset);
            var isAlertTimerElapsed = new ActionNode(new IsAlertTimerElapsed().Evaluate);
            var alertStateReset = new ActionNode(new AlertStateReset().Evaluate);
            lostPlayerSequenceNode.Add(isAlertTimerElapsed); 
            lostPlayerSequenceNode.Add(alertStateReset);
            
            
            // 3-1 배회 셀렉터 등록
            var patrolSelectorNode = new SelectorNode();
            
            // 3-2 patrolSelectorNode -> 지정된 경로 있는지 판단할 시퀀스 노드 등록, 맹목적 RUN반환 액션노드 등록
            var isNavMeshAgentNotHasPathSequenceNode = new SequenceNode();
            var returnRun = new ActionNode(new ReturnRun().Evaluate);
            patrolSelectorNode.Add(isNavMeshAgentNotHasPathSequenceNode);
            patrolSelectorNode.Add(returnRun); 
            
            // 3-3 isNavMeshAgentNotHasPathSequenceNode -> 지정된 경로 있는지 검사하는 액션노드, 랜덤위치 지정 액션노드 등록
            var isNavMeshAgentNotHasPath = new ActionNode(new IsNavMeshAgentNotHasPath().Evaluate);
            isNavMeshAgentNotHasPathSequenceNode.Add(isNavMeshAgentNotHasPath);
            var setDestinationToPatrol = new ActionNode(new SetDestinationToPatrol().Evaluate);
            isNavMeshAgentNotHasPathSequenceNode.Add(setDestinationToPatrol);
            
            
            // 최종 - rootNode에 모두 등록
            rootNode.Add(attackSequenceNode);
            rootNode.Add(chaseSequenceNode);
            rootNode.Add(patrolSelectorNode);
        }

        public bool IsAlertedCheck()
        {
            return isAlerted;
        }
        
        public void SetAlert(bool alert)
        {
            isAlerted = alert;
        }

        public void TimerStartIfNull()
        {
            timerCoroutine ??= StartCoroutine(TimerStart()); // null이면 코루틴 실행
        }

        public void ResetAll()
        {
            SetAlert(false);
            if (timerCoroutine != null)
            {
                StopCoroutine(timerCoroutine);
                timerCoroutine = null;
            }
            ResetTimer();
        }
        
        public void ResetTimer()
        {
            timer = 0f;
        }
        
        public float TimerCheck()
        {
            return timer;
        }

        private IEnumerator TimerStart()
        {
            while (true)
            {
                timer += Time.deltaTime;
                yield return null;
            }   
        }
    }
}