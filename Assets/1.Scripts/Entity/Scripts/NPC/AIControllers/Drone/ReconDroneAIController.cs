using System;
using System.Collections;
using System.Collections.Generic;
using _1.Scripts.Entity.Scripts.NPC.AIBehaviors;
using _1.Scripts.Entity.Scripts.NPC.AIBehaviors.Drone;
using _1.Scripts.Entity.Scripts.NPC.AIBehaviors.Drone.ReconDrone;
using _1.Scripts.Entity.Scripts.NPC.AIControllers;
using _1.Scripts.Entity.Scripts.NPC.AIControllers.Base;
using _1.Scripts.Entity.Scripts.NPC.BehaviorTree;
using _1.Scripts.Entity.Scripts.NPC.Data;
using _1.Scripts.Interfaces;
using _1.Scripts.Interfaces.NPC;
using _1.Scripts.Manager.Core;
using UnityEngine;

namespace _1.Scripts.Entity.Scripts.NPC.AIControllers.Drone
{
    /// <summary>
    /// 드론(Recon Drone) 
    /// </summary>
    public class ReconDroneAIController : BaseDroneAIController
    {
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
            var isEnemyInAttackRange = new ActionNode(new IsEnemyInAttackRange().Evaluate);
            var reconDroneAttack = new ActionNode(new ReconDroneAttacking().Evaluate);
            attackSequenceNode.Add(isEnemyInAttackRange);
            attackSequenceNode.Add(reconDroneAttack);
            
            // 2-1 추적 시퀀스 등록
            var chaseSequenceNode = new SequenceNode();

            // 2-2 추적상태인지 판단하는 셀렉터, 추적하는 액션노드 등록
            var chaseCheckSelectorNode = new SelectorNode();
            var chasingEnemy = new ActionNode(new SetDestinationToEnemy().Evaluate);
            chaseSequenceNode.Add(chaseCheckSelectorNode);
            chaseSequenceNode.Add(chasingEnemy);

            // 2-3 chaseCheckSelectorNode -> 알람상태인지 검사하는 시퀀스 노드, 탐지거리 내인지 검사하는 시퀀스 노드 등록
            // chasingSelectorNode -> 추적중인 상태인지 판단하는 시퀀스 노드, 맹목적 RUN 반환 액션노드 등록
            var checkAlertSequenceNode = new SequenceNode();
            var checkEnemyInDetectRangeSequenceNode = new SequenceNode();
            chaseCheckSelectorNode.Add(checkAlertSequenceNode);
            chaseCheckSelectorNode.Add(checkEnemyInDetectRangeSequenceNode);

            // 2-4 checkAlertSequenceNode -> 알림상태인지 판단 액션노드, 타이머 실행 액션노드, 타이머 관리 셀렉터 등록
            // checkEnemyInDetectRangeSequenceNode -> 탐지거리 내인지 체크 액션노드, 주변 알람 활성화 액션노드 등록
            var isDroneAlerted = new ActionNode(new IsDroneAlerted().Evaluate);
            var timerStart = new ActionNode(new AlertTimerStart().Evaluate);
            var timerControlSelectorNode = new SelectorNode();
            checkAlertSequenceNode.Add(isDroneAlerted);
            checkAlertSequenceNode.Add(timerStart);
            checkAlertSequenceNode.Add(timerControlSelectorNode);
            var isEnemyInDetectRange = new ActionNode(new IsEnemyInDetectRange().Evaluate);
            var alertNearByDrones = new ActionNode(new AlertNearbyDrones().Evaluate);
            checkEnemyInDetectRangeSequenceNode.Add(isEnemyInDetectRange);
            checkEnemyInDetectRangeSequenceNode.Add(alertNearByDrones);

            // 2-5 timerControlSelectorNode -> 타이머 리셋하는 시퀀스 노드, 타이머 지날 시 어그로 풀리는 시퀀스 노드, 맹목적 Success반환 액션노드 등록
            var timerResetSequenceNode = new SequenceNode();
            var lostEnemySequenceNode = new SequenceNode();
            var returnSuccess = new ActionNode(new ReturnSuccess().Evaluate);
            timerControlSelectorNode.Add(timerResetSequenceNode);
            timerControlSelectorNode.Add(lostEnemySequenceNode);
            timerControlSelectorNode.Add(returnSuccess);

            // 2-6 timerResetSequenceNode -> 적이 시야각 내 있는지 검사하는 액션노드, 타이머 초기화하는 액션노드 등록
            // lostEnemySequenceNode -> 타이머가 일정시간 이상 흘렀는지 검사하는 액션노드, 알람상태 전부 초기화하는 액션노드 등록
            var isEnemyInFOV = new ActionNode(new IsEnemyInFOV().Evaluate);
            var alertTimerReset = new ActionNode(new AlertTimerReset().Evaluate);
            timerResetSequenceNode.Add(isEnemyInFOV);
            timerResetSequenceNode.Add(alertTimerReset);
            var isAlertTimerElapsed = new ActionNode(new IsAlertTimerElapsed().Evaluate);
            var alertStateReset = new ActionNode(new AlertStateReset().Evaluate);
            lostEnemySequenceNode.Add(isAlertTimerElapsed);
            lostEnemySequenceNode.Add(alertStateReset);

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

            IPatrolable patrolable = null;
            statController.TryGetRuntimeStatInterface<IPatrolable>(out patrolable);
            var waitingForSeconds = new ActionNode(new WaitingForSeconds(patrolable.MinWaitingDuration, patrolable.MaxWaitingDuration).Evaluate);
            isNavMeshAgentNotHasPathSequenceNode.Add(waitingForSeconds);

            var setDestinationToPatrol = new ActionNode(new SetDestinationToPatrol().Evaluate);
            isNavMeshAgentNotHasPathSequenceNode.Add(setDestinationToPatrol);

            // 최종 - rootNode에 모두 등록
            rootNode.Add(attackSequenceNode);
            rootNode.Add(chaseSequenceNode);
            rootNode.Add(patrolSelectorNode);
        }

        public void CurrentActionRunningForAnimationEvent()
        {
            currentActionRunning = true;
        }

        public void CurrentActionFinishedForAnimationEvent()
        {
            if (!isStunned)
            {
                currentActionRunning = false;
            }
        }

        public void ShouldLookAtPlayerFalseForAnimationEvent()
        {
            shouldLookTarget = false;
        }

        public void SetDestinationNullForAnimationEvent()
        {
            navMeshAgent.SetDestination(transform.position);
        }
    }
}