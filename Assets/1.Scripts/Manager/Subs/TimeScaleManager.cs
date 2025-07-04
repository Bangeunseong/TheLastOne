using System;
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

        public void ChangeTimeScale(float target)
        {
            TargetTimeScale = target;
            Time.timeScale = target;
            Time.fixedDeltaTime = OriginalFixedDeltaTime * target;
            CurrentTimeScale = Time.timeScale;
        }
    }
}