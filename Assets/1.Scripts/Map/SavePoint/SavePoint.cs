using System;
using _1.Scripts.Manager.Core;
using _1.Scripts.Quests.Core;
using UnityEngine;

namespace _1.Scripts.Map.SavePoint
{
    public class SavePoint : MonoBehaviour, IGameEventListener
    {
        [field: Header("Save Point Id")]
        [field: SerializeField] public int Id { get; private set; }

        private void OnEnable()
        {
            GameEventSystem.Instance.RegisterListener(this);
        }

        private void OnDisable()
        {
            GameEventSystem.Instance.UnregisterListener(this);
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Player"))
            {
                GameEventSystem.Instance.RaiseEvent(Id);
            }
        }

        public void OnEventRaised(int eventID)
        {
            CoreManager.Instance.SaveData_QueuedAsync();
            enabled = false;
        }
    }
}
