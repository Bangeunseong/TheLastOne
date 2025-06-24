using System;
using System.Collections;
using System.Collections.Generic;
using _1.Scripts.Entity.Scripts.NPC.AIBehaviors;
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
    public class EnemyStandardDroneAIController : BaseNpcAI
    {
        protected override void Start()
        {
            statData = CoreManager.Instance.resourceManager.GetAsset<StandardDroneStat>("StandardDroneStat");
            // 액션노드 추가해서 등장연출 가능
            base.Start();
        }
        
        protected override void BuildTree()
        {
            ActionNode isPlayerInDetectRange = new ActionNode(new IsPlayerInDetectRange().Evaluate);
            rootNode.Add(isPlayerInDetectRange);
        }
    }
}
