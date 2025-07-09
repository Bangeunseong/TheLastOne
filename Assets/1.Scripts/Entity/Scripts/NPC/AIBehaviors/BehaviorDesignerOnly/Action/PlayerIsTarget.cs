using _1.Scripts.Manager.Core;
using UnityEngine;
using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;

namespace _1.Scripts.Entity.Scripts.NPC.AIBehaviors.BehaviorDesignerOnly.Action
{
	[TaskCategory("Every")]
	[TaskDescription("PlayerIsTarget")]
	public class PlayerIsTarget : global::BehaviorDesigner.Runtime.Tasks.Action
	{
		public SharedTransform targetTransform;
		public SharedVector3 targetPosition;
		public SharedCollider playerCollider;
		public SharedBool shouldLookTarget;

		public override void OnStart()
		{
			if (playerCollider.Value == null)
			{
				playerCollider.Value =
					Service.TryGetChildComponent<Collider>(CoreManager.Instance.gameManager.Player, "spine_03");
			}
		}

		public override TaskStatus OnUpdate()
		{
			targetTransform.Value = CoreManager.Instance.gameManager.Player.transform;
			targetPosition.Value = playerCollider.Value.bounds.center;
			shouldLookTarget.Value = true;
			
			return TaskStatus.Success;
		}
	}
}