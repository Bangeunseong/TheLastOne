using System.Collections;
using System.Collections.Generic;
using _1.Scripts.Manager.Core;
using _1.Scripts.Quests.Core;
using UnityEngine;

namespace _1.Scripts.Dialogue
{
    public class DialogueOnFirstKill : MonoBehaviour, IGameEventListener
    {
        [SerializeField] private string dialogueKey;
        private bool hasTriggered = false;

        private void OnEnable()
        {
            GameEventSystem.Instance.RegisterListener(this);
        }

        private void OnDisable()
        {
            GameEventSystem.Instance.UnregisterListener(this);
        }

        public void OnEventRaised(int eventID)
        {
            if (hasTriggered) return;

            hasTriggered = true;
            CoreManager.Instance.dialogueManager.TriggerDialogue(dialogueKey);
        }
    }
}