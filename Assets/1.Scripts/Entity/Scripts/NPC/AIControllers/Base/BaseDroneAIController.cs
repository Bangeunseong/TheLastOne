using System.Collections;
using System.Collections.Generic;
using _1.Scripts.Entity.Scripts.NPC.AIControllers;
using _1.Scripts.Entity.Scripts.NPC.BehaviorTree;
using _1.Scripts.Entity.Scripts.NPC.Data.AnimationHashData;
using _1.Scripts.Interfaces.NPC;
using _1.Scripts.Manager.Core;
using _1.Scripts.Manager.Subs;
using _1.Scripts.Static;
using UnityEngine;

namespace _1.Scripts.Entity.Scripts.NPC.AIControllers.Base
{
    public abstract class BaseDroneAIController : BaseNpcAI, IStunnable
    {
        [Header("Alert")]
        protected bool isAlerted = false;

        [Header("Timer")]
        protected Coroutine timerCoroutine = null;
        protected Coroutine stunnedCoroutine = null;
        protected float timer = 0f;

        [Header("Particle")]
        [SerializeField] protected ParticleSystem p_hit;
        
        [Header("Stunned")]
        protected bool isStunned;
        
        [Header("Light")]
        public Light alertLight;
        
        public bool IsAlertedCheck() => isAlerted;

        public float TimerCheck() => timer;
        
        public bool CheckStunned() => isStunned;

        public virtual void SetAlert(bool alert)
        {
            isAlerted = alert;
        }

        public virtual void TimerStartIfNull()
        {
            timerCoroutine ??= StartCoroutine(TimerStart());
        }

        public virtual void ResetAll()
        {
            targetPos = Vector3.zero;
            targetTransform = null;
            SetAlert(false);
            if (timerCoroutine != null)
            {
                StopCoroutine(timerCoroutine);
                timerCoroutine = null;
            }
            ResetTimer();
        }

        public virtual void ResetTimer()
        {
            timer = 0f;
        }
        
        private IEnumerator TimerStart()
        {
            while (true)
            {
                timer += Time.deltaTime;
                yield return null;
            }
        }

        public void AlertNearbyDrones()
        {
            if (!statController.TryGetRuntimeStatInterface<IAlertable>(out var alertable)) // 있을 시 변환
            {
                return;
            }

            alertLight.enabled = true; // 경고등 On
            CoreManager.Instance.soundManager.PlaySFX(SfxType.Drone, MyPos, 1); // 사운드 출력
            SetAlert(true); // 알람 활성화

            bool isAlly = statController.RuntimeStatData.isAlly; 
            Vector3 selfPos = transform.position;
            float range = alertable.AlertRadius;

            int layerMask = isAlly ? 1 << LayerConstants.Ally : 1 << LayerConstants.Enemy;
            Collider[] colliders = Physics.OverlapSphere(selfPos, range, layerMask); // 주변 콜라이더 모음

            foreach (var collider in colliders)
            {
                if (collider.TryGetComponent(out BaseDroneAIController drone))
                {
                    drone.targetTransform = targetTransform;
                    drone.targetPos = targetPos;
                    drone.SetAlert(true);
                }
            }
        }

        public override void HackingNpc()
        { 
            ResetAll();
            base.HackingNpc();
        }
        
        public void OnStunned(float duration)
        {
            isStunned = true;
            IsCurrentActionRunning(true);
            ResetAll();
            if (stunnedCoroutine != null)
            {
                StopCoroutine(stunnedCoroutine);
            }
            stunnedCoroutine = StartCoroutine(Stunned(duration));
        }

        private IEnumerator Stunned(float duration)
        {
            p_hit.Play();
            yield return new WaitForSeconds(duration); // 원하는 시간만큼 유지

            // main.duration = tempDuration;
            if (p_hit != null && p_hit.IsAlive())
            {
                p_hit.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            }

            isStunned = false;
            IsCurrentActionRunning(false);
            stunnedCoroutine = null;
        }
    }
}
