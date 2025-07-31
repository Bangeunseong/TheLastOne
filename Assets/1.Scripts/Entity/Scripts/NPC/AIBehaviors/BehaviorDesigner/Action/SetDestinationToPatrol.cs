using _1.Scripts.Entity.Scripts.NPC.AIBehaviors.BehaviorDesigner.SharedVariables;
using _1.Scripts.Entity.Scripts.NPC.Data.AnimationHashData;
using _1.Scripts.Interfaces.NPC;
using _1.Scripts.Manager.Core;
using UnityEngine;
using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using UnityEngine.AI;

namespace _1.Scripts.Entity.Scripts.NPC.AIBehaviors.BehaviorDesigner.Action
{
	[TaskCategory("Every")]
	[TaskDescription("SetDestinationToPatrol")]
	public class SetDestinationToPatrol : global::BehaviorDesigner.Runtime.Tasks.Action
	{
		public SharedTransform selfTransform;
		public SharedBaseNpcStatController statController;
		public SharedNavMeshAgent agent;
		public SharedAnimator animator;
		public bool isShebot;
		public bool isShebot_Rifle;
		
		public SharedLight enemyLight;
		public SharedLight allyLight;
		public SharedFloat stoppingDistance;

		public override void OnStart()
		{
			agent.Value.speed = statController.Value.RuntimeStatData.IsAlly
				? statController.Value.RuntimeStatData.MoveSpeed + statController.Value.RuntimeStatData.RunMultiplier
				: statController.Value.RuntimeStatData.MoveSpeed;
			agent.Value.isStopped = false;
			enemyLight.Value.enabled = false;
			allyLight.Value.enabled = false;
		}

		public override TaskStatus OnUpdate()
		{
			Vector3 targetPosition;
			if (statController.Value.RuntimeStatData.IsAlly)
			{
				targetPosition = GetPlayerPosition();
			}
			else
			{ 
				targetPosition = GetWanderLocation();
			}
			
			agent.Value.SetDestination(targetPosition);
			if (isShebot)
			{
				if (statController.Value.RuntimeStatData.IsAlly)
				{
					AnimatorStateInfo stateInfo = animator.Value.GetCurrentAnimatorStateInfo(0);
					
					if (Vector3.Distance(selfTransform.Value.position, targetPosition) <= 0.05)
					{
						if (isShebot_Rifle && !stateInfo.IsName(ShebotAnimationData.Shebot_Rifle_idleStr))
						{
							animator.Value.SetTrigger(ShebotAnimationData.Shebot_Rifle_idle);
						}
						else if (!stateInfo.IsName(ShebotAnimationData.Shebot_IdleStr))
						{
							animator.Value.SetTrigger(ShebotAnimationData.Shebot_Idle);
						}
					}
					else
					{
						if (!stateInfo.IsName(ShebotAnimationData.Shebot_WalkStr)) animator.Value.SetTrigger(ShebotAnimationData.Shebot_Walk);
					}
				}
				else
				{
					animator.Value.SetTrigger(ShebotAnimationData.Shebot_Walk);
				}
			}
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

		Vector3 GetPlayerPosition()
		{
			agent.Value.speed = statController.Value.RuntimeStatData.MoveSpeed + statController.Value.RuntimeStatData.RunMultiplier;
				
			Vector3 directionToPlayer = (CoreManager.Instance.gameManager.Player.transform.position - selfTransform.Value.position).normalized;
			Vector3 targetSpot = CoreManager.Instance.gameManager.Player.transform.position - directionToPlayer * (stoppingDistance.Value + 1.5f);
				
			if (NavMesh.SamplePosition(targetSpot, out NavMeshHit hit, 1.0f, NavMesh.AllAreas))
			{
				return hit.position;
			}

			return selfTransform.Value.position;
		}
	}
}