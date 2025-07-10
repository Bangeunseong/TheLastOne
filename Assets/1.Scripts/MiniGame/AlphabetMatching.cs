using System;
using System.Collections;
using System.Text;
using _1.Scripts.Entity.Scripts.Player.Core;
using _1.Scripts.Manager.Core;
using _1.Scripts.UI.InGame;
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
        
        
        [field: Header("UI")]
        [field: SerializeField] private MinigameUI ui;
        
        public event Action OnSuccess;

        private Console console;
        private Player player;
        private float startTime;

        private void OnEnable()
        {
            if (ui == null)
            {
                var uiRoot = GameObject.Find("MainCanvas")?.transform;
                if (uiRoot == null) return;
                var minigamePrefab = CoreManager.Instance.resourceManager.GetAsset<GameObject>("MinigameUI");
                if (minigamePrefab == null) return;
                var instance = GameObject.Instantiate(minigamePrefab, uiRoot);
                ui = instance.GetComponent<MinigameUI>();
                if (ui == null) return;
            }
            CurrentAlphabets = GetAlphabets();
            CurrentLoopCount = 0;
            IsPlaying = false;
            player.PlayerCondition.IsPlayerHasControl = false;
            player.Pov.m_HorizontalAxis.Reset();
            player.Pov.m_VerticalAxis.Reset();
            player.InputProvider.enabled = false;
            Cursor.lockState = CursorLockMode.None;

            ui.ShowPanel();
            if (IsLoop && LoopCount > 0)
                ui.UpdateLoopCount(CurrentLoopCount + 1, LoopCount);
        }

        public void Initialize(Console con, Player player)
        {
            console = con;
            this.player = player;
        }

        private void Update()
        {
            // Minigame 초입
            if (!IsPlaying)
            {
                if (Input.GetKeyDown(KeyCode.Return))
                {
                    StartCoroutine(StartCountdown_Coroutine());
                    ui.ShowAlphabet(true);
                    ui.CreateAlphabet(CurrentAlphabets);
                    ui.ShowTimeSlider(true);
                    ui.SetTimeSlider(Duration, Duration);
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
            if (isSuccess)
            {
                ui.ShowClearText(true);
                ui.SetClearText(true, "CLEAR!");
                console.OnCleared();
            }
            player.PlayerCondition.IsPlayerHasControl = true;
            player.InputProvider.enabled = true;
            Cursor.lockState = CursorLockMode.Locked;

            ui.HidePanel();
            CoreManager.Instance.objectPoolManager.Release(ui.gameObject);
            ui = null;
            enabled = false;
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

        private IEnumerator StartCountdown_Coroutine()
        {
            ui.ShowCountdownText(true);
            ui.SetCountdownText(Delay);
            
            var t = 0f;
            while (t < Delay)
            {
                t += Time.unscaledDeltaTime;
                ui.SetCountdownText(Delay - t);
                yield return null;
            }
            IsCounting = false;
            startTime = Time.unscaledTime;
            
            ui.ShowCountdownText(false);
            ui.ShowAlphabet(true);
            ui.CreateAlphabet(CurrentAlphabets);
            ui.ShowTimeSlider(true);
            ui.SetTimeSlider(Duration, 0f);
            Service.Log($"Start Game!: {CurrentAlphabets}");
        }
    }
}