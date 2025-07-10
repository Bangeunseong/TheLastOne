using System;
using System.Collections;
using System.Text;
using _1.Scripts.Manager.Core;
using _1.Scripts.UI.InGame;
using UnityEngine;
using UnityEngine.Events;
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
        
        [field: Header("UI")]
        [field: SerializeField] public MinigameUI ui;
        
        [Header("OnSuccess Callback")]
        public UnityEvent OnSuccess;

        private CoreManager coreManager;
        private float startTime;

        private void Awake()
        {
            coreManager = CoreManager.Instance;
        }

        private void OnEnable()
        {
            CurrentAlphabets = GetAlphabets();
            CurrentLoopCount = 0;
            IsPlaying = false;
            coreManager.gameManager.Player.PlayerCondition.IsPlayerHasControl = false;
            coreManager.gameManager.Player.Pov.m_HorizontalAxis.Reset();
            coreManager.gameManager.Player.Pov.m_VerticalAxis.Reset();
            coreManager.gameManager.Player.InputProvider.enabled = false;
            Cursor.lockState = CursorLockMode.None;
            // UI 띄우기
        }

        private void Update()
        {
            // Minigame 초입
            if (!IsPlaying)
            {
                if (!Input.GetKeyDown(KeyCode.Return))
                {
                    // Press Enter UI
                    return;
                }
                // Press Enter UI 끄기
                
                if (Input.GetKeyDown(KeyCode.Escape))
                {
                    FinishGame(false);
                    return;
                }
                StartCoroutine(StartCountdown_Coroutine());
                IsPlaying = true;
                return;
            }

            // Minigame 달성 여부 확인
            if (CurrentIndex >= AlphabetLength)
            {
                CurrentLoopCount++;
                // 부분 성공 UI 업데이트
                if (!IsLoop || CurrentLoopCount >= LoopCount)
                {
                    FinishGame(true); return;
                }
                ResetGame();
                return;
            }
            
            // Minigame 메인 로직
            if (Time.unscaledTime - startTime >= Duration)
            {
                FinishGame(false); return;
            }
            if (!Input.anyKeyDown) return;
            if (Input.inputString == null) return;
            if (string.Compare(Input.inputString, CurrentAlphabets[CurrentIndex].ToString(),
                    StringComparison.OrdinalIgnoreCase) == 0)
            {
                // TODO: Play UI Effect
                CurrentIndex++;
            }
        }

        private void FinishGame(bool isSuccess)
        {
            if (isSuccess) OnSuccess?.Invoke();
            coreManager.gameManager.Player.PlayerCondition.IsPlayerHasControl = true;
            coreManager.gameManager.Player.InputProvider.enabled = true;
            enabled = false;
            Cursor.lockState = CursorLockMode.Locked;
            // 미니게임 전체 UI 끄기
        }

        private void ResetGame()
        {
            IsPlaying = false;
            CurrentAlphabets = GetAlphabets();
            // UI 갱신
        }

        private string GetAlphabets()
        {
            StringBuilder builder = new();
            for (var i = 0; i < AlphabetLength; i++) builder.Append($"{'a' + Random.Range(0, 26)}");
            return builder.ToString();
        }

        private IEnumerator StartCountdown_Coroutine()
        {
            var t = 0f;
            while (t < Delay)
            {
                t += Time.unscaledDeltaTime;
                // TODO: Show Countdown UI 
                yield return null;
            }
            startTime = Time.unscaledTime;
            // 알파벳 출력 UI 갱신
        }
    }
}