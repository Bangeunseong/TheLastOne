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
            if (controller.navMeshAgent.hasPath)
            {
                return INode.State.FAILED;
            }
            else
            {
                Debug.Log("경로없음");
                return INode.State.SUCCESS;
            }
        }
    }
}