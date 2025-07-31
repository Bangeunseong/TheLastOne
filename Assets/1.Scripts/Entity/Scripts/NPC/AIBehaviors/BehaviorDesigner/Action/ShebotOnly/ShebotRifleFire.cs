using _1.Scripts.Entity.Scripts.NPC.AIBehaviors.BehaviorDesigner.SharedVariables;
using _1.Scripts.Entity.Scripts.NPC.Data.AnimationHashData;
using _1.Scripts.Static;
using _1.Scripts.Util;
using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using UnityEngine;

namespace _1.Scripts.Entity.Scripts.NPC.AIBehaviors.BehaviorDesigner.Action.ShebotOnly
{
    [TaskCategory("ShebotOnly")]
    [TaskDescription("ShebotRifleFire")]
    public class ShebotRifleFire : global::BehaviorDesigner.Runtime.Tasks.Action
    {
        public SharedAnimator animator;
        public SharedTransform selfTransform;
        public SharedTransform targetTransform;
        public SharedVector3 targetPosition;
        public SharedBaseNpcStatController statController;

        private Collider targetChset;
        public override void OnStart()
        {
            animator.Value.SetTrigger(ShebotAnimationData.Shebot_Rifle_fire_2);
            int layer = statController.Value.RuntimeStatData.IsAlly ? LayerConstants.Chest_E : LayerConstants.Chest_P;
            targetChset = NpcUtil.FindColliderOfLayerInChildren(targetTransform.Value.gameObject, layer);
        }

        public override TaskStatus OnUpdate()
        {
            AnimatorStateInfo stateInfo = animator.Value.GetCurrentAnimatorStateInfo(0);
            
            if (stateInfo.IsName(ShebotAnimationData.Shebot_Rifle_fire_2Str) && animator.Value.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1)
            {
                return TaskStatus.Success;
            }
            
            targetPosition.Value = targetChset.bounds.center;
            NpcUtil.LookAtTarget(selfTransform.Value, targetTransform.Value.position, additionalYangle:55);
            return TaskStatus.Running;
        }
    }
}