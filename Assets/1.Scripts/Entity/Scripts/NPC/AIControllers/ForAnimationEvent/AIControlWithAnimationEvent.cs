using System;
using System.Collections;
using System.Collections.Generic;
using _1.Scripts.Entity.Scripts.NPC.AIBehaviors.BehaviorDesigner.SharedVariables;
using _1.Scripts.Interfaces.NPC;
using _1.Scripts.Manager.Core;
using _1.Scripts.Quests.Core;
using UnityEngine;
using UnityEngine.AI;

namespace _1.Scripts.Entity.Scripts.NPC.AIControllers.ForAnimationEvent
{
    public class AIControlWithAnimationEvent : MonoBehaviour
    {
        private BehaviorDesigner.Runtime.BehaviorTree behaviorTree;

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
            var statController = behaviorTree.GetVariable("statController") as SharedBaseNpcStatController;

            if (statController != null && statController.Value is IStunnable stunnable)
            {
                if (!stunnable.IsStunned)
                {
                    behaviorTree.SetVariableValue("CanRun", true);
                }
            }
            else
            {
                behaviorTree.SetVariableValue("CanRun", true);
            }
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
        
        public void DestroyObjectForAnimationEvent()
        {
            CoreManager.Instance.spawnManager.RemoveMeFromSpawnedEnemies(this.gameObject);
            CoreManager.Instance.objectPoolManager.Release(this.gameObject);
        }
        
        public void DestroyObjectAndQuestCountForAnimationEvent()
        {
            CoreManager.Instance.spawnManager.RemoveMeFromSpawnedEnemies(this.gameObject);
            CoreManager.Instance.objectPoolManager.Release(this.gameObject);
            GameEventSystem.Instance.RaiseEvent(3);
        }
    }
}
