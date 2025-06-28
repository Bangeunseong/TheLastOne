using System.Collections;
using System.Collections.Generic;
using _1.Scripts.Entity.Scripts.NPC.AIControllers;
using _1.Scripts.Entity.Scripts.NPC.AIControllers.Enemy;
using _1.Scripts.Entity.Scripts.NPC.BehaviorTree;
using _1.Scripts.Entity.Scripts.NPC.Data.LayerConstants;
using _1.Scripts.Interfaces;
using _1.Scripts.Interfaces.NPC;
using UnityEngine;

namespace _1.Scripts.Entity.Scripts.NPC.AIBehaviors.Drone
{
    /// <summary>
    /// 알람 타이머 시작 노드
    /// </summary>
    public class AlertNearbyDrones : INode
    {
        public INode.State Evaluate(BaseNpcAI controller)
        {
            if (!controller.statController.TryGetRuntimeStatInterface<IAlertable>(out var alertable)) // 있을 시 변환
            {
                Debug.LogWarning($"[AlertNearbyDrones] statData가 IAlertable 구현하지 않음. 컨트롤러: {controller.name}");
                return INode.State.FAILED;
            }
            
            // EnemyReconDroneAIController 타입인지 확인
            if (controller.TryGetAIController<EnemyReconDroneAIController>(out var reconDrone))
            {
                reconDrone.SetAlert(true);
            }

            // EnemySuicideDroneAIController 타입인지 확인
            if (controller.TryGetAIController<EnemySuicideDroneAIController>(out var suicideDrone))
            {
                suicideDrone.SetAlert(true);
            }

            bool isAlly = controller.statController.RuntimeStatData.isAlly;
            Vector3 selfPos = controller.transform.position;
            float range = alertable.AlertRadius;
            
            int layerMask = isAlly ? 1 << LayerConstants.Enemy :  1 << LayerConstants.Ally;
            Collider[] colliders = Physics.OverlapSphere(selfPos, range, layerMask);

            foreach (var collider in colliders)
            {
                if (collider.TryGetComponent(out BaseNpcAI npc))
                {
                    // 자신 제외
                    if (npc == controller) continue;

                    // 경고 가능 드론이면 상태 전달
                    if (npc is EnemyReconDroneAIController reconDrones)
                    {
                        reconDrones.SetAlert(true);
                    }
                    else if (npc is EnemySuicideDroneAIController suicideDrones)
                    {
                        suicideDrones.SetAlert(true);
                    }
                }
            }

            return INode.State.SUCCESS;
        }
    }
}