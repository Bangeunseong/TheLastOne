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
        public TextMeshProUGUI missionText;
        public TextMeshProUGUI missionCountText;
        public Slider missionCountSlider;
        public Animator animator;
        [HideInInspector] public int questID;

        public void Initialize(int id, string text, int currentAmount, int requiredAmount)
        {
            questID = id;
            missionText.text = text;
            missionCountText.text = $"{currentAmount}/{requiredAmount}";
            missionCountSlider.value = currentAmount / (float) requiredAmount;
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
        
        public void UpdateProgress(int currentAmount, int requiredAmount)
        {
            missionCountText.text = $"{currentAmount} / {requiredAmount}";
            if (currentAmount >= requiredAmount) missionCountSlider.value = 1;
            else missionCountSlider.value = Mathf.Lerp(missionCountSlider.value, currentAmount / (float) requiredAmount, 0.1f);
        }
    }
}