using System.Collections;
using System.Collections.Generic;
using _1.Scripts.Entity.Scripts.NPC.AIControllers;
using _1.Scripts.Entity.Scripts.NPC.AIControllers.Base;
using _1.Scripts.Entity.Scripts.NPC.AIControllers.Enemy;
using _1.Scripts.Entity.Scripts.NPC.BehaviorTree;
using _1.Scripts.Interfaces;
using UnityEngine;

namespace _1.Scripts.Entity.Scripts.NPC.AIBehaviors.Drone
{
    /// <summary>
    /// 알람상태 초기화 (타겟 놓칠 시)
    /// </summary>
    public class AlertStateReset : INode
    {
        public INode.State Evaluate(BaseNpcAI controller)
        {
            // EnemyReconDroneAIController 타입인지 확인
            if (controller.TryGetAIController<BaseDroneAIController>(out var droneController))
            {
                droneController.ResetAll();
                return INode.State.SUCCESS;
            }

            return INode.State.FAILED;
        }
    }
}