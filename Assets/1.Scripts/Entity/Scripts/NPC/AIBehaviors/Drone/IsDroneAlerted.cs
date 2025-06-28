using System.Collections;
using System.Collections.Generic;
using _1.Scripts.Entity.Scripts.NPC.AIControllers;
using _1.Scripts.Entity.Scripts.NPC.AIControllers.Base;
using _1.Scripts.Entity.Scripts.NPC.AIControllers.Enemy;
using _1.Scripts.Entity.Scripts.NPC.BehaviorTree;
using _1.Scripts.Entity.Scripts.NPC.Data.LayerConstants;
using _1.Scripts.Entity.Scripts.Npc.StatControllers;
using _1.Scripts.Interfaces.NPC;
using UnityEngine;

namespace _1.Scripts.Entity.Scripts.NPC.AIBehaviors.Drone
{
    /// <summary>
    /// 알람 켜졌는지 알려주는 노드, 만약 켜져있는데 타겟이 비었다면 알람 비활성화
    /// </summary>
    public class IsDroneAlerted : INode
    {
        public INode.State Evaluate(BaseNpcAI controller)
        {
            if (controller.TryGetAIController<BaseDroneAIController>(out var droneController))
            {
                if (!droneController.IsAlertedCheck())
                {
                    return INode.State.FAILED;
                }

                if (!controller.statController.TryGetRuntimeStatInterface<IDetectable>(out var detectable))
                {
                    return INode.State.FAILED;
                }
                
                bool isAlly = controller.statController.RuntimeStatData.isAlly;
                Vector3 selfPos = controller.transform.position;
                float range = detectable.DetectRange;
            
                int layerMask = isAlly ? 1 << LayerConstants.Enemy :  1 << LayerConstants.Ally;
                Collider[] colliders = Physics.OverlapSphere(selfPos, range, layerMask);
                foreach (Collider collider in colliders)
                {
                    if (!collider.CompareTag("Player"))
                    {
                        var statController = collider.GetComponent<BaseNpcStatController>();
                        if (statController == null || statController.isDead)
                        {
                            continue;
                        }
                    }
                
                    Vector3 colliderPos = collider.bounds.center;
                    if (Service.IsTargetVisible(controller.MyPos, colliderPos, 100f, isAlly))
                    {
                        controller.targetTransform = collider.transform;
                        controller.targetPos = colliderPos;
                        return INode.State.SUCCESS;
                    }
                }
                
                if (droneController.targetTransform == null)
                {
                    droneController.ResetAll();
                    return INode.State.FAILED;
                }
                
                if (!droneController.targetTransform.CompareTag("Player"))
                {
                    var statController = droneController.targetTransform.GetComponent<BaseNpcStatController>();
                    if (statController == null || statController.isDead)
                    {
                        droneController.ResetAll();
                        return INode.State.FAILED;
                    }
                }
                
                return INode.State.SUCCESS;
            }
            
            return INode.State.FAILED;
        }
    }
}
