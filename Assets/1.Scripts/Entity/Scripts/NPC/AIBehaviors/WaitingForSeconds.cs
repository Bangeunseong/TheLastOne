using System.Collections;
using System.Collections.Generic;
using _1.Scripts.Entity.Scripts.NPC.AIControllers;
using _1.Scripts.Entity.Scripts.NPC.BehaviorTree;
using UnityEngine;

namespace _1.Scripts.Entity.Scripts.NPC.AIBehaviors
{
    /// <summary>
    /// 몇 초 기다려야 할 때 사용
    /// </summary>
    public class WaitingForSeconds : INode
    {
        private float minWait;
        private float maxWait;
        private float waitTime;
        private float startTime;
        private bool isWaiting;

        public WaitingForSeconds(float min, float max)
        {
            minWait = min;
            maxWait = max;
        }

        public INode.State Evaluate(BaseNpcAI controller)
        {
            if (!isWaiting)
            {
                waitTime = Random.Range(minWait, maxWait); // 랜덤으로 지정
                startTime = Time.time;
                Debug.Log(startTime);
                isWaiting = true;
            }

            if (Time.time - startTime >= waitTime)
            {
                isWaiting = false;
                return INode.State.SUCCESS;
            }

            return INode.State.RUN;
        }
    }
}