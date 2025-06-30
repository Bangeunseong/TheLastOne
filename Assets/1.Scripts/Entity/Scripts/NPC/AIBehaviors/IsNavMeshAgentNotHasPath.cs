using System.Collections;
using System.Collections.Generic;
using _1.Scripts.Entity.Scripts.NPC.AIControllers;
using _1.Scripts.Entity.Scripts.NPC.AIControllers.Base;
using _1.Scripts.Entity.Scripts.NPC.BehaviorTree;
using _1.Scripts.Entity.Scripts.NPC.Data.AnimationHashData;
using _1.Scripts.Interfaces;
using _1.Scripts.Manager.Core;
using UnityEngine;


namespace _1.Scripts.Entity.Scripts.NPC.AIBehaviors
{
    /// <summary>
    /// 지정한 경로가 없어야 SUCCESS
    /// </summary>
    public class IsNavMeshAgentNotHasPath : INode
    {
        public INode.State Evaluate(BaseNpcAI controller)
        {
            if (controller.TryGetAIController<BaseDroneAIController>(out var droneController))
            {
                AnimatorStateInfo stateInfo = droneController.animator.GetCurrentAnimatorStateInfo(0);
                if (!stateInfo.IsName("DroneBot_Hit1") &&
                    !stateInfo.IsName("DroneBot_Hit2") &&
                    !stateInfo.IsName("DroneBot_Hit3") &&
                    !stateInfo.IsName("DroneBot_Hit4") &&
                    !stateInfo.IsName("DroneBot_Idle1"))
                {
                    droneController.animator.SetTrigger(DroneAnimationHashData.Idle1);
                }
                droneController.alertLight.enabled = false;
            }

            controller.shouldLookTarget = false; // 보통 노드구조 맨 끝자락에 배회할때 쓰니까 추가
            
            return controller.navMeshAgent.hasPath ? INode.State.FAILED : INode.State.SUCCESS;
        }
    }
}