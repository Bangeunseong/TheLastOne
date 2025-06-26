using System.Collections;
using System.Collections.Generic;
using _1.Scripts.Entity.Scripts.NPC.AIControllers;
using _1.Scripts.Entity.Scripts.NPC.BehaviorTree;
using UnityEngine;

namespace _1.Scripts.Entity.Scripts.NPC.AIBehaviors
{
    /// <summary>
    /// 노드 구조상 RUN 반환해야 할 때 사용
    /// </summary>
    public class ReturnRun : INode
    {
        public INode.State Evaluate(BaseNpcAI controller)
        {
            return INode.State.RUN;
        }
    }
}