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
            statData = CoreManager.Instance.resourceManager.GetAsset<ReconDroneData>("ReconDroneData");
            // 액션노드 추가해서 등장연출 가능
            base.Start();
        }
        
        protected override void BuildTree()
        {
            // 공격 시퀀스 등록
            // 1-1
            SequenceNode attackSequenceNode = new SequenceNode();
            // 1-2
            ActionNode isPlayerInAttackRange = new ActionNode(new IsPlayerInAttackRange().Evaluate);
            ActionNode reconDroneAttack = new ActionNode(new ReconDroneAttacking().Evaluate);
            attackSequenceNode.Add(isPlayerInAttackRange);
            attackSequenceNode.Add(reconDroneAttack);
            
            // 추적 시퀀스 등록
            // 2-1
            SequenceNode chaseSequenceNode = new SequenceNode();
            // 2-2
            SelectorNode chaseCheckSelectorNode = new SelectorNode();
            SelectorNode chasingSelectorNode = new SelectorNode();
            chaseSequenceNode.Add(chaseCheckSelectorNode);
            chaseSequenceNode.Add(chasingSelectorNode);
            // 2-3
            SequenceNode checkAlertSequenceNode = new SequenceNode();
            SequenceNode checkPlayerInDetectRangeSequenceNode = new SequenceNode();
            chaseCheckSelectorNode.Add(checkAlertSequenceNode);
            chaseCheckSelectorNode.Add(checkPlayerInDetectRangeSequenceNode);
            SequenceNode giveNewPathSequenceNode = new SequenceNode();
            ActionNode returnRun = new ActionNode(new ReturnRun().Evaluate); 
            chasingSelectorNode.Add(giveNewPathSequenceNode);
            chaseCheckSelectorNode.Add(returnRun);
            // 2-4
            ActionNode isDroneAlerted = new ActionNode(new IsDroneAlerted().Evaluate);
            ActionNode timerStart = new ActionNode(new AlertTimerStart().Evaluate);
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
            ResetTimer();
            if (timerCoroutine != null)
            {
                StopCoroutine(timerCoroutine);
                timerCoroutine = null;
            }
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
            timer += Time.deltaTime;
            yield return null;
        }
    }
}