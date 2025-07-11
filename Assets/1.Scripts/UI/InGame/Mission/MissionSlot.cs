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
        public GameObject newMission;

        public void SetMission(string text, bool isNew)
        {
            missionText.text = text;
            if (newMission != null) newMission.SetActive(isNew);
        }
        
        public void PlayCompleteAnimation()
        {
            animator.SetTrigger("Complete");
        }

        public void PlayNewMissionAnimation()
        {
            animator.SetTrigger("NewMission");
        }
    }
}