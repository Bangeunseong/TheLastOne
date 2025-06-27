using System.Collections;
using System.Collections.Generic;
using _1.Scripts.Entity.Scripts.NPC.AIControllers;
using _1.Scripts.Entity.Scripts.NPC.AIControllers.Enemy;
using _1.Scripts.Entity.Scripts.NPC.BehaviorTree;
using _1.Scripts.Interfaces;
using UnityEngine;

namespace _1.Scripts.Entity.Scripts.NPC.AIBehaviors.Drone
{
    /// <summary>
    /// 알람 타이머 시작 노드
    /// </summary>
    public class AlertStateReset : INode
    {
        public INode.State Evaluate(BaseNpcAI controller)
        {
            // EnemyReconDroneAIController 타입인지 확인
            if (controller.TryGetAIController<EnemyReconDroneAIController>(out var reconDrone))
            {
                reconDrone.ResetAll();
                return INode.State.SUCCESS;
            }

            // EnemySuicideDroneAIController 타입인지 확인
            if (controller.TryGetAIController<EnemySuicideDroneAIController>(out var suicideDrone))
            {
                suicideDrone.ResetAll();
                return INode.State.SUCCESS;
            }

            return INode.State.FAILED;
        }
    }
}