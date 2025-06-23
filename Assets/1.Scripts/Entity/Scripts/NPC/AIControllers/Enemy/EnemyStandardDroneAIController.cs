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
        protected override void BuildTree()
        {
            ActionNode rolling = new ActionNode(new RollActionNode().Evaluate);
            rootNode.Add(rolling);
        }
    }
}
