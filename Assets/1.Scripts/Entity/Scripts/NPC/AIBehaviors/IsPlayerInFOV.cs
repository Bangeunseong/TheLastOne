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
            return INode.State.FAILED;
        }
    }   
}