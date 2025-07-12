using System;
using System.Collections;
using System.Text;
using _1.Scripts.Entity.Scripts.Player.Core;
using _1.Scripts.Manager.Core;
using _1.Scripts.Manager.Subs;
using _1.Scripts.UI.InGame;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;
using Console = _1.Scripts.Map.Console.Console;
using Random = UnityEngine.Random;

namespace _1.Scripts.MiniGame
{
    public class AlphabetMatching : MonoBehaviour
    {
        [field: Header("Game Settings")]
        [field: SerializeField] public int AlphabetLength { get; private set; } = 3;
        [field: SerializeField] public float Duration { get; private set; } = 5f;
        [field: SerializeField] public float Delay { get; private set; } = 3f;
        [field: SerializeField] public bool IsLoop { get; private set; }
        [field: SerializeField] public int LoopCount { get; private set; } = 2;
        
        [field: Header("Current Game State")]
        [field: SerializeField] public string CurrentAlphabets { get; private set; }
        [field: SerializeField] public int CurrentIndex { get; private set; }
        [field: SerializeField] public int CurrentLoopCount { get; private set; }
        [field: SerializeField] public bool IsPlaying { get; private set; }
        [field: SerializeField] public bool IsCounting { get; private set; }
        
        private MinigameUI ui;
        private Console console;
        private CoreManager coreManager;
        private UIManager uiManager;
        private Player player;
        private float startTime;
        private bool isFinished;

        private void OnEnable()
        {
            CurrentAlphabets = GetAlphabets();
            CurrentLoopCount = 0;
            IsPlaying = false;
            player.PlayerCondition.IsPlayerHasControl = false;
            player.Pov.m_HorizontalAxis.Reset();
            player.Pov.m_VerticalAxis.Reset();
            player.InputProvider.enabled = false;
            Cursor.lockState = CursorLockMode.None;
        }

        public void Initialize(Console con, Player player)
        {
            console = con;
            coreManager = CoreManager.Instance;
            uiManager = coreManager.uiManager;
            this.player = player;
        }

        private void Update()
        {
            if (coreManager.gameManager.IsGamePaused || isFinished) return;
            
            // Minigame 초입
            if (!IsPlaying)
            {
                if (Input.GetKeyDown(KeyCode.Return))
                {
                    ui.ShowEnterText(false);
                    _ = StartCountdown_Async();
                    IsCounting = IsPlaying = true;
                    return;
                }
                
                if (Input.GetKeyDown(KeyCode.Z))
                {
                    FinishGame(false);
                }
                return;
            }

            if (IsCounting) return;
            
            float elapsed = Time.time - startTime;
            float remaining = Mathf.Max(0, Duration - elapsed);
            ui.UpdateTimeSlider(remaining);
            
            // Minigame 달성 여부 확인
            if (CurrentIndex >= AlphabetLength)
            {
                CurrentLoopCount++;
                if (!IsLoop || CurrentLoopCount >= LoopCount)
                {
                    FinishGame(true); return;
                }
                ResetGame();
                return;
            }
            
            // Minigame 메인 로직
            if (Time.time - startTime >= Duration)
            {
                FinishGame(false); return;
            }
            if (!Input.anyKeyDown) return;
            if (Input.inputString == null) return;
            if (string.Compare(Input.inputString, CurrentAlphabets[CurrentIndex].ToString(),
                    StringComparison.OrdinalIgnoreCase) == 0)
            {
                ui.AlphabetAnim(CurrentIndex, true);
                CurrentIndex++;
            }
        }

        public void StartMiniGame(Console con, Player ply)
        {
            console = con;
            player = ply;

            ui = uiManager.ShowMinigameUI();
            ui.ShowPanel();
            enabled = true;
        }

        private void FinishGame(bool isSuccess)
        {
            Service.Log("Finished Game");
            isFinished = true;
            _ = EndGame_Async(isSuccess);
        }

        private void ResetGame()
        {
            IsPlaying = IsCounting = false;
            CurrentAlphabets = GetAlphabets();
            ui.ShowPanel();
            if (IsLoop && LoopCount > 0) 
                ui.UpdateLoopCount(CurrentLoopCount + 1, LoopCount);
        }

        private string GetAlphabets()
        {
            StringBuilder builder = new();
            for (var i = 0; i < AlphabetLength; i++) builder.Append($"{(char)Random.Range('a', 'z' + 1)}");
            return builder.ToString();
        }

        private async UniTask StartCountdown_Async()
        {
            ui.ShowCountdownText(true);
            ui.SetCountdownText(Delay);
            
            var t = 0f;
            while (t < Delay)
            {
                if (!coreManager.gameManager.IsGamePaused) t += Time.unscaledDeltaTime;
                ui.SetCountdownText(Delay - t);
                await UniTask.Yield(PlayerLoopTiming.Update);
            }
            
            ui.ShowCountdownText(false);
            ui.CreateAlphabet(CurrentAlphabets); ui.SetTimeSlider(Duration, Duration);
            ui.ShowAlphabet(true); ui.ShowTimeSlider(true);
            
            IsCounting = false; 
            startTime = Time.unscaledTime;
        }

        private async UniTask EndGame_Async(bool success)
        {
            if (success) { ui.ShowClearText(true); ui.SetClearText(true, "CLEAR!"); }
            
            ui.ShowAlphabet(false);
            await UniTask.WaitForSeconds(1.5f, true);
            
            CoreManager.Instance.uiManager.HideMinigameUI();
            player.PlayerCondition.IsPlayerHasControl = true;
            player.InputProvider.enabled = true;
            Cursor.lockState = CursorLockMode.Locked;
            ui = null;
            
            if (success) console.OnCleared();
        }
    }
}