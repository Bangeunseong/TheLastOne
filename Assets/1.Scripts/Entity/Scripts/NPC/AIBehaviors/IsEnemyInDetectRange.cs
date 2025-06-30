using System.Collections;
using System.Collections.Generic;
using _1.Scripts.Entity.Scripts.Common;
using _1.Scripts.Entity.Scripts.NPC.AIControllers;
using _1.Scripts.Entity.Scripts.NPC.BehaviorTree;
using _1.Scripts.Entity.Scripts.NPC.Data;
using _1.Scripts.Interfaces;
using _1.Scripts.Interfaces.NPC;
using _1.Scripts.Manager.Core;
using _1.Scripts.Static;
using UnityEngine;

namespace _1.Scripts.Entity.Scripts.NPC.AIBehaviors
{
    /// <summary>
    /// 플레이어가 DetectRange 내에 있는지 검사하는 노드, 반드시 컨트롤러의 스탯에 IDetectable이 있어야함
    /// </summary>
    public class IsEnemyInDetectRange : INode 
    {
        public INode.State Evaluate(BaseNpcAI controller)
        {
            if (!controller.statController.TryGetRuntimeStatInterface<IDetectable>(out var detectable))
            {
                Debug.LogWarning($"[IsPlayerInAttackRange] {controller.name}이 IAttackable을 구현하지 않음");
                return INode.State.FAILED;
            }
            
            bool isAlly = controller.statController.RuntimeStatData.isAlly;
            Vector3 selfPos = controller.transform.position;
            float range = detectable.DetectRange;
            
            int layerMask = isAlly ? 1 << LayerConstants.Enemy :  1 << LayerConstants.Ally;
            Collider[] colliders = Physics.OverlapSphere(selfPos, range, layerMask);
            foreach (Collider collider in colliders)
            {
                Vector3 colliderPos = collider.bounds.center;
                if (Service.IsTargetVisible(controller.MyPos, colliderPos, 100f, isAlly))
                {
                    Debug.Log("Warning : Enemy in DetectRange");
                    controller.targetTransform = collider.transform;
                    controller.targetPos = colliderPos;
                    return INode.State.SUCCESS;
                }
            }
            
            return INode.State.FAILED;
        }
    }   
}