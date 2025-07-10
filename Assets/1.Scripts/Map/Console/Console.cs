using System.Collections.Generic;
using _1.Scripts.Entity.Scripts.Player.Core;
using _1.Scripts.Interfaces.Player;
using _1.Scripts.Map.Doors;
using _1.Scripts.MiniGame;
using UnityEngine;

namespace _1.Scripts.Map.Console
{
    public class Console : MonoBehaviour, IInteractable
    {
        [field: Header("Console Settings")]
        [field: SerializeField] public int Id { get; private set; }
        [field: SerializeField] public bool IsCleared { get; private set; }
        [field: SerializeField] public List<ConsoleDoor> Doors { get; private set; }
        
        [field: Header("Minigames")]
        [field: SerializeField] public AlphabetMatching AlphabetGame { get; private set; }

        private void Awake()
        {
            if (!AlphabetGame) AlphabetGame = this.TryGetComponent<AlphabetMatching>();
            if (Doors.Count <= 0) Doors = new List<ConsoleDoor>(GetComponentsInChildren<ConsoleDoor>());
        }

        private void Reset()
        {
            if (!AlphabetGame) AlphabetGame = this.TryGetComponent<AlphabetMatching>();
            if (Doors.Count <= 0) Doors = new List<ConsoleDoor>(GetComponentsInChildren<ConsoleDoor>());
        }

        public void OnCleared()
        {
            IsCleared = true;
            foreach (var door in Doors) door.OpenDoor();
            // TODO: Save cleared info. to DTO
        }

        public void OnInteract(GameObject ownerObj)
        {
            if (!ownerObj.TryGetComponent(out Player player)) return;
            Service.Log("Interacted!");
            if (IsCleared) return;
            AlphabetGame.Initialize(this, player);
            AlphabetGame.enabled = true;
        }
    }
}