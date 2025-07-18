using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace _1.Scripts.UI.InGame.Mission
{
    public class MissionSlot : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI missionText;
        [SerializeField] private TextMeshProUGUI missionCountText;
        [SerializeField] private Slider missionCountSlider;
        [SerializeField] private Animator animator;
        [HideInInspector] public int questID;

        
        public void Initialize(int id, string text, int currentAmount, int requiredAmount)
        {
            questID = id;
            missionText.text = text;
            UpdateProgress(currentAmount, requiredAmount, immediate: true);
        }

        public void PlayCompleteAnimation()
        {
            ResetAnimation();
            animator.SetTrigger("Complete");
        }

        public void PlayNewMissionAnimation()
        {
            ResetAnimation();
            animator.SetTrigger("NewMission");
        }

        private void ResetAnimation()
        {
            animator.ResetTrigger("Complete");
            animator.ResetTrigger("NewMission");
        }

        public void UpdateProgress(int currentAmount, int requiredAmount, bool immediate = false)
        {
            missionCountText.text = $"{currentAmount} / {requiredAmount}";
            float targetValue = Mathf.Clamp01(currentAmount / (float)requiredAmount);
            
            if (immediate)
            {
                missionCountSlider.value = targetValue;
            }
            else
            {
                missionCountSlider.value = Mathf.Lerp(missionCountSlider.value, targetValue, 0.1f);
            }
        }

        public IEnumerator WaitCompleteAnimation()
        {
            while (missionCountSlider.value < 0.99f)
            {
                missionCountSlider.value = Mathf.Lerp(missionCountSlider.value, 1f, 0.1f);
                yield return null;
            }
            
            missionCountSlider.value = 1f;
            missionCountText.text = "COMPLETE!";
        }
    }
}