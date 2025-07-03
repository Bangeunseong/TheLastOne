using _1.Scripts.Entity.Scripts.NPC.AIBehaviors.BehaviorDesigner.SharedVariables;
using _1.Scripts.Entity.Scripts.Npc.StatControllers.Base;
using _1.Scripts.Interfaces.NPC;
using _1.Scripts.Static;
using UnityEngine;
using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;

namespace _1.Scripts.Entity.Scripts.NPC.AIBehaviors.BehaviorDesigner.Condition
{
	[TaskCategory("Every")]
	[TaskDescription("IsTargetInDetectRange")]
	public class IsTargetInDetectRange : Conditional
	{
		public SharedTransform selfTransform;
		public SharedTransform targetTransform;
		public SharedVector3 targetPos;
		public SharedFloat maxViewDistance;
		public SharedBool shouldLookTarget;
		public SharedBaseNpcStatController statController;

		public override TaskStatus OnUpdate()
		{
			bool ally = statController.Value.RuntimeStatData.isAlly;
			Vector3 selfPos = selfTransform.Value.position;
			float range = 0f;
			if (statController.Value.TryGetRuntimeStatInterface<IDetectable>(out var detectable))
			{
				range = detectable.DetectRange;
			}
			else
			{
				// 공격 사거리 정보가 없으면 실패 처리
				return TaskStatus.Failure;
			}

			int layerMask = ally ? (1 << LayerConstants.Enemy) : (1 << LayerConstants.Ally);

			Collider[] colliders = Physics.OverlapSphere(selfPos, range, layerMask);

			foreach (var collider in colliders)
			{
				if (!collider.CompareTag("Player"))
				{
					var otherStatController = collider.GetComponent<BaseNpcStatController>();
					if (otherStatController == null || otherStatController.isDead)
					{
						continue;
					}
				}

				Vector3 colliderPos = collider.bounds.center;

				if (Service.IsTargetVisible(selfPos, colliderPos, maxViewDistance.Value, ally))
				{
					shouldLookTarget.Value = true;
					return TaskStatus.Success;
				}
			}
			
			return TaskStatus.Failure;
		}	
	}
}