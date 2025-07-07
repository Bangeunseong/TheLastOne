using System.Collections;
using System.Collections.Generic;
using _1.Scripts.Entity.Scripts.NPC.AIBehaviors.BehaviorDesigner.SharedVariables;
using _1.Scripts.Entity.Scripts.NPC.AIControllers.Drone;
using _1.Scripts.Entity.Scripts.NPC.BehaviorTree;
using _1.Scripts.Interfaces.Common;
using _1.Scripts.Interfaces.NPC;
using _1.Scripts.Manager.Core;
using _1.Scripts.Manager.Subs;
using _1.Scripts.Static;
using UnityEngine;
using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;

namespace _1.Scripts.Entity.Scripts.NPC.AIBehaviors.BehaviorDesigner.Action.DroneOnly
{
    [TaskCategory("DroneOnly")]
    [TaskDescription("SuicideDroneAttacking")]
    public class SuicideDroneAttacking : global::BehaviorDesigner.Runtime.Tasks.Action
    {
        public SharedTransform selfTransform;
        public SharedBaseNpcStatController statController;
        public SharedParticleSystem explosionParticle;
        public SharedCollider myCollider;
        public SharedBool isDead;
        
        private bool isExploded = false;
        
        public override TaskStatus OnUpdate()
        {
            if (isExploded) // 이미 폭발했으면 return
            {
                return TaskStatus.Running;
            }
            
            if (!statController.Value.TryGetRuntimeStatInterface<IBoomable>(out var boomable))
            {
                return TaskStatus.Failure;
            }
            
            // 1. 파티클 생성 및 사운드 출력
            explosionParticle.Value.Play();
            CoreManager.Instance.soundManager.PlaySFX(SfxType.Drone, selfTransform.Value.position, index:4);
                
            // 2. 데미지 주기
            bool isAlly = statController.Value.RuntimeStatData.isAlly;
            Vector3 selfPos = selfTransform.Value.position;
            float range = boomable.BoomRange;
            
            int layerMask = isAlly ? 1 << LayerConstants.Enemy :  1 << LayerConstants.Ally | 1 << LayerConstants.Chest;
            Collider[] colliders = Physics.OverlapSphere(selfPos, range, layerMask);
            foreach (Collider collider in colliders)
            {
                if (collider.gameObject == selfTransform.Value.gameObject)
                {
                    continue;   
                }
                
                if (collider.TryGetComponent(out IDamagable damagable))
                {
                    Service.Log("데미지 입히기 실행");
                    Service.Log($"{statController.Value.RuntimeStatData.baseDamage}");
                    damagable.OnTakeDamage(statController.Value.RuntimeStatData.baseDamage);
                }
            }

            // 3. 드론 렌더러 전부 끄기
            foreach (var rend in selfTransform.Value.GetComponentsInChildren<Renderer>())
            {
                if (rend.gameObject.name.Contains("Explosion"))
                {
                    continue;
                }
                rend.enabled = false;
            }
            
            // 4. 드론 콜라이더 비활성화
            myCollider.Value.enabled = false;
            
            // 5. isDead = true
            isDead.Value = true;
            
            // 6. 기다린 후 파괴
            StartCoroutine(DelayedDestroy(selfTransform.Value));
            
            isExploded = true;
            return TaskStatus.Running;
        }

        private IEnumerator DelayedDestroy(Transform destroyTarget)
        {
            yield return new WaitForSeconds(2.5f);
            Object.Destroy(destroyTarget.gameObject);
        }
    }
}