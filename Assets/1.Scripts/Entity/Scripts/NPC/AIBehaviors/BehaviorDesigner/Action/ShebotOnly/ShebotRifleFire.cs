using _1.Scripts.Entity.Scripts.NPC.AIBehaviors.BehaviorDesigner.SharedVariables;
using _1.Scripts.Entity.Scripts.NPC.Data.AnimationHashData;
using BehaviorDesigner.Runtime.Tasks;
using UnityEngine;

namespace _1.Scripts.Entity.Scripts.NPC.AIBehaviors.BehaviorDesigner.Action.ShebotOnly
{
    [TaskCategory("ShebotOnly")]
    [TaskDescription("ShebotRifleFire")]
    public class ShebotRifleFire : global::BehaviorDesigner.Runtime.Tasks.Action
    {
        public SharedAnimator animator;
        
        public override void OnStart()
        {
            animator.Value.SetTrigger(ShebotAnimationData.Shebot_Rifle_fire_2);
        }

        public override TaskStatus OnUpdate()
        {
            AnimatorStateInfo stateInfo = animator.Value.GetCurrentAnimatorStateInfo(0);
            
            if (stateInfo.IsName(ShebotAnimationData.Shebot_Rifle_fire_2Str) && animator.Value.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1)
            {
                return TaskStatus.Success;
            }
            
            return TaskStatus.Running;
        }
    }
}