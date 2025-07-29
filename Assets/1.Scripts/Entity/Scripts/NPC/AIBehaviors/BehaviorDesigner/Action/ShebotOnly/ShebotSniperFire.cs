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
        public SharedFloat shootInterval;
        public SharedFloat reloadingDuration;
        public SharedLineRenderer lineRenderer;
        
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
                    // 쏘는 애니메이션 실행 (사격은 내부 애니메이션 이벤트에 존재)
                    lineRenderer.Value.enabled = false;
                    hasFired = true;
                    timer = 0f;
                }
            }
            else if (timer >= reloadingDuration.Value)
            {
                return TaskStatus.Success;
            }
            
            return TaskStatus.Running;
        }
    }
}