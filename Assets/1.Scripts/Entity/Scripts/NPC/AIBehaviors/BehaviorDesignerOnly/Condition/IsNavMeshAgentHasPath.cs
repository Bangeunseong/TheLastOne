using _1.Scripts.Entity.Scripts.NPC.AIBehaviors.BehaviorDesignerOnly.SharedVariables;
using _1.Scripts.Entity.Scripts.NPC.BehaviorTree;
using _1.Scripts.Entity.Scripts.NPC.Data.AnimationHashData;
using UnityEngine;
using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;

namespace _1.Scripts.Entity.Scripts.NPC.AIBehaviors.BehaviorDesignerOnly.Condition
{
	[TaskCategory("Every")]
	[TaskDescription("IsNavMeshAgentHasPath")]
	public class IsNavMeshAgentHasPath : Conditional
	{
		public SharedNavMeshAgent agent;
		public SharedVector3 targetPosition;
		public SharedBool shouldAlertNearBy;
		
		public override TaskStatus OnUpdate()
		{
			if (!agent.Value.hasPath && targetPosition.Value == Vector3.zero)
			{
				return TaskStatus.Success;
			}

			return TaskStatus.Failure;
		}
	}
}