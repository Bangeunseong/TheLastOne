using _1.Scripts.Entity.Scripts.NPC.AIBehaviors.BehaviorDesigner.SharedVariables;
using _1.Scripts.Entity.Scripts.Npc.StatControllers.Base;
using UnityEngine;
using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using UnityEngine.AI;

namespace _1.Scripts.Entity.Scripts.NPC.AIBehaviors.BehaviorDesigner.Action
{
	[TaskCategory("Every")]
	[TaskDescription("SetDestinationToEnemy")]
	public class SetDestinationToEnemy : global::BehaviorDesigner.Runtime.Tasks.Action
	{
		public SharedTransform targetTransform;
		public SharedVector3 targetPos;
		public SharedNavMeshAgent navMeshAgent;
		public SharedBaseNpcStatController statController;
		public SharedBool shouldLookTarget;

		public override TaskStatus OnUpdate()
		{
			if (targetTransform.Value == null || targetPos.Value == Vector3.zero)
				return TaskStatus.Failure;

			if (!targetTransform.Value.CompareTag("Player"))
			{
				var statCtrl = targetTransform.Value.GetComponent<BaseNpcStatController>();
				if (statCtrl == null || statCtrl.isDead)
				{
					return TaskStatus.Failure;
				}
			}

			navMeshAgent.Value.speed = statController.Value.RuntimeStatData.moveSpeed + statController.Value.RuntimeStatData.runMultiplier;
			shouldLookTarget.Value = true;

			if (NavMesh.SamplePosition(targetTransform.Value.position, out NavMeshHit hit, 1.0f, NavMesh.AllAreas))
			{
				navMeshAgent.Value.SetDestination(hit.position);
			}

			return TaskStatus.Success;
		}
	}
}