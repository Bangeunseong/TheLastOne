using System;
using System.Collections;
using System.Collections.Generic;
using _1.Scripts.Entity.Scripts.NPC.AIBehaviors;
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
        private bool isAlerted = false;

        protected override void Start()
        {
            statData = CoreManager.Instance.resourceManager.GetAsset<ReconDroneData>("ReconDroneData");
            // 액션노드 추가해서 등장연출 가능
            base.Start();
        }
        
        protected override void BuildTree()
        {
            // 공격 시퀀스 등록
            SequenceNode attackSequenceNode = new SequenceNode();
            ActionNode isPlayerInAttackRange = new ActionNode(new IsPlayerInAttackRange().Evaluate);
            ActionNode reconDroneAttack = new ActionNode(new ReconDroneAttacking().Evaluate);
            attackSequenceNode.Add(isPlayerInAttackRange);
            attackSequenceNode.Add(reconDroneAttack);
            
            // 추적 시퀀스 등록
            SequenceNode chaseSequenceNode = new SequenceNode();
            SelectorNode chaseCheckSelectorNode = new SelectorNode();
            SelectorNode chasingSelectorNode = new SelectorNode();
            chaseSequenceNode.Add(chaseCheckSelectorNode);
            chaseSequenceNode.Add(chasingSelectorNode);
            
            SequenceNode checkAlertSequenceNode = new SequenceNode();
            SequenceNode checkPlayerInDetectRangeSequenceNode = new SequenceNode();
            
            
            ActionNode isPlayerInDetectRange = new ActionNode(new IsPlayerInDetectRange().Evaluate);
            rootNode.Add(isPlayerInDetectRange);
        }
        
        public void SetAlert(bool alert)
        {
            isAlerted = alert;
        }
    }
}