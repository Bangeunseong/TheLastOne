using _1.Scripts.Entity.Scripts.NPC.AIBehaviors.BehaviorDesigner.SharedVariables;
using _1.Scripts.Entity.Scripts.Npc.StatControllers.Base;
using _1.Scripts.Interfaces.NPC;
using _1.Scripts.Static;
using UnityEngine;
using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;


namespace _1.Scripts.Entity.Scripts.NPC.AIBehaviors.BehaviorDesigner.Action
{
	[TaskCategory("Every")]
	[TaskDescription("InterruptibleWait")]
	public class InterruptibleWait : global::BehaviorDesigner.Runtime.Tasks.Action
	{
		private float startTime;
		private float waitingTime;
		
		public SharedTransform selfTransform;
		public SharedBool isAlerted;
		public SharedFloat maxViewDistance;
		public SharedBaseNpcStatController statController;
		
		public override void OnStart()
		{
			startTime = Time.time;

			if (statController.Value.TryGetRuntimeStatInterface<IPatrolable>(out var patrolable))
			{
				waitingTime = Random.Range(patrolable.MinWaitingDuration, patrolable.MaxWaitingDuration);
			}
			else
			{
				waitingTime = 0f; // fallback
			}
		}

		public override TaskStatus OnUpdate()
		{
			if (Time.time - startTime >= waitingTime)
			{
				return TaskStatus.Success;
			}

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
					return TaskStatus.Failure;
				}
			}

			if (isAlerted.Value)
			{
				return TaskStatus.Failure;
			}
			
			return TaskStatus.Running;
		}	
	}
}