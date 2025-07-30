using _1.Scripts.Entity.Scripts.NPC.AIBehaviors.BehaviorDesigner.SharedVariables;
using _1.Scripts.Entity.Scripts.NPC.Data.AnimationHashData;
using _1.Scripts.Util;
using UnityEngine;
using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;

namespace _1.Scripts.Entity.Scripts.NPC.AIBehaviors.BehaviorDesigner.Action.ShebotOnly
{
	[TaskCategory("ShebotOnly")]
	[TaskDescription("ShebotSwordAttacking")]
	public class ShebotSwordAttacking : global::BehaviorDesigner.Runtime.Tasks.Action
	{
		public SharedAnimator animator;
		public SharedBool isInterrupted;

		public SharedTransform selfTransform;
		public SharedTransform targetTransform;
		public SharedBaseNpcStatController statController;
		
		public override void OnStart()
		{
			if (statController.Value.RuntimeStatData.IsAlly)
			{
				animator.Value.SetTrigger(ShebotAnimationData.Shebot_Sword_Attack_Full);
			}
			else
			{
				animator.Value.SetTrigger(ShebotAnimationData.Shebot_Sword_Attack3);
			}
		}

		public override TaskStatus OnUpdate()
		{
			if (isInterrupted.Value)
			{
				isInterrupted.Value = false;
				return TaskStatus.Success;
			}
			
			// 아군일때만 피할수없게 공격
			if (statController.Value.RuntimeStatData.IsAlly) NpcUtil.LookAtTarget(selfTransform.Value, targetTransform.Value.position, 50f);
			return TaskStatus.Running;
		}
	}
}
