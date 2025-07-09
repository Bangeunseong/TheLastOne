using _1.Scripts.Entity.Scripts.NPC.AIBehaviors.BehaviorDesignerOnly.SharedVariables;
using _1.Scripts.Entity.Scripts.NPC.Data.AnimationHashData;
using UnityEngine;
using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;

namespace _1.Scripts.Entity.Scripts.NPC.AIBehaviors.BehaviorDesignerOnly.Action.DroneOnly
{
	[TaskCategory("DroneOnly")]
	[TaskDescription("DroneEnterPatrolMode")]
	public class DroneEnterPatrolMode : global::BehaviorDesigner.Runtime.Tasks.Action
	{
		public SharedBool shouldLookTarget;
		public SharedAnimator animator;
		
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
			
			shouldLookTarget.Value = false; // 보통 노드구조 맨 끝자락에 배회할때 쓰니까 추가
			
			return TaskStatus.Success;
		}
	}
}