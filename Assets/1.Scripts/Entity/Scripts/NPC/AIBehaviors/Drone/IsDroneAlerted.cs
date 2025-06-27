using System.Collections;
using System.Collections.Generic;
using _1.Scripts.Entity.Scripts.NPC.AIControllers;
using _1.Scripts.Entity.Scripts.NPC.AIControllers.Enemy;
using _1.Scripts.Entity.Scripts.NPC.BehaviorTree;
using UnityEngine;

namespace _1.Scripts.Entity.Scripts.NPC.AIBehaviors.Drone
{
    /// <summary>
    /// 알람 켜졌는지 알려주는 노드
    /// </summary>
    public class IsDroneAlerted : INode
    {
        public INode.State Evaluate(BaseNpcAI controller)
        {
            if (controller.TryGetAIController<EnemyReconDroneAIController>(out var reconDrone))
            {
                return reconDrone.IsAlertedCheck() ? INode.State.SUCCESS : INode.State.FAILED;
            }

            if (controller.TryGetAIController<EnemySuicideDroneAIController>(out var suicideDrone))
            {
                return suicideDrone.IsAlertedCheck() ? INode.State.SUCCESS : INode.State.FAILED;
            }

            return INode.State.FAILED;
        }
    }
}
