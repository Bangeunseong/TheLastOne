using System.Collections;
using System.Collections.Generic;
using _1.Scripts.Entity.Scripts.NPC.AIControllers;
using _1.Scripts.Entity.Scripts.NPC.BehaviorTree;
using _1.Scripts.Entity.Scripts.Npc.StatControllers;
using _1.Scripts.Interfaces;
using _1.Scripts.Manager.Core;
using UnityEngine;
using UnityEngine.AI;

namespace _1.Scripts.Entity.Scripts.NPC.AIBehaviors
{
    /// <summary>
    /// 플레이어에게 경로 지정
    /// </summary>
    public class SetDestinationToEnemy : INode
    {
        public INode.State Evaluate(BaseNpcAI controller)
        {
            if (controller.targetTransform == null || controller.targetPos == Vector3.zero)
            {
                return INode.State.FAILED;
            }

            // 타겟의 스탯 컨트롤러 가져오기
            var statController = controller.targetTransform.GetComponent<BaseNpcStatController>();
            if (statController == null || statController.isDead)
            {
                return INode.State.FAILED;
            }
            
            
            controller.navMeshAgent.speed = controller.statController.RuntimeStatData.moveSpeed + controller.statController.RuntimeStatData.runMultiplier;
            controller.shouldLookTarget = true;
            
            // NavMesh 위의 가장 가까운 위치를 찾음
            if (NavMesh.SamplePosition(controller.targetTransform.position, out NavMeshHit hit, 1.0f, NavMesh.AllAreas))
            {
                if (controller.targetTransform != null)
                {
                    controller.navMeshAgent.SetDestination(hit.position);   
                }
            }

            return INode.State.RUN;
        }
    }
}