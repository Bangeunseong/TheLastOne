using System.Collections;
using _1.Scripts.Entity.Scripts.NPC.AIBehaviors.BehaviorDesigner.SharedVariables;
using _1.Scripts.Entity.Scripts.NPC.Data.AnimationHashData;
using _1.Scripts.Util;
using UnityEngine;
using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;

namespace _1.Scripts.Entity.Scripts.NPC.AIBehaviors.BehaviorDesigner.Action.ShebotOnly
{
    [TaskCategory("ShebotOnly")]
    [TaskDescription("ShebotSniperFire")]
    public class ShebotSniperFire : global::BehaviorDesigner.Runtime.Tasks.Action
    {
        public SharedTransform selfTransform;
        public SharedFloat shootInterval;
        public SharedFloat reloadingDuration;
        public SharedLineRenderer lineRenderer;
        public SharedAnimator animator;
        public SharedBool isAlerted;
        public SharedTransform targetTransform;
        public SharedVector3 targetPos;
        public SharedQuaternion baseRotation;
        public SharedLight enemyLight;
        public SharedLight allyLight;
        
        private float timer;
        private bool hasFired;
        
        public override void OnStart()
        {
            timer = 0f;
            hasFired = false;
        }

        public override TaskStatus OnUpdate()
        {
            timer += Time.deltaTime;

            if (!hasFired)
            {
                if (timer >= shootInterval.Value)
                {
                    animator.Value.SetTrigger(ShebotAnimationHashData.Shebot_Rifle_fire);
                    lineRenderer.Value.enabled = false;
                    hasFired = true;
                    timer = 0f;
                }
            }
            else if (timer >= reloadingDuration.Value)
            {
                enemyLight.Value.enabled = false;
                allyLight.Value.enabled = false;
                isAlerted.Value = false;
                targetTransform.Value = null;
                targetPos.Value = Vector3.zero;
                baseRotation.Value = selfTransform.Value.rotation;
                
                return TaskStatus.Success;
            }
            
            return TaskStatus.Running;
        }
    }
}