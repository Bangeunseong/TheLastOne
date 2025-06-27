using System.Collections;
using System.Collections.Generic;
using _1.Scripts.Entity.Scripts.NPC.AIControllers;
using _1.Scripts.Entity.Scripts.NPC.AIControllers.Enemy;
using _1.Scripts.Entity.Scripts.NPC.BehaviorTree;
using UnityEngine;

namespace _1.Scripts.Entity.Scripts.NPC.AIBehaviors.Drone
{
    /// <summary>
    /// 알람 타이머 시작 노드
    /// </summary>
    public class AlertTimerStart : INode
    {
        public INode.State Evaluate(BaseNpcAI controller)
        {
            // EnemyReconDroneAIController 타입인지 확인
            if (controller.TryGetAIController<EnemyReconDroneAIController>(out var reconDrone))
            {
                reconDrone.TimerStartIfNull();
                return INode.State.SUCCESS;
            }

            // EnemySuicideDroneAIController 타입인지 확인
            if (controller.TryGetAIController<EnemySuicideDroneAIController>(out var suicideDrone))
            {
                suicideDrone.TimerStartIfNull();
                return INode.State.SUCCESS;
            }
            
            return INode.State.FAILED;
        }
    }
}