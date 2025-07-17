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

        private MinigameUI panelUI;
        private AlphabetMatchingUI ui;
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
            isFinished = false;
            IsPlaying = false;
            player.PlayerCondition.OnDisablePlayerMovement();
            Cursor.lockState = CursorLockMode.None;
        }

        public void StartMiniGame(Console con, Player ply)
        {
            console = con;
            player = ply;
            coreManager = CoreManager.Instance;
            uiManager = coreManager.uiManager;

            panelUI = uiManager.GetUI<MinigameUI>();
            ui = panelUI.GetAlphabetMatchingUI();
            enabled = true;
        }

        private void Update()
        {
            if (coreManager.gameManager.IsGamePaused || isFinished) return;
            
            // Minigame 초입
            if (!IsPlaying)
            {
                if (Input.GetKeyDown(KeyCode.Return))
                {
                    panelUI.ShowEnterText(false);
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
            panelUI.UpdateTimeSlider(remaining);
            
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
            panelUI.ShowPanel();
            if (IsLoop && LoopCount > 0) 
                panelUI.UpdateLoopCount(CurrentLoopCount + 1, LoopCount);
        }

        private string GetAlphabets()
        {
            StringBuilder builder = new();
            for (var i = 0; i < AlphabetLength; i++) builder.Append($"{(char)Random.Range('A', 'Z' + 1)}");
            return builder.ToString();
        }

        private async UniTask StartCountdown_Async()
        {
            panelUI.ShowCountdownText(true);
            panelUI.SetCountdownText(Delay);
            
            var t = 0f;
            while (t < Delay)
            {
                if (!coreManager.gameManager.IsGamePaused) t += Time.unscaledDeltaTime;
                panelUI.SetCountdownText(Delay - t);
                await UniTask.Yield(PlayerLoopTiming.Update);
            }
            
            panelUI.ShowCountdownText(false);
            ui.CreateAlphabet(CurrentAlphabets); panelUI.SetTimeSlider(Duration, Duration);
            ui.ShowAlphabet(true); panelUI.ShowTimeSlider(true);
            
            IsCounting = false; 
            startTime = Time.unscaledTime;
        }

        private async UniTask EndGame_Async(bool success)
        {
            if (success) { panelUI.ShowClearText(true); panelUI.SetClearText(true, "CLEAR!"); }
            
            ui.ShowAlphabet(false);
            await UniTask.WaitForSeconds(1.5f, true);
            
            CoreManager.Instance.uiManager.HideUI<MinigameUI>();
            Cursor.lockState = CursorLockMode.Locked;
            ui = null;
            
            console.OnCleared(success);
            enabled = false;
        }
    }
}