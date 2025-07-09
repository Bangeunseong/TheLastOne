using System.Collections;
using System.Collections.Generic;
using _1.Scripts.Entity.Scripts.NPC.AIControllers.Base;
using _1.Scripts.Entity.Scripts.NPC.BehaviorTree;
using _1.Scripts.Entity.Scripts.NPC.Data.AnimationHashData;
using _1.Scripts.Entity.Scripts.Npc.StatControllers.Base;
using _1.Scripts.Interfaces;
using _1.Scripts.Manager.Core;
using UnityEngine;

namespace _1.Scripts.Entity.Scripts.NPC.AIBehaviors.Drone.ReconDrone
{
    public class ReconDroneAttacking : INode
    {
        public INode.State Evaluate(BaseNpcAI controller)
        {
            if (controller.targetTransform == null || controller.targetPos == Vector3.zero)
            {
                return INode.State.FAILED;
            }

            if (!controller.targetTransform.CompareTag("Player"))
            {
                var statController = controller.targetTransform.GetComponent<BaseNpcStatController>();
                if (statController == null || statController.IsDead)
                {
                    return INode.State.FAILED;
                }
            }
            
            controller.shouldLookTarget = true;
            
            if (controller.animator.GetCurrentAnimatorStateInfo(0).IsName("DroneBot_Fire"))
            {
                return INode.State.RUN;
            }
            
            controller.animator.SetTrigger(DroneAnimationHashData.Fire);
            
            // 레콘드론 공격
            return INode.State.RUN;
        }
    }
}
