using System.Collections;
using System.Collections.Generic;
using _1.Scripts.Entity.Scripts.NPC.AIControllers.Base;
using _1.Scripts.Entity.Scripts.NPC.BehaviorTree;
using _1.Scripts.Entity.Scripts.NPC.Data.AnimationHashData;
using UnityEngine;


namespace _1.Scripts.Entity.Scripts.NPC.AIBehaviors.Drone.SuicideDrone
{
    public class SuicideDroneAttack : INode
    {
        public INode.State Evaluate(BaseNpcAI controller)
        {
            if (controller.TryGetAIController<BaseDroneAIController>(out var droneController))
            {
                Service.Log("자폭공격!");
                return INode.State.RUN;
            }

            return INode.State.FAILED;
        }
    }
}