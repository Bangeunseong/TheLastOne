using System;
using System.Collections.Generic;
using _1.Scripts.Entity.Scripts.Player.Core;
using _1.Scripts.Interfaces.Player;
using _1.Scripts.Manager.Core;
using _1.Scripts.Manager.Subs;
using _1.Scripts.Map.Doors;
using _1.Scripts.MiniGame;
using UnityEngine;
using UnityEngine.Playables;

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

        [field: Header("CutScene")]
        [field: SerializeField] public PlayableDirector CutScene { get; private set; }
        
        [SerializeField] private bool shouldChangeBGM = false;
        [SerializeField] private int indexOfBGM;
        
        private CoreManager coreManager;
        
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

        private void Start()
        {
            // TODO: Get Cleared Info. from DTO
            coreManager = CoreManager.Instance;
            // IsCleared = coreManager.gameManager.SaveData...

            foreach(var door in Doors) door.Initialize(IsCleared);
        }

        public void OnCleared(bool success)
        {
            if (success)
            {
                IsCleared = true; 
                // TODO: Save cleared info. to DTO
                OnClear();
            } else coreManager.gameManager.Player.PlayerCondition.OnEnablePlayerMovement();
        }

        private void OnClear()
        {
            if (!CutScene)
            {
                coreManager.gameManager.Player.PlayerCondition.OnEnablePlayerMovement();
                foreach (var door in Doors) door.OpenDoor();
            }
            else CutScene.Play();

            if (shouldChangeBGM)
            {
                if (Enum.TryParse(coreManager.sceneLoadManager.CurrentScene.ToString(), out BgmType bgmType))
                {
                    coreManager.soundManager.PlayBGM(bgmType, index:indexOfBGM);
                }
            }
        }

        public void OnInteract(GameObject ownerObj)
        {
            if (!ownerObj.TryGetComponent(out Player player)) return;
            Service.Log("Interacted!");
            if (IsCleared) return;
            AlphabetGame.StartMiniGame(this, player);
        }
    }
}