using System;
using System.Collections;
using System.Collections.Generic;
using _1.Scripts.Entity.Scripts.NPC.AIBehaviors;
using _1.Scripts.Entity.Scripts.NPC.AIBehaviors.Drone;
using _1.Scripts.Entity.Scripts.NPC.AIBehaviors.Drone.ReconDrone;
using _1.Scripts.Entity.Scripts.NPC.AIBehaviors.Drone.SuicideDrone;
using _1.Scripts.Entity.Scripts.NPC.AIControllers;
using _1.Scripts.Entity.Scripts.NPC.AIControllers.Base;
using _1.Scripts.Entity.Scripts.NPC.BehaviorTree;
using _1.Scripts.Entity.Scripts.NPC.Data;
using _1.Scripts.Manager.Core;
using UnityEngine;

namespace _1.Scripts.Entity.Scripts.NPC.AIControllers.Drone
{
    /// <summary>
    /// 드론(Suicide Drone) 
    /// </summary>
    public class SuicideDroneAIController : BaseDroneAIController
    {
        protected override void Start()
        {
            // 액션노드 추가해서 등장연출 가능
            base.Start();
        }

        protected override void Update()
        {
            base.Update();
        }
    
        protected override void BuildTree()
        {
            // 1-1 알람 상태 체크 시퀀스 등록
            var isNotAlertedSequenceNode = new SequenceNode();
            
            // 1-2 알람상태가 아닐 시, 드론 비활성화
            var isNotAlerted = new ActionNode(new IsDroneNotAlerted().Evaluate);
            var resetSuicideDroneState = new ActionNode(new ResetSuicideDroneState().Evaluate);
            isNotAlertedSequenceNode.Add(isNotAlerted);
            isNotAlertedSequenceNode.Add(resetSuicideDroneState);
            
            // 2-1 공격 시퀀스 등록
            var attackSequenceNode = new SequenceNode();
            
            // 2-2 공격사거리 내일 시, 자폭공격
            var isEnemyInAttackRange = new ActionNode(new IsEnemyInAttackRange().Evaluate);
            var suicideDroneAttack = new ActionNode(new SuicideDroneAttack().Evaluate);
            attackSequenceNode.Add(isEnemyInAttackRange);
            attackSequenceNode.Add(suicideDroneAttack);
            
            
            // 3-1 추적 시퀀스 등록
            var chaseSequenceNode = new SequenceNode();
            
            // 3-2 타이머 컨트롤, 적 추적 액션노드 등록
            var timerControlSequenceNode = new SequenceNode();
            var chasingEnemy = new ActionNode(new SetDestinationToEnemy().Evaluate);
            chaseSequenceNode.Add(timerControlSequenceNode);
            chaseSequenceNode.Add(chasingEnemy);
            
            // 3-3 알람 상태인지 확인 후 타겟 사거리 내에서 계속 갱신하는 액션노드 & 타이머 실행 액션노드 & 타이머 갱신 또는 초기화 셀렉터
            var isDroneAlerted = new ActionNode(new IsDroneAlerted().Evaluate);
            var alertTimerStart = new ActionNode(new AlertTimerStart().Evaluate);
            var timerControlSelectorNode = new SelectorNode();
            timerControlSequenceNode.Add(isDroneAlerted);
            timerControlSequenceNode.Add(alertTimerStart);
            timerControlSequenceNode.Add(timerControlSelectorNode);
            
            // 3-4 timerControlSelectorNode -> 타이머 리셋하는 시퀀스 노드, 타이머 지날 시 어그로 풀리는 시퀀스 노드, 맹목적 Success반환 액션노드 등록
            var timerResetSequenceNode = new SequenceNode();
            var lostEnemySequenceNode = new SequenceNode();
            var returnSuccess = new ActionNode(new ReturnSuccess().Evaluate);
            timerControlSelectorNode.Add(timerResetSequenceNode);
            timerControlSelectorNode.Add(lostEnemySequenceNode);
            timerControlSelectorNode.Add(returnSuccess);
            
            // 3-5 timerResetSequenceNode -> 적이 시야각 내 있는지 검사하는 액션노드, 타이머 초기화하는 액션노드 등록
            // lostEnemySequenceNode -> 타이머가 일정시간 이상 흘렀는지 검사하는 액션노드, 알람상태 전부 초기화하는 액션노드 등록
            var isEnemyInFOV = new ActionNode(new IsEnemyInFOV().Evaluate);
            var alertTimerReset = new ActionNode(new AlertTimerReset().Evaluate);
            timerResetSequenceNode.Add(isEnemyInFOV);
            timerResetSequenceNode.Add(alertTimerReset);
            var isAlertTimerElapsed = new ActionNode(new IsAlertTimerElapsed().Evaluate);
            var alertStateReset = new ActionNode(new AlertStateReset().Evaluate);
            lostEnemySequenceNode.Add(isAlertTimerElapsed);
            lostEnemySequenceNode.Add(alertStateReset);
            
            rootNode.Add(isNotAlertedSequenceNode);
            rootNode.Add(attackSequenceNode);
            rootNode.Add(chaseSequenceNode);
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