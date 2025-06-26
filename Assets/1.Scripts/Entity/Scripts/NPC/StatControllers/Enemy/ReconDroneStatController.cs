using System;
using System.Collections;
using System.Collections.Generic;
using _1.Scripts.Entity.Scripts.NPC.Data;
using _1.Scripts.Entity.Scripts.NPC.Data.ForRuntime;
using _1.Scripts.Entity.Scripts.NPC.Data.StatDataSO;
using _1.Scripts.Entity.Scripts.Npc.StatControllers;
using _1.Scripts.Interfaces;
using _1.Scripts.Manager.Core;
using UnityEngine;

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
        }

        public override void OnTakeDamage(int damage)
        {
            runtimeReconDroneStatData.maxHealth -= damage;
            if (runtimeReconDroneStatData.maxHealth <= 0)
            {
                // Die 로직 추가
                Destroy(gameObject);
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
    }
}
