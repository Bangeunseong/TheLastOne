using System;
using System.Collections;
using System.Collections.Generic;
using _1.Scripts.Entity.Scripts.NPC.AIBehaviors;
using _1.Scripts.Entity.Scripts.NPC.AIBehaviors.Drone.ReconDrone;
using _1.Scripts.Entity.Scripts.NPC.AIControllers;
using _1.Scripts.Entity.Scripts.NPC.BehaviorTree;
using _1.Scripts.Entity.Scripts.NPC.Data;
using _1.Scripts.Manager.Core;
using UnityEngine;

namespace _1.Scripts.Entity.Scripts.NPC.AIControllers.Enemy
{
    /// <summary>
    /// 드론(Suicide Drone) 
    /// </summary>
    public class EnemySuicideDroneAIController : BaseNpcAI
    {
        [Header("Alert")]
        private bool isAlerted = false;

        [Header("Timer")] 
        private Coroutine timerCoroutine = null;
        private float timer = 0f;

        protected override void Start()
        {
            // statData = CoreManager.Instance.resourceManager.GetAsset<ReconDroneData>("ReconDroneData");
            // 액션노드 추가해서 등장연출 가능
            base.Start();
        }
        
        protected override void BuildTree()
        {
        }
        
        public bool IsAlertedCheck()
        {
            return isAlerted;
        }
        
        public void SetAlert(bool alert)
        {
            isAlerted = alert;
        }

        public void TimerStartIfNull()
        {
            timerCoroutine ??= StartCoroutine(TimerStart()); // null이면 코루틴 실행
        }

        public void ResetAll()
        {
            SetAlert(false);
            ResetTimer();
            if (timerCoroutine != null)
            {
                StopCoroutine(timerCoroutine);
                timerCoroutine = null;
            }
        }
        
        public void ResetTimer()
        {
            timer = 0f;
        }
        
        public float TimerCheck()
        {
            return timer;
        }

        private IEnumerator TimerStart()
        {
            timer += Time.deltaTime;
            yield return null;
        }
    }
}