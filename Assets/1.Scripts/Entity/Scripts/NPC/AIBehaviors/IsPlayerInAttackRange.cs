using System.Collections;
using System.Collections.Generic;
using _1.Scripts.Entity.Scripts.NPC.AIControllers;
using _1.Scripts.Entity.Scripts.NPC.BehaviorTree;
using _1.Scripts.Interfaces;
using _1.Scripts.Manager.Core;
using UnityEngine;


namespace _1.Scripts.Entity.Scripts.NPC.AIBehaviors
{
    /// <summary>
    /// 플레이어가 AttackRange 내에 있는지 검사하는 노드, 반드시 컨트롤러의 스탯에 IAttackable이 있어야함
    /// </summary>
    public class IsPlayerInAttackRange : INode
    {
        public INode.State Evaluate(BaseNpcAI controller)
        {
            if (!controller.statController.TryGetRuntimeStatInterface<IAttackable>(out var attackable))
            {
                Debug.LogWarning($"[IsPlayerInAttackRange] {controller.name}이 IAttackable을 구현하지 않음");
                return INode.State.FAILED;
            }

            Vector3 npcPos = controller.transform.position;
            Vector3 playerPos = CoreManager.Instance.gameManager.Player.transform.position;

            float attackRange = attackable.AttackRange;
            float sqrDist = (npcPos - playerPos).sqrMagnitude;
            if (sqrDist > attackRange * attackRange)
            {
                return INode.State.FAILED;
            }

            if (Service.IsPlayerVisible(npcPos + Vector3.up, 100f))
            {
                // 공격 거리 안이며 플레이어가 보이는 곳에 있음
                Debug.Log("공격 거리 안이며 플레이어가 보이는 곳에 있음");
                return INode.State.SUCCESS;
            }
            else
            {
                return INode.State.FAILED;
            }
        }
    }
}