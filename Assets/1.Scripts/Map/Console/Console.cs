using System;
using System.Collections.Generic;
using _1.Scripts.Entity.Scripts.Player.Core;
using _1.Scripts.Interfaces.Player;
using _1.Scripts.Manager.Core;
using _1.Scripts.Manager.Subs;
using _1.Scripts.Map.Doors;
using _1.Scripts.MiniGame;
using _1.Scripts.MiniGame.AlphabetMatch;
using _1.Scripts.MiniGame.WireConnection;
using _1.Scripts.Quests.Core;
using UnityEngine;
using UnityEngine.Playables;
using Random = System.Random;

namespace _1.Scripts.Map.Console
{
    public class Console : MonoBehaviour, IInteractable
    {
        [field: Header("Console Settings")]
        [field: SerializeField] public int Id { get; private set; }
        [field: SerializeField] public bool IsCleared { get; private set; }
        [field: SerializeField] public List<ConsoleDoor> Doors { get; private set; }

        [field: Header("Minigames")]
        [field: SerializeField] public List<BaseMiniGame> MiniGames { get; private set; }
        [field: SerializeField] public int CurrentMiniGame { get; private set; }
        
        [field: Header("CutScene")]
        [field: SerializeField] public PlayableDirector CutScene { get; private set; }
        
        [SerializeField] private bool shouldChangeBGM = false;
        [SerializeField] private int indexOfBGM;
        
        private CoreManager coreManager;
        
        private void Awake()
        {
            if (MiniGames.Count <= 0) MiniGames = new List<BaseMiniGame>(GetComponentsInChildren<BaseMiniGame>());
            if (Doors.Count <= 0) Doors = new List<ConsoleDoor>(GetComponentsInChildren<ConsoleDoor>());
        }

        private void Reset()
        {
            if (MiniGames.Count <= 0) MiniGames = new List<BaseMiniGame>(GetComponentsInChildren<BaseMiniGame>());
            if (Doors.Count <= 0) Doors = new List<ConsoleDoor>(GetComponentsInChildren<ConsoleDoor>());
        }

        private void Start()
        {
            coreManager = CoreManager.Instance;
            foreach(var door in Doors) door.Initialize(IsCleared);
        }

        public void OpenDoors()
        {
            IsCleared = true;
            foreach(var door in Doors) door.Initialize(true);
        }

        public void OnCleared(bool success)
        {
            if (success)
            {
                IsCleared = true; 
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
            else
            {
                CutScene.played += coreManager.uiManager.OnCutsceneStarted;
                CutScene.stopped += coreManager.uiManager.OnCutsceneStopped;
                CutScene.Play();
            }
            GameEventSystem.Instance.RaiseEvent(Id);

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
            CurrentMiniGame = UnityEngine.Random.Range(0, MiniGames.Count);
            CurrentMiniGame = 2;
            MiniGames[CurrentMiniGame].StartMiniGame(this, player);
        }

        public void OnCancelInteract()
        {
            MiniGames[CurrentMiniGame].CancelMiniGame();
        }
    }
}