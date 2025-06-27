using System;
using System.Collections;
using System.Collections.Generic;
using _1.Scripts.Entity.Scripts.NPC.Data;
using _1.Scripts.Entity.Scripts.NPC.Data.AnimationHashData;
using _1.Scripts.Entity.Scripts.NPC.Data.ForRuntime;
using _1.Scripts.Entity.Scripts.NPC.Data.StatDataSO;
using _1.Scripts.Entity.Scripts.Npc.StatControllers;
using _1.Scripts.Interfaces;
using _1.Scripts.Manager.Core;
using UnityEngine;
using Random = System.Random;

namespace _1.Scripts.Entity.Scripts.NPC.StatControllers.Enemy
{
    public class ReconDroneStatController : BaseNpcStatController
    {
        private RuntimeReconDroneStatData runtimeReconDroneStatData;
        public override RuntimeEntityStatData RuntimeStatData => runtimeReconDroneStatData; // 부모에게 자신의 스탯 전송

        [Header("속도 저장용")]
        private float baseMoveSpeed;
        
        private void Awake()
        {
            var reconDroneStatData = CoreManager.Instance.resourceManager.GetAsset<ReconDroneStatData>("ReconDroneStatData"); // 자신만의 데이터 가져오기
            runtimeReconDroneStatData = new RuntimeReconDroneStatData(reconDroneStatData); // 복사
            baseMoveSpeed = runtimeReconDroneStatData.moveSpeed;
            animator = GetComponent<Animator>();
        }

        private void Update()
        {
            runtimeReconDroneStatData.isAlly = isAlly;
        }

        public override void OnTakeDamage(int damage)
        {
            if (!isDead)
            {
                runtimeReconDroneStatData.maxHealth -= damage;
                animator.SetTrigger(DroneAnimationHashData.Hit3);
                if (runtimeReconDroneStatData.maxHealth <= 0)
                {
                    int[] deathHashes = new int[]
                    {
                        DroneAnimationHashData.Dead1,
                        DroneAnimationHashData.Dead2,
                        DroneAnimationHashData.Dead3
                    };

                    int randomIndex = UnityEngine.Random.Range(0, deathHashes.Length);
                    animator.SetTrigger(deathHashes[randomIndex]);

                    isDead = true;
                }
            }
        }

        public override void ModifySpeed(float percent)
        {
            runtimeReconDroneStatData.moveSpeed *= percent;
        }

        public override void Running(bool isRunning)
        {
            runtimeReconDroneStatData.moveSpeed = isRunning ? baseMoveSpeed + runtimeReconDroneStatData.runMultiplier : baseMoveSpeed;
        }

        public void DestroyObjectForAnimationEvent()
        {
            Destroy(gameObject);
        }
    }
}
