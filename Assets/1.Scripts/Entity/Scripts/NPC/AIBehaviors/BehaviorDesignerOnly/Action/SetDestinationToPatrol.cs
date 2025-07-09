using _1.Scripts.Entity.Scripts.NPC.AIBehaviors.BehaviorDesignerOnly.SharedVariables;
using _1.Scripts.Interfaces.NPC;
using UnityEngine;
using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using UnityEngine.AI;

namespace _1.Scripts.Entity.Scripts.NPC.AIBehaviors.BehaviorDesignerOnly.Action
{
	[TaskCategory("Every")]
	[TaskDescription("SetDestinationToPatrol")]
	public class SetDestinationToPatrol : global::BehaviorDesigner.Runtime.Tasks.Action
	{
		public SharedTransform selfTransform;
		public SharedBaseNpcStatController statController;
		public SharedNavMeshAgent agent;
		
		public SharedLight enemyLight;
		public SharedLight allyLight;

		public override TaskStatus OnUpdate()
		{
			agent.Value.speed = statController.Value.RuntimeStatData.MoveSpeed;
			agent.Value.isStopped = false;
			
			Vector3 targetPosition = GetWanderLocation();
			
			agent.Value.SetDestination(targetPosition);
			
			enemyLight.Value.enabled = false;
			allyLight.Value.enabled = false;
			
			return TaskStatus.Success;
		}
		
		Vector3 GetWanderLocation()
		{
			NavMeshHit hit;
			int i = 0;

			statController.Value.TryGetRuntimeStatInterface<IPatrolable>(out var patrolable);
			statController.Value.TryGetRuntimeStatInterface<IDetectable>(out var detectable);
            
			do
			{
				NavMesh.SamplePosition(selfTransform.Value.position + (Random.onUnitSphere * Random.Range(patrolable.MinWanderingDistance, patrolable.MaxWanderingDistance)), out hit, patrolable.MaxWanderingDistance, NavMesh.AllAreas);
				i++;
				if (i == 30) break;
			} 
			while (Vector3.Distance(selfTransform.Value.position, hit.position) < detectable.DetectRange);
        
			return hit.position;
		}
	}
}