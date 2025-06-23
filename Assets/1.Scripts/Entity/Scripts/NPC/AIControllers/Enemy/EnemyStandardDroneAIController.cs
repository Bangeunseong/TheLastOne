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
        void Start()
        {
            BuildTree();
        }
        
        protected override void BuildTree()
        {
            RollActionNode rollActionNode = new RollActionNode();
            ActionNode rolling = new ActionNode(context => rollActionNode.Evaluate(context));
            rootNode.Add(rolling);
        }
    }
}
