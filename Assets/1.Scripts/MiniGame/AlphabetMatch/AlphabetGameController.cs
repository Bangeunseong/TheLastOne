using System;
using System.Text;
using _1.Scripts.Entity.Scripts.Player.Core;
using _1.Scripts.Manager.Core;
using _1.Scripts.Manager.Subs;
using _1.Scripts.UI.InGame;
using _1.Scripts.UI.InGame.Minigame;
using Cysharp.Threading.Tasks;
using UnityEngine;
using Console = _1.Scripts.Map.Console.Console;
using Random = UnityEngine.Random;

namespace _1.Scripts.MiniGame.AlphabetMatch
{
    public class AlphabetGameController : BaseMiniGame
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

        private AlphabetMatchingUI alphabetUI;
        
        protected override void OnEnable()
        {
            CurrentAlphabets = GetAlphabets();
            CurrentLoopCount = 0;
            base.OnEnable();
        }
        
        protected override void Update()
        {
            if (coreManager.gameManager.IsGamePaused || isFinished) return;
            
            // Minigame 초입
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
            
            float elapsed = Time.unscaledTime - startTime;
            float remaining = Mathf.Max(0, Duration - elapsed);
            coreManager.uiManager.GetUI<MinigameUI>().UpdateTimeSlider(remaining);
            
            // Minigame 달성 여부 확인
            if (CurrentIndex >= AlphabetLength)
            {
                CurrentLoopCount++;
                if (!IsLoop || CurrentLoopCount >= LoopCount)
                {
                    FinishGame(true, 1.5f); return;
                }
                ResetGame();
                return;
            }
            
            // Minigame 메인 로직
            if (Time.unscaledTime - startTime >= Duration)
            {
                FinishGame(false, 1.5f); return;
            }
            if (!Input.anyKeyDown) return;
            if (Input.inputString == null) return;
            if (string.Compare(Input.inputString, CurrentAlphabets[CurrentIndex].ToString(),
                    StringComparison.OrdinalIgnoreCase) == 0)
            {
                alphabetUI.AlphabetAnim(CurrentIndex, true);
                CurrentIndex++;
            }
        }

        public override void StartMiniGame(Console con, Player ply)
        {
            base.StartMiniGame(con, ply);

            uiManager.ShowUI<MinigameUI>().ShowPanel();
            uiManager.GetUI<MinigameUI>().ShowEnterText(true);
            uiManager.GetUI<MinigameUI>().ShowClearText(false);
            uiManager.GetUI<MinigameUI>().ShowTimeSlider(false);
            uiManager.GetUI<MinigameUI>().ShowCountdownText(false);
            uiManager.GetUI<MinigameUI>().ShowLoopText(IsLoop);
            uiManager.GetUI<MinigameUI>().ShowAlphabetMatching(true);
            
            alphabetUI = uiManager.GetUI<MinigameUI>().GetAlphabetMatchingUI();
            alphabetUI.ResetUI();
            enabled = true;
        }

        public override void CancelMiniGame()
        {
            base.CancelMiniGame();
            FinishGame(false, 0f);
        }
        
        private void ResetGame()
        {
            IsPlaying = IsCounting = false;
            CurrentAlphabets = GetAlphabets();
            uiManager.ShowUI<MinigameUI>().ShowPanel();
            uiManager.GetUI<MinigameUI>().ShowLoopText(IsLoop);
            if (IsLoop && LoopCount > 0) 
                uiManager.GetUI<MinigameUI>().UpdateLoopCount(CurrentLoopCount + 1, LoopCount);
            alphabetUI.CreateAlphabet(CurrentAlphabets);
            alphabetUI.ShowAlphabet(true);
            CurrentIndex = 0;
        }

        private string GetAlphabets()
        {
            StringBuilder builder = new();
            for (var i = 0; i < AlphabetLength; i++) builder.Append($"{(char)Random.Range('A', 'Z' + 1)}");
            return builder.ToString();
        }

        protected override async UniTask StartCountdown_Async()
        {
            uiManager.GetUI<MinigameUI>().ShowCountdownText(true);
            uiManager.GetUI<MinigameUI>().SetCountdownText(Delay);
            uiManager.GetUI<MinigameUI>().ShowTimeSlider(false);
            uiManager.GetUI<MinigameUI>().ShowEnterText(false);
            uiManager.GetUI<MinigameUI>().ShowClearText(false);
            uiManager.GetUI<MinigameUI>().ShowLoopText(IsLoop);
            uiManager.GetUI<MinigameUI>().ShowAlphabetMatching(true);
            
            var t = 0f;
            while (t < Delay)
            {
                if (!coreManager.gameManager.IsGamePaused) t += Time.unscaledDeltaTime;
                uiManager.GetUI<MinigameUI>().SetCountdownText(Delay - t);
                await UniTask.Yield(PlayerLoopTiming.Update);
            }
            
            uiManager.GetUI<MinigameUI>().ShowCountdownText(false);
            
            alphabetUI.CreateAlphabet(CurrentAlphabets);
            alphabetUI.ShowAlphabet(true);
            uiManager.GetUI<MinigameUI>().ShowTimeSlider(true);
            uiManager.GetUI<MinigameUI>().SetTimeSlider(Duration, Duration);
            if (IsLoop && LoopCount > 0)
                uiManager.GetUI<MinigameUI>().UpdateLoopCount(CurrentLoopCount + 1, LoopCount);
            
            CurrentIndex = 0;
            IsCounting = false; 
            startTime = Time.unscaledTime;
        }

        protected override async UniTask EndGame_Async(bool success, float duration)
        {
            uiManager.GetUI<MinigameUI>().ShowClearText(true);
            uiManager.GetUI<MinigameUI>().SetClearText(success, success ? "CLEAR!" : "FAIL");
            
            alphabetUI.ShowAlphabet(false);
            uiManager.GetUI<MinigameUI>().ShowTimeSlider(false);
            uiManager.GetUI<MinigameUI>().ShowLoopText(false);
            uiManager.GetUI<MinigameUI>().ShowEnterText(false);
            await UniTask.WaitForSeconds(duration, true);
            uiManager.HideUI<MinigameUI>();
            uiManager.UnloadUI<MinigameUI>();
            alphabetUI = null;
            
            Cursor.lockState = CursorLockMode.Locked;
            console.OnCleared(success);
            enabled = false;
        }
    }
}