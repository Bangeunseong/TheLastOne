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
        private Transform transform;
        private float rollDuration = 1.0f;
        private float rollTimer = 0f;
        private bool isRolling = false;

        public RollActionNode(Transform transform)
        {
            this.transform = transform;
        }
        
        public INode.State Evaluate()
        {
            if (!isRolling)
            {
                isRolling = true;
                rollTimer = 0f;
                PlayRollAnimation();
            }

            rollTimer += Time.deltaTime;
            if (rollTimer >= rollDuration)
            {
                isRolling = false;
                return INode.State.SUCCESS;
            }

            return INode.State.RUN;
        }

        private void PlayRollAnimation()
        {
            Debug.Log("Roll Start Animation");
            // anim.SetTrigger("Roll");
        }
    }
}
