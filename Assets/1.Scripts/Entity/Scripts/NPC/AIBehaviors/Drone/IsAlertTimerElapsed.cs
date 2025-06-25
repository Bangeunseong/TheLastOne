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
    public class IsAlertTimerElapsed : INode
    {
        public INode.State Evaluate(BaseNpcAI controller)
        {
            if (!controller.TryGetStatInterface<IAlertable>(out var alertable)) // 있을 시 변환
            {
                return INode.State.FAILED;
            }
            
            // EnemyReconDroneAIController 타입인지 확인
            if (controller.TryGetAIController<EnemyReconDroneAIController>(out var reconDrone))
            {
                return reconDrone.TimerCheck() <= alertable.AlertDuration ?  INode.State.SUCCESS : INode.State.FAILED;
            }

            // EnemySuicideDroneAIController 타입인지 확인
            if (controller is EnemySuicideDroneAIController suicideDrone)
            {
                return suicideDrone.TimerCheck() <= alertable.AlertDuration ?  INode.State.SUCCESS : INode.State.FAILED;
            }
            
            return INode.State.FAILED;
        }
    }
}