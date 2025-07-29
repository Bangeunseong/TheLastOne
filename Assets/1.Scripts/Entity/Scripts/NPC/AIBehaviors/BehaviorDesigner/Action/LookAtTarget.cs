using _1.Scripts.Util;
using UnityEngine;
using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;

namespace _1.Scripts.Entity.Scripts.NPC.AIBehaviors.BehaviorDesigner.Action
{
	[TaskCategory("Every")]
	[TaskDescription("LookAtTarget")]
	public class LookAtTarget : global::BehaviorDesigner.Runtime.Tasks.Action
	{
		public SharedTransform selfTransform;
		public SharedTransform targetTransform;
		public SharedBool shouldLookTarget;
		
		public override TaskStatus OnUpdate()
		{
			if (shouldLookTarget.Value)
			{
				Service.Log("쳐다보는중");
				NpcUtil.LookAtTarget(selfTransform.Value, targetTransform.Value);
			}
			return TaskStatus.Success;
		}
	}
}