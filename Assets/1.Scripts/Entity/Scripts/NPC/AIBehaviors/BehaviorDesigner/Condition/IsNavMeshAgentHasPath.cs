using _1.Scripts.Entity.Scripts.NPC.AIBehaviors.BehaviorDesigner.SharedVariables;
using _1.Scripts.Entity.Scripts.NPC.BehaviorTree;
using _1.Scripts.Entity.Scripts.NPC.Data.AnimationHashData;
using UnityEngine;
using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;

namespace _1.Scripts.Entity.Scripts.NPC.AIBehaviors.BehaviorDesigner.Condition
{
	[TaskCategory("Every")]
	[TaskDescription("IsNavMeshAgentHasPath")]
	public class IsNavMeshAgentHasPath : Conditional
	{
		public SharedNavMeshAgent agent;
		
		public override TaskStatus OnUpdate()
		{
			if (agent.Value.remainingDistance < 0.1f || !agent.Value.hasPath)
			{
				return TaskStatus.Success;
			}

			return TaskStatus.Failure;
		}
	}
}