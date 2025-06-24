using System.Collections;
using System.Collections.Generic;
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
        private BTContext context;

        public INode.State Evaluate(BTContext context)
        {
            this.context = context;
            
            if (isRunning) return INode.State.RUN;

            context.controller.IsCurrentActionRunning(true);
            context.controller.StartCoroutine(RollCoroutine());
            return INode.State.RUN;
        }

        private IEnumerator RollCoroutine()
        {
            isRunning = true;
            Debug.Log("Roll Start");

            yield return new WaitForSeconds(1.0f);

            Debug.Log("Roll End");
            context.controller.IsCurrentActionRunning(false);
            isRunning = false;
        }
    }

}
