using System.Collections;
using System.Collections.Generic;
using _1.Scripts.Entity.Scripts.NPC.AIControllers;
using _1.Scripts.Entity.Scripts.NPC.AIControllers.Base;
using _1.Scripts.Entity.Scripts.NPC.BehaviorTree;
using _1.Scripts.Interfaces;
using _1.Scripts.Interfaces.NPC;
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
            if (!controller.statController.TryGetRuntimeStatInterface<IAlertable>(out var alertable)) // 있을 시 변환
            {
                return INode.State.FAILED;
            }
            
            // EnemyReconDroneAIController 타입인지 확인
            if (controller.TryGetAIController<BaseDroneAIController>(out var droneController))
            {
                if (droneController.TimerCheck() >= alertable.AlertDuration)
                {
                    return INode.State.SUCCESS;
                }
                else
                {
                    return INode.State.FAILED;
                }
            }
            
            return INode.State.FAILED;
        }
    }
}