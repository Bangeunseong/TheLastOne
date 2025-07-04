using System;
using System.Collections;
using System.Collections.Generic;
using _1.Scripts.Entity.Scripts.NPC.AIBehaviors.BehaviorDesigner.SharedVariables;
using UnityEngine;
using UnityEngine.AI;

namespace  _1.Scripts.Entity.Scripts.NPC.AIControllers.ForAnimationEvent
{
    public class AIControlWithAnimationEvent : MonoBehaviour
    {
        public BehaviorDesigner.Runtime.BehaviorTree behaviorTree;

        private void Awake()
        {
            behaviorTree = GetComponent<BehaviorDesigner.Runtime.BehaviorTree>();
        }
        
        public void AIOffForAnimationEvent()
        {
            behaviorTree.SetVariableValue("CanRun", false);
        }

        public void AIOnForAnimationEvent()
        {
            behaviorTree.SetVariableValue("CanRun", true);
        }

        public void SetDestinationNullForAnimationEvent()
        {
            var sharedAgent = behaviorTree.GetVariable("agent") as SharedNavMeshAgent;

            if (sharedAgent != null && sharedAgent.Value != null)
            {
                sharedAgent.Value.SetDestination(transform.position);
            }
            else
            {
                Debug.LogWarning("SharedNavMeshAgent를 찾을 수 없거나 값이 없습니다.");
            }
        }
    }
}
