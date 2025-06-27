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
    /// 지정한 경로가 없어야 SUCCESS
    /// </summary>
    public class IsNavMeshAgentNotHasPath : INode
    {
        public INode.State Evaluate(BaseNpcAI controller)
        {
            controller.shouldLookTarget = false; // 보통 노드구조 맨 끝자락에 배회할때 쓰니까 추가
            
            return controller.navMeshAgent.hasPath ? INode.State.FAILED : INode.State.SUCCESS;
        }
    }
}