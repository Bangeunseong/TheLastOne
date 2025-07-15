using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace _1.Scripts.UI.InGame.Mission
{
    public class MissionSlot : MonoBehaviour
    {
        public TextMeshProUGUI missionText;
        public Animator animator;
        [HideInInspector] public int questID;

        public void Initialize(int id, string text, bool isNew)
        {
            questID = id;
            missionText.text = text;
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
    }
}