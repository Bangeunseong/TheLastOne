using System.Collections;
using System.Collections.Generic;
using _1.Scripts.Entity.Scripts.NPC.AIControllers.Base;
using _1.Scripts.Entity.Scripts.NPC.AIControllers.Drone;
using _1.Scripts.Entity.Scripts.NPC.BehaviorTree;
using _1.Scripts.Entity.Scripts.NPC.Data.AnimationHashData;
using _1.Scripts.Entity.Scripts.Npc.StatControllers.Base;
using _1.Scripts.Interfaces.Common;
using _1.Scripts.Interfaces.NPC;
using _1.Scripts.Manager.Core;
using _1.Scripts.Manager.Subs;
using _1.Scripts.Static;
using UnityEngine;


namespace _1.Scripts.Entity.Scripts.NPC.AIBehaviors.Drone.SuicideDrone
{
    public class SuicideDroneAttack : INode
    {
        private bool isExploded = false;

        public INode.State Evaluate(BaseNpcAI controller)
        {
            if (controller.TryGetAIController<SuicideDroneAIController>(out var droneController))
            {
                if (isExploded) // 이미 폭발했으면 return
                {
                    return INode.State.SUCCESS;
                }
                
                if (!droneController.statController.TryGetRuntimeStatInterface<IBoomable>(out var boomable))
                {
                    return INode.State.FAILED;
                }
                
                Service.Log("자폭공격!");
                
                // 1. 파티클 생성
                droneController.explosionParticle.Play();
                CoreManager.Instance.soundManager.PlaySFX(SfxType.Drone, controller.transform.position, 4);
                
                
                // 2. 데미지 주기
                bool isAlly = controller.statController.RuntimeStatData.IsAlly;
                Vector3 selfPos = controller.transform.position;
                float range = boomable.BoomRange;
            
                int layerMask = isAlly ? 1 << LayerConstants.Enemy :  1 << LayerConstants.Ally | 1 << LayerConstants.Chest;
                Collider[] colliders = Physics.OverlapSphere(selfPos, range, layerMask);
                foreach (Collider collider in colliders)
                {
                    Service.Log($"{collider.gameObject.name}");
                    if (collider.gameObject == droneController.gameObject)
                    {
                        Service.Log("콘티뉴");
                        continue;   
                    }
                    
                    if (collider.TryGetComponent(out IDamagable damagable))
                    {
                        Service.Log("데미지 입히기 실행");
                        Service.Log($"{droneController.statController.RuntimeStatData.BaseDamage}");
                        damagable.OnTakeDamage(droneController.statController.RuntimeStatData.BaseDamage);
                    }
                }

                // 2. 드론 렌더러 전부 끄기
                foreach (var rend in droneController.GetComponentsInChildren<Renderer>())
                {
                    if (rend.gameObject.name.Contains("Explosion"))
                    {
                        continue;
                    }
                    rend.enabled = false;
                }
                
                // 3. 드론 콜라이더 비활성화
                Collider droneCollider = droneController.GetComponent<Collider>();
                droneCollider.enabled = false;

                // 4. 드론 죽음 true (트리 정지)
                droneController.statController.Dead();

                // 5. 기다린 후 파괴
                droneController.StartCoroutine(DelayedDestroy(droneController));
                
                isExploded = true;
                return INode.State.RUN;
            }

            return INode.State.FAILED;
        }

        private IEnumerator DelayedDestroy(SuicideDroneAIController droneController)
        {
            yield return new WaitForSeconds(2.5f);
            Object.Destroy(droneController.gameObject);
        }
    }
}