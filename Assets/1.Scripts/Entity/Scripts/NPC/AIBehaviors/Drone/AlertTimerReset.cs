using System.Collections;
using System.Collections.Generic;
using _1.Scripts.Entity.Scripts.NPC.AIControllers;
using _1.Scripts.Entity.Scripts.NPC.AIControllers.Enemy;
using _1.Scripts.Entity.Scripts.NPC.BehaviorTree;
using UnityEngine;

namespace _1.Scripts.Entity.Scripts.NPC.AIBehaviors.Drone
{
    /// <summary>
    /// 알람 타이머 리셋 (플레이어가 계속 보일 시)
    /// </summary>
    public class AlertTimerReset : INode
    {
        public INode.State Evaluate(BaseNpcAI controller)
        {
            // EnemyReconDroneAIController 타입인지 확인
            if (controller is EnemyReconDroneAIController reconDrone)
            {
                reconDrone.ResetTimer();
            }

            // EnemySuicideDroneAIController 타입인지 확인
            if (controller is EnemySuicideDroneAIController suicideDrone)
            {
                suicideDrone.ResetTimer();
            }

            return INode.State.SUCCESS;
        }
    }
}