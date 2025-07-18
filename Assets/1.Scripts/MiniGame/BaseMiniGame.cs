using System;
using _1.Scripts.Entity.Scripts.Player.Core;
using _1.Scripts.Manager.Core;
using _1.Scripts.Manager.Subs;
using Cysharp.Threading.Tasks;
using UnityEngine;
using Console = _1.Scripts.Map.Console.Console;

namespace _1.Scripts.MiniGame
{
    public abstract class BaseMiniGame : MonoBehaviour
    {
        public bool IsPlaying { get; protected set; }
        public bool IsCounting { get; protected set; }
        public bool IsCleared { get; protected set; }
        
        protected Console console;
        protected CoreManager coreManager;
        protected UIManager uiManager;
        protected Player player;
        protected float startTime;
        protected bool isFinished;

        protected virtual void Awake() { }
        protected virtual void Reset() { }
        protected virtual void Start() { }
        protected virtual void Update() { }
        
        protected virtual void OnEnable()
        {
            isFinished = IsPlaying = IsCounting = false;
            player?.PlayerCondition.OnDisablePlayerMovement();
            Cursor.lockState = CursorLockMode.None;
        }
        
        protected virtual void OnDisable() { }
        
        public virtual void StartMiniGame(Console con, Player ply)
        {
            console = con;
            player = ply;
            coreManager = CoreManager.Instance;
            uiManager = coreManager.uiManager;
        }

        public virtual void CancelMiniGame()
        {
            if (!isActiveAndEnabled || isFinished) return;
            isFinished = true;
        }
        
        protected void FinishGame(bool isSuccess, float duration)
        {
            // Service.Log("Finished Game");
            isFinished = true;
            _ = EndGame_Async(isSuccess, duration);
        }
        
        protected virtual async UniTask StartCountdown_Async() { }
        protected virtual async UniTask EndGame_Async(bool success, float duration) { }
    }
}