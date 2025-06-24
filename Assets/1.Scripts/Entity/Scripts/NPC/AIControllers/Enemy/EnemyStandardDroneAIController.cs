using System;
using System.Collections;
using System.Collections.Generic;
using _1.Scripts.Entity.Scripts.NPC.AIBehaviors;
using _1.Scripts.Entity.Scripts.NPC.AIControllers;
using _1.Scripts.Entity.Scripts.NPC.BehaviorTree;
using UnityEngine;

namespace _1.Scripts.Entity.Scripts.NPC.AIControllers.Enemy
{
    public class EnemyStandardDroneAIController : BaseNpcAI
    {
        protected override void Start()
        {
            // 액션노드 추가해서 등장연출 가능
            base.Start();
        }
        
        protected override void BuildTree()
        {
            ActionNode rolling = new ActionNode(new RollActionNode().Evaluate);
            rootNode.Add(rolling);
        }
    }
}
