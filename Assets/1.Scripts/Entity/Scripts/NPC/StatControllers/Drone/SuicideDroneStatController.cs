using System.Collections;
using System.Collections.Generic;
using _1.Scripts.Entity.Scripts.NPC.Data.AnimationHashData;
using _1.Scripts.Entity.Scripts.NPC.Data.ForRuntime;
using _1.Scripts.Entity.Scripts.NPC.Data.StatDataSO;
using _1.Scripts.Entity.Scripts.Npc.StatControllers.Base;
using _1.Scripts.Manager.Core;
using BehaviorDesigner.Runtime;
using UnityEngine;

namespace _1.Scripts.Entity.Scripts.NPC.StatControllers.Drone
{
    public class SuicideDroneStatController : BaseNpcStatController
    {
        private RuntimeSuicideDroneStatData runtimeSuicideDroneStatData;
        public override RuntimeEntityStatData RuntimeStatData => runtimeSuicideDroneStatData; // 부모에게 자신의 스탯 전송
        
        [Header("AI 참조")]
        public BehaviorDesigner.Runtime.BehaviorTree behaviorTree;
        public SharedBool canRun;
        
        [Header("속도 저장용")]
        private float baseMoveSpeed;
        
        private void Awake()
        {
            var suicideDroneStatData = CoreManager.Instance.resourceManager.GetAsset<SuicideDroneStatData>("SuicideDroneStatData"); // 자신만의 데이터 가져오기
            runtimeSuicideDroneStatData = new RuntimeSuicideDroneStatData(suicideDroneStatData); // 복사
            baseMoveSpeed = runtimeSuicideDroneStatData.moveSpeed;
            animator = GetComponent<Animator>();
            behaviorTree = GetComponent<BehaviorDesigner.Runtime.BehaviorTree>();
            canRun = behaviorTree.GetVariable("CanRun") as SharedBool;
        }

        public override void OnTakeDamage(int damage)
        {
            if (!isDead)
            {
                runtimeSuicideDroneStatData.maxHealth -= damage;
                if (runtimeSuicideDroneStatData.maxHealth <= 0)
                {
                    isDead = true;
                    canRun = false;
                    
                    int[] deathHashes = new int[]
                    {
                        DroneAnimationHashData.Dead1,
                        DroneAnimationHashData.Dead2,
                        DroneAnimationHashData.Dead3
                    };

                    int randomIndex = UnityEngine.Random.Range(0, deathHashes.Length);
                    animator.SetTrigger(deathHashes[randomIndex]);  
                }
                else
                {
                    animator.SetTrigger(DroneAnimationHashData.Hit3);
                }
            }
        }

        public override void ModifySpeed(float percent)
        {
            runtimeSuicideDroneStatData.moveSpeed *= percent;
        }

        public override void Running(bool isRunning)
        {
            runtimeSuicideDroneStatData.moveSpeed = isRunning ? baseMoveSpeed + runtimeSuicideDroneStatData.runMultiplier : baseMoveSpeed;
        }

        public void DestroyObjectForAnimationEvent()
        {
            Destroy(gameObject);
        }

        public override void Hacking()
        {
            runtimeSuicideDroneStatData.isAlly = true;
        }
    }
}
