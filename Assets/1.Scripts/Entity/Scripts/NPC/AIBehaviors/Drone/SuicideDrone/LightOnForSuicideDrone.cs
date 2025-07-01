using System.Collections;
using System.Collections.Generic;
using _1.Scripts.Entity.Scripts.NPC.AIControllers.Base;
using _1.Scripts.Entity.Scripts.NPC.BehaviorTree;
using _1.Scripts.Entity.Scripts.NPC.Data.AnimationHashData;
using _1.Scripts.Manager.Core;
using _1.Scripts.Manager.Subs;
using UnityEngine;

namespace _1.Scripts.Entity.Scripts.NPC.AIBehaviors.Drone.SuicideDrone
{
    public class LightOnForSuicideDrone : INode
    {
        public INode.State Evaluate(BaseNpcAI controller)
        {
            if (controller.TryGetAIController<BaseDroneAIController>(out var droneController))
            {
                droneController.alertLight.enabled = true;
                CoreManager.Instance.soundManager.PlaySFX(SfxType.Drone, droneController.MyPos, 1);
                return INode.State.SUCCESS;
            }
            return INode.State.FAILED;
        }
    }
}