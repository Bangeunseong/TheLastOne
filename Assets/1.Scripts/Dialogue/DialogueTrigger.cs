using _1.Scripts.Manager.Core;
using UnityEngine;

namespace _1.Scripts.Dialogue
{
    public class DialogueTrigger : MonoBehaviour
    {
        [SerializeField] private string dialogueKey;
        [SerializeField] private bool triggerOnce = true;

        private bool hasTriggered = false;

        private void OnTriggerEnter(Collider other)
        {
            if (hasTriggered && triggerOnce) return;
            if (!other.CompareTag("Player")) return;

            hasTriggered = true;
            CoreManager.Instance.dialogueManager.TriggerDialogue(dialogueKey);
        }
    }
}