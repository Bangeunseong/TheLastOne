using System.Collections;
using System.Collections.Generic;
using _1.Scripts.Entity.Scripts.NPC.AIControllers;
using _1.Scripts.Entity.Scripts.NPC.AIControllers.Base;
using _1.Scripts.Entity.Scripts.NPC.BehaviorTree;
using _1.Scripts.Interfaces;
using _1.Scripts.Interfaces.NPC;
using _1.Scripts.Manager.Core;
using UnityEngine;
using UnityEngine.AI;

namespace _1.Scripts.Entity.Scripts.NPC.AIBehaviors
{
    /// <summary>
    /// 배회경로 지정
    /// </summary>
    public class SetDestinationToPatrol : INode
    {
        private BaseNpcAI controller;
        
        public INode.State Evaluate(BaseNpcAI controller)
        {
            this.controller = controller;

            controller.targetPos = Vector3.zero;
            controller.targetTransform = null;
            
            controller.navMeshAgent.speed = controller.statController.RuntimeStatData.moveSpeed;
            controller.navMeshAgent.isStopped = false;

            Vector3 targetPosition = GetWanderLocation();
            
            controller.navMeshAgent.SetDestination(targetPosition);
            
            return INode.State.RUN;
        }
    
        Vector3 GetWanderLocation()
        {
            NavMeshHit hit;
            int i = 0;

            controller.statController.TryGetRuntimeStatInterface<IPatrolable>(out var patrolable);
            controller.statController.TryGetRuntimeStatInterface<IDetectable>(out var detectable);
            
            do
            {
                NavMesh.SamplePosition(controller.targetPos + (Random.onUnitSphere * Random.Range(patrolable.MinWanderingDistance, patrolable.MaxWanderingDistance)), out hit, patrolable.MaxWanderingDistance, NavMesh.AllAreas);
                i++;
                if (i == 30) break;
            } 
            while (Vector3.Distance(controller.transform.position, hit.position) < detectable.DetectRange);
        
            return hit.position;
        }
        
    }
}