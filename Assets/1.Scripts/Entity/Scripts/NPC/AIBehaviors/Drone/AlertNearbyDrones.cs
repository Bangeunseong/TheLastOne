using System.Collections;
using System.Collections.Generic;
using _1.Scripts.Entity.Scripts.NPC.AIControllers;
using _1.Scripts.Entity.Scripts.NPC.AIControllers.Base;
using _1.Scripts.Entity.Scripts.NPC.AIControllers.Enemy;
using _1.Scripts.Entity.Scripts.NPC.BehaviorTree;
using _1.Scripts.Interfaces;
using _1.Scripts.Interfaces.NPC;
using _1.Scripts.Manager.Core;
using _1.Scripts.Manager.Subs;
using _1.Scripts.Static;
using UnityEngine;

namespace _1.Scripts.Entity.Scripts.NPC.AIBehaviors.Drone
{
    /// <summary>
    /// 알람 타이머 시작 노드
    /// </summary>
    public class AlertNearbyDrones : INode
    {
        public INode.State Evaluate(BaseNpcAI controller)
        {
            if (controller.TryGetAIController<BaseDroneAIController>(out var droneController))
            { 
                droneController.AlertNearbyDrones();
                return INode.State.SUCCESS;
            }

            return INode.State.FAILED;
        }
    }
}