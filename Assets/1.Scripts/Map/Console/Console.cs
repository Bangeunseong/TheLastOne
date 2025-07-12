using System;
using System.Collections.Generic;
using _1.Scripts.Entity.Scripts.Player.Core;
using _1.Scripts.Interfaces.Player;
using _1.Scripts.Manager.Core;
using _1.Scripts.Map.Doors;
using _1.Scripts.MiniGame;
using Cinemachine;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace _1.Scripts.Map.Console
{
    public class Console : MonoBehaviour, IInteractable
    {
        [field: Header("Console Settings")]
        [field: SerializeField] public int Id { get; private set; }
        [field: SerializeField] public bool IsCleared { get; private set; }
        [field: SerializeField] public List<ConsoleDoor> Doors { get; private set; }
        [field: SerializeField] public List<Light> Indicators { get; private set; }
        
        [field: Header("Minigames")]
        [field: SerializeField] public AlphabetMatching AlphabetGame { get; private set; }

        private CoreManager coreManager;
        
        private void Awake()
        {
            if (!AlphabetGame) AlphabetGame = this.TryGetComponent<AlphabetMatching>();
            if (Indicators.Count <= 0) Indicators = new List<Light>(GetComponentsInChildren<Light>()); 
            if (Doors.Count <= 0) Doors = new List<ConsoleDoor>(GetComponentsInChildren<ConsoleDoor>());
        }

        private void Reset()
        {
            if (!AlphabetGame) AlphabetGame = this.TryGetComponent<AlphabetMatching>();
            if (Indicators.Count <= 0) Indicators = new List<Light>(GetComponentsInChildren<Light>()); 
            if (Doors.Count <= 0) Doors = new List<ConsoleDoor>(GetComponentsInChildren<ConsoleDoor>());
        }

        private void Start()
        {
            // TODO: Get Cleared Info. from DTO
            coreManager = CoreManager.Instance;
            // IsCleared = coreManager.gameManager.SaveData...

            foreach(var door in Doors) door.Initialize(IsCleared);
        }

        public void OnCleared()
        {
            IsCleared = true;
            // TODO: Save cleared info. to DTO
            OnClear();
            // else _ = OnClear_Async();
        }

        private void OnClear()
        {
            foreach (var indicator in Indicators) indicator.color = Color.green;
            foreach (var door in Doors) door.OpenDoor();
        }

        // private async UniTaskVoid OnClear_Async()
        // {
        //     
        // }

        public void OnInteract(GameObject ownerObj)
        {
            if (!ownerObj.TryGetComponent(out Player player)) return;
            Service.Log("Interacted!");
            if (IsCleared) return;
            AlphabetGame.Initialize(this, player);
            AlphabetGame.enabled = true;
            AlphabetGame.StartMiniGame(this, player);
        }
    }
}