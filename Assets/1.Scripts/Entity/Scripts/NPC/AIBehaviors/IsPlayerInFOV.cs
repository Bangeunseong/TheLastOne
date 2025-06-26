using System.Collections;
using System.Collections.Generic;
using _1.Scripts.Entity.Scripts.Common;
using _1.Scripts.Entity.Scripts.NPC.AIControllers;
using _1.Scripts.Entity.Scripts.NPC.BehaviorTree;
using _1.Scripts.Entity.Scripts.NPC.Data;
using _1.Scripts.Interfaces;
using _1.Scripts.Manager.Core;
using UnityEngine;

namespace _1.Scripts.Entity.Scripts.NPC.AIBehaviors
{
    /// <summary>
    /// 플레이어가 육안으로 보이는 곳에 있는지 검사
    /// </summary>
    public class IsPlayerInFOV : INode 
    {
        public INode.State Evaluate(BaseNpcAI controller)
        {
            Vector3 origin = controller.transform.position;
            Vector3 direction = (CoreManager.Instance.gameManager.Player.transform.position - origin).normalized;

            // 시야 최대 거리 설정 (예: 100f)
            float maxViewDistance = 100f;
            
            Debug.DrawRay(origin + Vector3.up, direction * maxViewDistance, Color.red);
            if (Physics.Raycast(origin + Vector3.up, direction, out RaycastHit hit, maxViewDistance))
            {
                if (hit.collider.CompareTag("Player"))
                {
                    Debug.Log("플레이어 시야 내에 보임. 타이머 초기화");
                    return INode.State.SUCCESS;
                }
            }

            return INode.State.FAILED;
        }
    }   
}