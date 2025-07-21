using _1.Scripts.Entity.Scripts.NPC.AIBehaviors.BehaviorDesigner.SharedVariables;
using _1.Scripts.Entity.Scripts.Npc.StatControllers.Base;
using _1.Scripts.Interfaces.NPC;
using _1.Scripts.Static;
using _1.Scripts.Util;
using UnityEngine;
using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;

namespace _1.Scripts.Entity.Scripts.NPC.AIBehaviors.BehaviorDesigner.Condition
{
	[TaskCategory("Every")]
	[TaskDescription("IsAlerted")]
	public class IsAlerted : Conditional
	{
		public SharedBool isAlerted;

		public SharedTransform selfTransform;
		public SharedCollider selfCollider; 
		public SharedTransform targetTransform;
		public SharedVector3 targetPos;
		
		public SharedBaseNpcStatController statController;
		public SharedBool isAlertedOnce;
		
		public override TaskStatus OnUpdate()
		{
			if (!isAlerted.Value)
			{
				return TaskStatus.Failure;
			}

			if (!statController.Value.TryGetRuntimeStatInterface<IDetectable>(out var detectable))
			{
				return TaskStatus.Failure;
			}
			
			bool isAlly = statController.Value.RuntimeStatData.IsAlly;
			Vector3 selfPos = selfTransform.Value.position;
			float range = detectable.DetectRange;
            
			int layerMask = isAlly ? 1 << LayerConstants.Enemy :  1 << LayerConstants.Ally;
			
			Collider[] colliders = Physics.OverlapSphere(selfPos, range, layerMask);
			foreach (var col in colliders)
			{
				if (!col.CompareTag("Player"))
				{
					var statCtrl = col.GetComponent<BaseNpcStatController>();
					if (statCtrl == null || statCtrl.IsDead || statCtrl.isHacking)
					{
						continue;
					}
				}

				Vector3 center = col.bounds.center;
				if (NpcUtil.IsTargetVisible(selfCollider.Value.bounds.center, center, 100f, isAlly))
				{
					if (isAlertedOnce != null) isAlertedOnce.Value = true;
					
					targetTransform.Value = col.transform;
					targetPos.Value = center;
					return TaskStatus.Success;
				}
			}

			if (targetTransform.Value == null) // 예외처리 1 : 타겟이 사라졌을 시
			{
				targetPos.Value = Vector3.zero;
				targetTransform.Value = null;
				isAlerted.Value = false;

				return TaskStatus.Failure;
			}
			
			if (!targetTransform.Value.CompareTag("Player")) // 예외처리 2 : 타겟이 사망했을 시 & 해킹당하는 중일때 & 지정했던 타겟이 아군이 됐을 때
			{
				var stat = targetTransform.Value.GetComponent<BaseNpcStatController>();
				if (stat == null || stat.IsDead || stat.isHacking || stat.RuntimeStatData.IsAlly)
				{
					targetPos.Value = Vector3.zero;
					targetTransform.Value = null;
					isAlerted.Value = false;
					
					return TaskStatus.Failure;
				}
			}
			
			if (isAlertedOnce != null) isAlertedOnce.Value = true;
			return TaskStatus.Success;
		}
	}
}