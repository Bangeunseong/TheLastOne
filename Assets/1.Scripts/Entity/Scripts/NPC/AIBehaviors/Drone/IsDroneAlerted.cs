using System.Collections;
using System.Collections.Generic;
using _1.Scripts.Entity.Scripts.NPC.BehaviorTree;
using UnityEngine;

namespace _1.Scripts.Entity.Scripts.NPC.AIBehaviors.Drone
{
    public class IsDroneAlerted : INode
    {
        public INode.State Evaluate(BTContext context)
        {
            // 레콘드론 공격
            return INode.State.SUCCESS;
        }
    }
}
