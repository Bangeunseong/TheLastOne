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
    /// 노드 구조상 SUCCESS 반환해야 할 때 사용
    /// </summary>
    public class ReturnSuccess : INode
    {
        public INode.State Evaluate(BaseNpcAI controller)
        {
            return INode.State.SUCCESS;
        }
    }
}