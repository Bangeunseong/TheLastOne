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
    /// 배회경로 지정
    /// </summary>
    public class SetDestinationToPatrol : INode
    {
        public INode.State Evaluate(BaseNpcAI controller)
        {
            controller.navMeshAgent.SetDestination(CoreManager.Instance.gameManager.Player.transform.position);
            return INode.State.RUN;
        }
    }
}