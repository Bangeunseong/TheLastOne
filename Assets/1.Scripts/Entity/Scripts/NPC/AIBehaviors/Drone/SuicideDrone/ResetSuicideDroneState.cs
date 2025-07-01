using System.Collections;
using System.Collections.Generic;
using _1.Scripts.Entity.Scripts.NPC.AIControllers.Base;
using _1.Scripts.Entity.Scripts.NPC.BehaviorTree;
using _1.Scripts.Entity.Scripts.NPC.Data.AnimationHashData;
using UnityEngine;

namespace _1.Scripts.Entity.Scripts.NPC.AIBehaviors.Drone.SuicideDrone
{
    public class ResetSuicideDroneState : INode
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
                    !stateInfo.IsName("DroneBot_Repair"))
                {
                    droneController.animator.SetTrigger(DroneAnimationHashData.Repair);
                }

                droneController.alertLight.enabled = false;
                droneController.navMeshAgent.SetDestination(droneController.transform.position);
                return INode.State.RUN;
            }
            
            return INode.State.FAILED;
        }
    }
}
