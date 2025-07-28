using _1.Scripts.Entity.Scripts.NPC.AIBehaviors.BehaviorDesigner.SharedVariables;
using _1.Scripts.Entity.Scripts.NPC.Data.AnimationHashData;
using _1.Scripts.Util;
using UnityEngine;
using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;


namespace _1.Scripts.Entity.Scripts.NPC.AIBehaviors.BehaviorDesigner.Action.ShebotOnly
{
	[TaskCategory("ShebotOnly")]
	[TaskDescription("ShebotSwordOnShield")]
	public class ShebotSwordOnShield : global::BehaviorDesigner.Runtime.Tasks.Action
	{
		public SharedTransform selfTransform;
		public SharedTransform targetTransform;
		public SharedAnimator animator;
		public SharedBool isInterrupted;
		public SharedBool shieldUsedOnce;
		public SharedBool hasEnteredShield; // 쉴드를 발동한 단 한번만 Update가 실행하도록 하는 플래그
		
		public override void OnStart()
		{
			if (!shieldUsedOnce.Value)
			{
				animator.Value.SetTrigger(ShebotAnimationHashData.Shebot_Guard);
				shieldUsedOnce.Value = true;
				hasEnteredShield.Value = true;
			}
			else
			{
				hasEnteredShield.Value = false;
			}
		}

		public override TaskStatus OnUpdate()
		{
			if (!hasEnteredShield.Value) return TaskStatus.Success;
			
			if (isInterrupted.Value)
			{
				isInterrupted.Value = false;
				return TaskStatus.Success;
			}

			if (targetTransform.Value != null)
			{
				NpcUtil.LookAtTarget(selfTransform.Value, targetTransform.Value);
			}
			
			return TaskStatus.Running;
		}
	}
}