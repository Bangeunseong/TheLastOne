using System;
using System.Collections;
using _1.Scripts.Manager.Core;
using UnityEngine;

namespace _1.Scripts.Manager.Subs
{
    [Serializable] public class TimeScaleManager
    {
        [field: Header("TimeScale Values")]
        [field: SerializeField] public float OriginalFixedDeltaTime { get; private set; }
        [field: SerializeField] public float CurrentTimeScale { get; private set; } = 1f;
        [field: SerializeField] public float TargetTimeScale { get; private set; } = 1f;
        
        private Coroutine timeScaleCoroutine;
        private CoreManager coreManager;
        
        public void Start()
        {
            coreManager = CoreManager.Instance;
            CurrentTimeScale = Time.timeScale;
            OriginalFixedDeltaTime = Time.fixedDeltaTime;
        }

        public void Update()
        {
            CurrentTimeScale = Time.timeScale;
        }

        public void Reset()
        {
            if (timeScaleCoroutine != null) 
            {
                coreManager.StopCoroutine(timeScaleCoroutine);
                timeScaleCoroutine = null;
            }
            
            Time.timeScale = 1f;
            Time.fixedDeltaTime = OriginalFixedDeltaTime;
            CurrentTimeScale = 1f;
            TargetTimeScale = 1f;
        }

        public void ChangeTimeScale(float target, float duration)
        {
            if (timeScaleCoroutine != null){ coreManager.StopCoroutine(timeScaleCoroutine); }
            TargetTimeScale = target;
            timeScaleCoroutine = coreManager.StartCoroutine(ChangeTimeScale_Coroutine(duration));
        }

        private IEnumerator ChangeTimeScale_Coroutine(float duration)
        {
            float startScale = Time.timeScale;
            float t = 0f;

            while (t < duration)
            {
                t += Time.unscaledDeltaTime;
                float elapsed = Mathf.Clamp01(t / duration);
                Time.timeScale = Mathf.Lerp(startScale, TargetTimeScale, elapsed);
                Time.fixedDeltaTime = OriginalFixedDeltaTime * Time.timeScale;
                yield return null;
            }
            
            Time.timeScale = TargetTimeScale;
            Time.fixedDeltaTime = TargetTimeScale * OriginalFixedDeltaTime;
            timeScaleCoroutine = null;
        }
    }
}