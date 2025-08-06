using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using DG.Tweening;

namespace _1.Scripts.VisualEffects
{
    public class PostProcessColorAdjustmentsEdit : MonoBehaviour
    {
        [SerializeField] private Volume volume;
        [SerializeField] private float darkExposure = -1.3f;
        [SerializeField] private float transitionTime = 1f;

        private ColorAdjustments colorAdjustments;

        private void Awake()
        {
            if (volume.profile.TryGet(out colorAdjustments))
            {
                colorAdjustments.postExposure.overrideState = true;
            }
        }

        /// <summary>
        /// Focus모드 진입 또는 퇴장
        /// </summary>
        /// <param name="isOn"></param>
        public void FocusModeOnOrNot(bool isOn)
        {
            float value = isOn ? darkExposure : 0f;
            
            DOTween.To(
                () => colorAdjustments.postExposure.value,
                x => colorAdjustments.postExposure.value = x,
                value,
                transitionTime
            );
        }
    }
}
