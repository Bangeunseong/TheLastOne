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
		public SharedAnimator animator;
		public SharedBool shouldLookTarget;
		public SharedNavMeshAgent agent;
		public SharedLight light;
		
		public override TaskStatus OnUpdate()
		{
			AnimatorStateInfo stateInfo = animator.Value.GetCurrentAnimatorStateInfo(0);
			if (!stateInfo.IsName("DroneBot_Hit1") &&
			    !stateInfo.IsName("DroneBot_Hit2") &&
			    !stateInfo.IsName("DroneBot_Hit3") &&
			    !stateInfo.IsName("DroneBot_Hit4") &&
			    !stateInfo.IsName("DroneBot_Idle1"))
			{
				animator.Value.SetTrigger(DroneAnimationHashData.Idle1);
			}
			light.Value.enabled = false;
			
			shouldLookTarget.Value = false; // 보통 노드구조 맨 끝자락에 배회할때 쓰니까 추가

			if (agent.Value.remainingDistance < 0.1f || !agent.Value.hasPath)
			{
				return TaskStatus.Success;
			}

			return TaskStatus.Failure;
		}
	}
}