using System.Collections;
using System.Collections.Generic;
using _1.Scripts.Entity.Scripts.NPC.AIControllers;
using _1.Scripts.Entity.Scripts.NPC.Data.AnimationHashData;
using _1.Scripts.Interfaces.NPC;
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
        
        public bool IsAlertedCheck() => isAlerted;

        public float TimerCheck() => timer;
        
        public virtual void SetAlert(bool alert)
        {
            isAlerted = alert;
        }

        protected override void Update()
        {
            base.Update();
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

        public override void HackingNpc()
        { 
            ResetAll();
            base.HackingNpc();
        }
        
        public void OnStunned(float duration)
        {
            IsCurrentActionRunning(true);
            ResetAll();
            if (stunnedCoroutine != null)
            {
                StopCoroutine(stunnedCoroutine);
            }
            animator.SetTrigger(DroneAnimationHashData.Hit2);
            stunnedCoroutine = StartCoroutine(Stunned(duration));
        }

        private IEnumerator Stunned(float duration)
        {
            if (p_hit.IsAlive())
                p_hit.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            
            var main = p_hit.main;
            float tempDuration = main.duration;
            main.duration = duration;
            
            p_hit.Play();
            yield return new WaitForSeconds(duration); // 원하는 시간만큼 유지

            // main.duration = tempDuration;
            if (p_hit != null && p_hit.IsAlive())
            {
                p_hit.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            }

            IsCurrentActionRunning(false);
            stunnedCoroutine = null;
        }
    }
}
