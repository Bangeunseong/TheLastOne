using System.Collections;
using System.Collections.Generic;
using _1.Scripts.Entity.Scripts.NPC.BehaviorTree;
using _1.Scripts.Interfaces;
using _1.Scripts.Manager.Core;
using UnityEngine;

namespace _1.Scripts.Entity.Scripts.NPC.AIBehaviors.Drone.ReconDrone
{
    public class ReconDroneAttacking : INode
    {
        public INode.State Evaluate(BTContext context)
        {
            // 레콘드론 공격
            return INode.State.RUN;
        }
    }
}
