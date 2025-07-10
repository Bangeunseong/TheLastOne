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
            coreManager.gameManager.Player.PlayerCondition.IsPlayerHasControl = false;
            coreManager.gameManager.Player.Pov.m_HorizontalAxis.Reset();
            coreManager.gameManager.Player.Pov.m_VerticalAxis.Reset();
            coreManager.gameManager.Player.InputProvider.enabled = false;
            Cursor.lockState = CursorLockMode.None;
            ui.ShowPanel();
            if (IsLoop && LoopCount > 0)
                ui.UpdateLoopCount(CurrentLoopCount, LoopCount);
        }

        private void Update()
        {
            // Minigame 초입
            if (!IsPlaying)
            {
                if (!Input.GetKeyDown(KeyCode.Return))
                {
                    ui.ShowEnterText(true);
                    return;
                }
                ui.ShowEnterText(false);
                
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
                ui.UpdateLoopCount(CurrentLoopCount, LoopCount);
                return;
            }
            
            // Minigame 메인 로직
            float remainTime = Duration - (Time.unscaledTime - startTime);
            ui.UpdateTimeSlider(remainTime);
            if (Time.unscaledTime - startTime >= Duration)
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
            if (isSuccess)
            {
                ui.ShowClearText(true);
                ui.SetClearText(true, "CLEAR!");
                OnSuccess?.Invoke();
            }
            coreManager.gameManager.Player.PlayerCondition.IsPlayerHasControl = true;
            coreManager.gameManager.Player.InputProvider.enabled = true;
            enabled = false;
            Cursor.lockState = CursorLockMode.Locked;
            ui.ShowClearText(false);
            ui.HidePanel();
            Destroy(ui.gameObject, 1f);
            ui = null;
        }

        private void ResetGame()
        {
            IsPlaying = false;
            CurrentAlphabets = GetAlphabets();
            ui.ShowPanel();
        }

        private string GetAlphabets()
        {
            StringBuilder builder = new();
            for (var i = 0; i < AlphabetLength; i++) builder.Append($"{'a' + Random.Range(0, 26)}");
            return builder.ToString();
        }

        private IEnumerator StartCountdown_Coroutine()
        {
            ui.ShowCountdownText(true);
            ui.SetCountdownText(Duration);
            var t = 0f;
            while (t < Delay)
            {
                t += Time.unscaledDeltaTime;
                ui.SetCountdownText(Delay - t);
                yield return null;
            }
            ui.ShowCountdownText(false);
            ui.ShowAlphabet(true);
            ui.CreateAlphabet(CurrentAlphabets);
            ui.ShowTimeSlider(true);
            ui.SetTimeSlider(Duration, Duration);
            startTime = Time.unscaledTime;
        }
    }
}