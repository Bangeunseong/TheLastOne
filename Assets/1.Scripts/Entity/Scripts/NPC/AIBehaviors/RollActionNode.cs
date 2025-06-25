using System.Collections;
using System.Collections.Generic;
using _1.Scripts.Entity.Scripts.NPC.AIControllers;
using _1.Scripts.Entity.Scripts.NPC.BehaviorTree;
using UnityEngine;

namespace _1.Scripts.Entity.Scripts.NPC.AIBehaviors
{
    /// <summary>
    /// 테스트 중
    /// </summary>
    public class RollActionNode : INode
    {
        private bool isRunning = false;
        private BaseNpcAI controller;
        
        public INode.State Evaluate(BaseNpcAI aiController)
        {
            controller = aiController;
            
            if (isRunning) return INode.State.RUN;

            controller.IsCurrentActionRunning(true);
            controller.StartCoroutine(RollCoroutine());
            return INode.State.RUN;
        }

        private IEnumerator RollCoroutine()
        {
            isRunning = true;
            Debug.Log("Roll Start");

            yield return new WaitForSeconds(1.0f);

            Debug.Log("Roll End");
            controller.IsCurrentActionRunning(false);
            isRunning = false;
        }
    }

}
