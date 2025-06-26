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
    /// 플레이어가 DetectRange 내에 있는지 검사하는 노드, 반드시 컨트롤러의 스탯에 IDetectable이 있어야함
    /// </summary>
    public class IsPlayerInDetectRange : INode 
    {
        public INode.State Evaluate(BaseNpcAI controller)
        {
            if (!controller.statController.TryGetRuntimeStatInterface<IDetectable>(out var detectable)) // 있을 시 변환
            {
                Debug.LogWarning($"[IsPlayerInDetectRange] statData가 IDetectable을 구현하지 않음. 컨트롤러: {controller.name}");
                return INode.State.FAILED;
            }
            
            float detectRange = detectable.DetectRange;
            float sqrDetectRange = detectRange * detectRange;

            Vector3 pos = controller.transform.position;
            Vector3 playerPos = CoreManager.Instance.gameManager.Player.transform.position;
            float sqrDistance = (pos - playerPos).sqrMagnitude;
            
            return sqrDistance <= sqrDetectRange ? INode.State.SUCCESS : INode.State.FAILED;
        }
    }   
}
