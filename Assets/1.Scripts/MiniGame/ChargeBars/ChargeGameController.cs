using System.Collections.Generic;
using _1.Scripts.Entity.Scripts.Player.Core;
using _1.Scripts.Manager.Core;
using _1.Scripts.Manager.Subs;
using _1.Scripts.Map.Console;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace _1.Scripts.MiniGame.ChargeBars
{
    public class ChargeGameController : MonoBehaviour
    {
        [field: Header("Components")]
        [field: SerializeField] public GameObject BarPrefab { get; private set; }
        [field: SerializeField] public RectTransform BarLayout { get; private set; }
        [field: SerializeField] public RectTransform ControlLayout { get; private set; }
        [field: SerializeField] public RectTransform TargetObj { get; private set; }
        [field: SerializeField] public RectTransform ControlObj { get; private set; }
        
        [field: Header("Game Settings")]
        [field: Range(2, 5)][field: SerializeField] public int BarCount { get; private set; } = 3;
        [field: SerializeField] public float Duration { get; private set; } = 15f;
        [field: SerializeField] public float Delay { get; private set; } = 3f;
        [field: SerializeField] public float ChargeRate { get; private set; } = 0.25f;
        [field: SerializeField] public float LossRate { get; private set; } = 0.1f;
        
        [field: Header("Current Game State")]
        [field: SerializeField] public bool IsPlaying { get; private set; }
        [field: SerializeField] public bool IsCounting { get; private set; }
        [field: SerializeField] public bool IsCleared { get; private set; }
        [field: SerializeField] public int CurrentBarIndex { get; private set; }
        
        private List<Bar> bars;
        
        private Console console;
        private CoreManager coreManager;
        private UIManager uiManager;
        private Player player;
        private float startTime;
        private bool isFinished;
        
        private void OnEnable()
        {
            isFinished = IsPlaying = IsCounting = false;
            player.PlayerCondition.OnDisablePlayerMovement();
            Cursor.lockState = CursorLockMode.None;
        }
        
        private void Update()
        {
            if (isFinished) return;

            if (!IsPlaying)
            {
                if (Input.GetKeyDown(KeyCode.Return))
                {
                    _ = StartCountdown_Async(); 
                    IsCounting = IsPlaying = true; 
                    return;
                }
                if (Input.GetKeyDown(KeyCode.Z)) FinishGame(false, 0f);
                return;
            }
            
            if (IsCounting) return;
            
            bars[CurrentBarIndex].DecreaseValue(LossRate * Time.deltaTime);
            
            if (!(Time.time - startTime >= Duration)) return;
            FinishGame(false, 0f);
        }
        
        public void StartMiniGame(Console con, Player ply)
        {
            console = con;
            player = ply;
            coreManager = CoreManager.Instance;
            uiManager = coreManager.uiManager;
            
            // Initialize MiniGame
            enabled = true;
        }
        
        public void CancelMiniGame()
        {
            if (!isActiveAndEnabled || isFinished) return;
            isFinished = true;
            
            // Clear all remaining bars
            
            FinishGame(false, 0f);
        }

        public void OnBarFilled()
        {
            CurrentBarIndex++;
            if (CurrentBarIndex < bars.Count) return;
            FinishGame(true, 1.5f);
        }

        private void CreateBars()
        {
            bars = new List<Bar>{Capacity = BarCount};
            
            for (var i = 0; i < BarCount; i++)
            {
                var go = Instantiate(BarPrefab, BarLayout);
                if (!go.TryGetComponent(out Bar bar)) continue;
                bar.Initialize(this);
                bars.Add(bar);
            }
        }
        
        private void FinishGame(bool isSuccess, float duration)
        {
            // Service.Log("Finished Game");
            isFinished = true;
            _ = EndGame_Async(isSuccess, duration);
        }
        
        private async UniTask StartCountdown_Async()
        {
            var t = 0f;
            while (t < Delay)
            {
                if (!coreManager.gameManager.IsGamePaused) t += Time.unscaledDeltaTime;
                await UniTask.Yield(PlayerLoopTiming.Update);
            }
            
            CreateBars();
            IsCounting = false; 
            startTime = Time.unscaledTime;
        }

        private async UniTask EndGame_Async(bool success, float duration)
        {
            if (success)
            {
                // TODO: Show Clear UI
                Service.Log("Cleared MiniGame!");
            } else Service.Log("Better Luck NextTime");
            
            await UniTask.WaitForSeconds(duration, true);
            
            console.OnCleared(success);
            Cursor.lockState = CursorLockMode.Locked; 
            Cursor.visible = false;
            enabled = false;
        }
    }
}