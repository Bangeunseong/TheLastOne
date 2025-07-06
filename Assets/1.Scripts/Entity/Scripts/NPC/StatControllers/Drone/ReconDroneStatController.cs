using System;
using System.Collections;
using System.Collections.Generic;
using _1.Scripts.Entity.Scripts.NPC.Data;
using _1.Scripts.Entity.Scripts.NPC.Data.AnimationHashData;
using _1.Scripts.Entity.Scripts.NPC.Data.ForRuntime;
using _1.Scripts.Entity.Scripts.NPC.Data.StatDataSO;
using _1.Scripts.Entity.Scripts.Npc.StatControllers.Base;
using _1.Scripts.Interfaces;
using _1.Scripts.Manager.Core;
using _1.Scripts.Static;
using _1.Scripts.Util;
using UnityEngine;
using Random = System.Random;

namespace _1.Scripts.Entity.Scripts.NPC.StatControllers.Drone
{
    public class ReconDroneStatController : BaseNpcStatController
    {
        private RuntimeReconDroneStatData runtimeReconDroneStatData;
        public override RuntimeEntityStatData RuntimeStatData => runtimeReconDroneStatData; // 부모에게 자신의 스탯 전송
        
        [Header("Behavior Tree")]
        private BehaviorDesigner.Runtime.BehaviorTree behaviorTree;
        
        private void Awake()
        {
            var reconDroneStatData = CoreManager.Instance.resourceManager.GetAsset<ReconDroneStatData>("ReconDroneStatData"); // 자신만의 데이터 가져오기
            runtimeReconDroneStatData = new RuntimeReconDroneStatData(reconDroneStatData); // 복사
            animator = GetComponent<Animator>();
            behaviorTree = GetComponent<BehaviorDesigner.Runtime.BehaviorTree>();
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
                else
                {
                    int[] HitHashes = new int[]
                    {
                        DroneAnimationHashData.Hit1,
                        DroneAnimationHashData.Hit2,
                        DroneAnimationHashData.Hit3,
                        DroneAnimationHashData.Hit4
                    };

                    int randomIndex = UnityEngine.Random.Range(0, HitHashes.Length);
                    animator.SetTrigger(HitHashes[randomIndex]);
                }
            }
        }

        public override void ModifySpeed(float percent)
        {
            runtimeReconDroneStatData.moveSpeed *= percent;
        }

        public void DestroyObjectForAnimationEvent()
        {
            Destroy(gameObject);
        }

        public override void Hacking()
        {
            if (!runtimeReconDroneStatData.isAlly)
            {
                runtimeReconDroneStatData.isAlly = true;
                NpcUtil.SetLayerRecursively(this.gameObject, LayerConstants.Ally);
                
                behaviorTree.SetVariableValue("target_Transform", null);
                behaviorTree.SetVariableValue("target_Pos", Vector3.zero);
                behaviorTree.SetVariableValue("shouldLookTarget", false);
                behaviorTree.SetVariableValue("IsAlerted", false);
                behaviorTree.SetVariableValue("timer", 0f);
                behaviorTree.SetVariableValue("light", false);
            }
        }
    }
}
