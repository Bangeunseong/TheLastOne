using _1.Scripts.Entity.Scripts.NPC.AIBehaviors.BehaviorDesigner.SharedVariables;
using _1.Scripts.Entity.Scripts.NPC.AIControllers.Base;
using _1.Scripts.Interfaces.NPC;
using _1.Scripts.Manager.Core;
using _1.Scripts.Manager.Subs;
using _1.Scripts.Static;
using UnityEngine;
using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;

namespace _1.Scripts.Entity.Scripts.NPC.AIBehaviors.BehaviorDesigner.Action
{	
	[TaskCategory("DroneOnly")]
	[TaskDescription("AlertNearBy")]
	public class AlertNearBy : global::BehaviorDesigner.Runtime.Tasks.Action
	{
		public SharedTransform selfTransform;
		public SharedTransform targetTransform;
		public SharedVector3 targetPos;
		
		public SharedCollider myCollider;
		public SharedBaseNpcStatController statController;
		public SharedLight enemylight;
		public SharedLight allyLight;
		public SharedCollider collider;
		public SharedBool isAlerted;
		
		public override TaskStatus OnUpdate()
		{            
			Debug.Log("알람 ON");
			
			if (!statController.Value.TryGetRuntimeStatInterface<IAlertable>(out var alertable)) // 있을 시 변환
			{
				return TaskStatus.Failure;
			}
			
			(statController.Value.RuntimeStatData.isAlly ? allyLight.Value : enemylight.Value).enabled = true; // 경고등 On
			CoreManager.Instance.soundManager.PlaySFX(SfxType.Drone, collider.Value.bounds.center, index:1); // 사운드 출력
			isAlerted.Value = true;

			bool isAlly = statController.Value.RuntimeStatData.isAlly; 
			Vector3 selfPos = selfTransform.Value.position;
			float range = alertable.AlertRadius;

			int layerMask = isAlly ? 1 << LayerConstants.Ally : 1 << LayerConstants.Enemy;
			Collider[] colliders = Physics.OverlapSphere(selfPos, range, layerMask); // 주변 콜라이더 모음

			foreach (var col in colliders)
			{
				if (col == myCollider.Value)
				{
					continue;
				}
				
				var BT = col.GetComponent<global::BehaviorDesigner.Runtime.BehaviorTree>();
				if (BT != null)
				{
					BT.SetVariableValue("target_Transform", targetTransform.Value);
					BT.SetVariableValue("target_Pos", targetPos);
					BT.SetVariableValue("IsAlerted", true);
				}
			}
			
			return TaskStatus.Success;
		}
	}
}