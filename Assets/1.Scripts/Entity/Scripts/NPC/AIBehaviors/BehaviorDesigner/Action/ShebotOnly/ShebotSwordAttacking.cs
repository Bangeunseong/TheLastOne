using _1.Scripts.Entity.Scripts.NPC.AIBehaviors.BehaviorDesigner.SharedVariables;
using _1.Scripts.Entity.Scripts.NPC.Data.AnimationHashData;
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
		
		public override void OnStart()
		{
			animator.Value.SetTrigger(ShebotAnimationHashData.Shebot_Sword_Attack3);
		}

		public override TaskStatus OnUpdate()
		{
			if (isInterrupted.Value)
			{
				isInterrupted.Value = false;
				return TaskStatus.Success;
			}
			
			return TaskStatus.Running;
		}
	}
}
