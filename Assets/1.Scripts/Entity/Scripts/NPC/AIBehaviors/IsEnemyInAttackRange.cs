using _1.Scripts.Entity.Scripts.NPC.AIControllers;
using _1.Scripts.Entity.Scripts.NPC.AIControllers.Base;
using _1.Scripts.Entity.Scripts.NPC.BehaviorTree;
using _1.Scripts.Entity.Scripts.Npc.StatControllers;
using _1.Scripts.Interfaces.NPC;
using _1.Scripts.Static;
using UnityEngine;


namespace _1.Scripts.Entity.Scripts.NPC.AIBehaviors
{
    /// <summary>
    /// 플레이어가 AttackRange 내에 있는지 검사하는 노드, 반드시 컨트롤러의 스탯에 IAttackable이 있어야함
    /// </summary>
    public class IsEnemyInAttackRange : INode
    {
        public INode.State Evaluate(BaseNpcAI controller)
        {
            if (!controller.statController.TryGetRuntimeStatInterface<IAttackable>(out var attackable))
            {
                return INode.State.FAILED;
            }
            
            bool isAlly = controller.statController.RuntimeStatData.isAlly;
            Vector3 selfPos = controller.transform.position;
            float range = attackable.AttackRange;
            
            int layerMask = isAlly ? 1 << LayerConstants.Enemy :  1 << LayerConstants.Ally;
            Collider[] colliders = Physics.OverlapSphere(selfPos, range, layerMask);
            foreach (Collider collider in colliders)
            {
                if (!collider.CompareTag("Player"))
                {
                    var statController = collider.GetComponent<BaseNpcStatController>();
                    if (statController == null || statController.isDead)
                    {
                        continue;
                    }
                }
                
                Vector3 colliderPos = collider.bounds.center;
                if (Service.IsTargetVisible(controller.MyPos, colliderPos, 100f, isAlly))
                {
                    if (controller.TryGetAIController<BaseDroneAIController>(out var drone) && controller.targetTransform == null)
                    {
                        drone.AlertNearbyDrones();
                    }
                    controller.targetTransform = collider.transform;
                    controller.targetPos = colliderPos;
                    return INode.State.SUCCESS;
                }
            }
            
            return INode.State.FAILED;
        }
    }
}