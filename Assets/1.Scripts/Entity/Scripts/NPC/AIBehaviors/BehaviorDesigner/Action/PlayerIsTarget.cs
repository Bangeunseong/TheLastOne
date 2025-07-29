using _1.Scripts.Entity.Scripts.NPC.AIBehaviors.BehaviorDesigner.SharedVariables;
using _1.Scripts.Manager.Core;
using UnityEngine;
using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;

namespace _1.Scripts.Entity.Scripts.NPC.AIBehaviors.BehaviorDesigner.Action
{
	[TaskCategory("Every")]
	[TaskDescription("PlayerIsTarget")]
	public class PlayerIsTarget : global::BehaviorDesigner.Runtime.Tasks.Action
	{
		public SharedTransform targetTransform;
		public SharedVector3 targetPosition;
		public SharedCollider playerCollider;
		public SharedBool shouldLookTarget;
		public SharedBaseNpcStatController statController;
		
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
			if (statController.Value.RuntimeStatData.IsAlly) return TaskStatus.Success;
			
			targetTransform.Value = CoreManager.Instance.gameManager.Player.transform;
			targetPosition.Value = playerCollider.Value.bounds.center;
			shouldLookTarget.Value = true;
			
			return TaskStatus.Success;
		}
	}
}