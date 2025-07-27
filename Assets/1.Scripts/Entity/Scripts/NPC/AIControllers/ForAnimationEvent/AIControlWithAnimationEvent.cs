using System;
using System.Collections;
using System.Collections.Generic;
using _1.Scripts.Entity.Scripts.NPC.AIBehaviors.BehaviorDesigner.SharedVariables;
using _1.Scripts.Interfaces.NPC;
using _1.Scripts.Manager.Core;
using _1.Scripts.Quests.Core;
using _1.Scripts.Static;
using _1.Scripts.Util;
using UnityEngine;
using UnityEngine.AI;

namespace _1.Scripts.Entity.Scripts.NPC.AIControllers.ForAnimationEvent
{
    public class AIControlWithAnimationEvent : MonoBehaviour
    {
        private BehaviorDesigner.Runtime.BehaviorTree behaviorTree;
        private Animator animator;
        
        private void Awake()
        {
            behaviorTree = GetComponent<BehaviorDesigner.Runtime.BehaviorTree>();
            animator = GetComponent<Animator>();
        }
        
        public void AIOffForAnimationEvent()
        {
            behaviorTree.SetVariableValue(BehaviorNames.CanRun, false);
        }

        public void AIOnForAnimationEvent()
        {
            var statController = behaviorTree.GetVariable(BehaviorNames.StatController) as SharedBaseNpcStatController;

            if (statController != null && statController.Value is IStunnable stunnable)
            {
                if (!stunnable.IsStunned)
                {
                    behaviorTree.SetVariableValue(BehaviorNames.CanRun, true);
                }
            }
            else
            {
                behaviorTree.SetVariableValue(BehaviorNames.CanRun, true);
            }
        }

        public void SetDestinationNullForAnimationEvent()
        {
            var sharedAgent = behaviorTree.GetVariable(BehaviorNames.Agent) as SharedNavMeshAgent;

            if (sharedAgent != null && sharedAgent.Value != null)
            {
                sharedAgent.Value.SetDestination(transform.position);
            }
            else
            {
                Debug.LogWarning("SharedNavMeshAgent를 찾을 수 없거나 값이 없습니다.");
            }
        }
        
        public void DestroyObjectForAnimationEvent()
        {
            NpcUtil.DisableNpc(this.gameObject);
        }

        public void EnableApplyRootMotion()
        {
            animator.applyRootMotion = true;
        }
    }
}
