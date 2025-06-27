using System;
using System.Collections;
using System.Collections.Generic;
using _1.Scripts.Entity.Scripts.NPC.AIBehaviors;
using _1.Scripts.Entity.Scripts.NPC.AIBehaviors.Drone;
using _1.Scripts.Entity.Scripts.NPC.AIBehaviors.Drone.ReconDrone;
using _1.Scripts.Entity.Scripts.NPC.AIControllers;
using _1.Scripts.Entity.Scripts.NPC.BehaviorTree;
using _1.Scripts.Entity.Scripts.NPC.Data;
using _1.Scripts.Interfaces;
using _1.Scripts.Interfaces.NPC;
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
            var attackSequenceNode = new SequenceNode();
            var isEnemyInAttackRange = new ActionNode(new IsEnemyInAttackRange().Evaluate);
            var reconDroneAttack = new ActionNode(new ReconDroneAttacking().Evaluate);
            attackSequenceNode.Add(isEnemyInAttackRange);
            attackSequenceNode.Add(reconDroneAttack);

            var chaseSequenceNode = new SequenceNode();
            var chaseCheckSelectorNode = new SelectorNode();
            var chasingEnemy = new ActionNode(new SetDestinationToEnemy().Evaluate);
            chaseSequenceNode.Add(chaseCheckSelectorNode);
            chaseSequenceNode.Add(chasingEnemy);

            var checkAlertSequenceNode = new SequenceNode();
            var checkEnemyInDetectRangeSequenceNode = new SequenceNode();
            chaseCheckSelectorNode.Add(checkAlertSequenceNode);
            chaseCheckSelectorNode.Add(checkEnemyInDetectRangeSequenceNode);

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

            var timerResetSequenceNode = new SequenceNode();
            var lostEnemySequenceNode = new SequenceNode();
            var returnSuccess = new ActionNode(new ReturnSuccess().Evaluate);
            timerControlSelectorNode.Add(timerResetSequenceNode);
            timerControlSelectorNode.Add(lostEnemySequenceNode);
            timerControlSelectorNode.Add(returnSuccess);

            var isEnemyInFOV = new ActionNode(new IsEnemyInFOV().Evaluate);
            var alertTimerReset = new ActionNode(new AlertTimerReset().Evaluate);
            timerResetSequenceNode.Add(isEnemyInFOV);
            timerResetSequenceNode.Add(alertTimerReset);

            var isAlertTimerElapsed = new ActionNode(new IsAlertTimerElapsed().Evaluate);
            var alertStateReset = new ActionNode(new AlertStateReset().Evaluate);
            lostEnemySequenceNode.Add(isAlertTimerElapsed);
            lostEnemySequenceNode.Add(alertStateReset);

            var patrolSelectorNode = new SelectorNode();
            var isNavMeshAgentNotHasPathSequenceNode = new SequenceNode();
            var returnRun = new ActionNode(new ReturnRun().Evaluate);
            patrolSelectorNode.Add(isNavMeshAgentNotHasPathSequenceNode);
            patrolSelectorNode.Add(returnRun);

            var isNavMeshAgentNotHasPath = new ActionNode(new IsNavMeshAgentNotHasPath().Evaluate);
            isNavMeshAgentNotHasPathSequenceNode.Add(isNavMeshAgentNotHasPath);

            IPatrolable patrolable = null;
            statController.TryGetRuntimeStatInterface<IPatrolable>(out patrolable);
            var waitingForSeconds =
                new ActionNode(new WaitingForSeconds(patrolable.MinWaitingDuration, patrolable.MaxWaitingDuration)
                    .Evaluate);
            isNavMeshAgentNotHasPathSequenceNode.Add(waitingForSeconds);

            var setDestinationToPatrol = new ActionNode(new SetDestinationToPatrol().Evaluate);
            isNavMeshAgentNotHasPathSequenceNode.Add(setDestinationToPatrol);

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

        public void CurrentActionRunningForAnimationEvent()
        {
            currentActionRunning = true;
        }

        public void CurrentActionFinishedForAnimationEvent()
        {
            currentActionRunning = false;
        }

        public void ShouldLookAtPlayerFalseForAnimationEvent()
        {
            shouldLookAtPlayer = false;
        }

        public void SetDestinationNullForAnimationEvent()
        {
            navMeshAgent.SetDestination(transform.position);
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