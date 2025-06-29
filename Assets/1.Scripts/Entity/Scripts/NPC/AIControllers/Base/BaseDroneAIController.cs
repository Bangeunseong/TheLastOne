using System.Collections;
using System.Collections.Generic;
using _1.Scripts.Entity.Scripts.NPC.AIControllers;
using UnityEngine;

namespace _1.Scripts.Entity.Scripts.NPC.AIControllers.Base
{
    public abstract class BaseDroneAIController : BaseNpcAI
    {
        [Header("Alert")]
        protected bool isAlerted = false;

        [Header("Timer")]
        protected Coroutine timerCoroutine = null;
        protected float timer = 0f;

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
            Debug.Log("Reset All");
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
    }
}
