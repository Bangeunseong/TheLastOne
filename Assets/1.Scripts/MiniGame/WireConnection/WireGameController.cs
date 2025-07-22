using System;
using System.Collections.Generic;
using System.Linq;
using _1.Scripts.Entity.Scripts.Player.Core;
using _1.Scripts.UI.InGame;
using _1.Scripts.UI.InGame.Minigame;
using Cysharp.Threading.Tasks;
using UnityEngine;
using Console = _1.Scripts.Map.Console.Console;
using Random = UnityEngine.Random;

namespace _1.Scripts.MiniGame.WireConnection
{
    public class WireGameController : BaseMiniGame
    {
        [field: Header("Components")]
        [SerializeField] private GameObject wirePrefab;
        [SerializeField] private GameObject socketPrefab;
        [SerializeField] private Canvas canvas;
        [SerializeField] private RectTransform top;
        [SerializeField] private RectTransform bottom;
        [field: SerializeField] public RectTransform WireContainer { get; private set; }
        
        [field: Header("Game Settings")]
        [field: Range(2, 5)][field: SerializeField] public int SocketCount { get; private set; } = 3;
        [field: SerializeField] public float Duration { get; private set; } = 5f;
        [field: SerializeField] public float Delay { get; private set; } = 3f;
        
        private readonly List<(Socket, Socket, GameObject)> connections = new();
        private readonly List<GameObject> sockets = new();
        private WireConnectionUI wireConnectionUI;
        private MinigameUI minigameUI;
        
        private void Initialize(Canvas can, WireConnectionUI ui)
        {
            canvas = can;
            top = ui.Top;
            bottom = ui.Bottom;
            WireContainer = ui.WireContainer;
        }
        
        public override void StartMiniGame(Console con, Player ply)
        {
            base.StartMiniGame(con, ply);
            
            minigameUI = uiManager.ShowUI<MinigameUI>();
            minigameUI.ShowMiniGame();
            minigameUI.SetDescriptionText("WIRECONNECT");
            wireConnectionUI = uiManager.GetUI<MinigameUI>().GetWireConnectionUI(); 
            Initialize(uiManager.RootCanvas, wireConnectionUI); 
            wireConnectionUI.Show();
            enabled = true;
        }
        
        protected override void Update()
        {
            if (coreManager.gameManager.IsGamePaused || isFinished) return;

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
            minigameUI.UpdateTimeSlider(remaining);
            if (!(Time.unscaledTime - startTime >= Duration)) return;
            FinishGame(false, 0f);
        }
        
        protected override void OnDisable()
        {
            ResetAllConnections();
            ResetAllSockets();
        }

        public override void CancelMiniGame()
        {
            base.CancelMiniGame();
            
            // Clear all remaining sockets and line renderers
            ResetAllConnections();
            ResetAllSockets();
            
            FinishGame(false, 0f);
        }
        
        private void CreateSockets()
        {
            var colorList = GetRandomColors();
            foreach (var color in colorList)
            {
                var topSocket = Instantiate(socketPrefab, top);
                
                if (topSocket.TryGetComponent(out Socket topSo)) topSo.Initialize(color, SocketType.Start); 
                if (topSocket.TryGetComponent(out WireDragger topDrag)) topDrag.Initialize(canvas, this);
                sockets.Add(topSocket);
            }
            
            Shuffle(colorList);
            foreach (var color in colorList)
            {
                var bottomSocket = Instantiate(socketPrefab, bottom);
                if (bottomSocket.TryGetComponent(out Socket bottomSo)) bottomSo.Initialize(color, SocketType.End); 
                if (bottomSocket.TryGetComponent(out WireDragger bottomDrag)) bottomDrag.Initialize(canvas, this);
                sockets.Add(bottomSocket);                                                  
            }
        }

        public GameObject CreateLine(Transform wireContainer)
        {
            GameObject go = Instantiate(wirePrefab, wireContainer);
            return go;
        }

        public void RegisterConnection(Socket start, Socket end, GameObject line)
        {
            connections.Add((start, end, line));
            if (connections.Count < SocketCount) return;
            IsCleared = true;
            FinishGame(IsCleared, 1.5f);
        }

        private void ResetAllConnections()
        {
            foreach (var (start, end, line) in connections)
            {
                start.IsConnected = end.IsConnected = false;
                Destroy(line);
            }
            connections.Clear();
        }

        private void ResetAllSockets()
        {
            foreach (var socket in sockets) Destroy(socket);
            sockets.Clear();
        }
        
        protected override async UniTask StartCountdown_Async()
        {
            minigameUI.ShowCountdownText(true);
            minigameUI.SetCountdownText(Delay);
            minigameUI.ShowTimeSlider(false);
            minigameUI.ShowEnterText(false);
            minigameUI.ShowClearText(false);
            minigameUI.ShowLoopText(false);
            
            var t = 0f;
            while (t < Delay)
            {
                if (!coreManager.gameManager.IsGamePaused)
                    t += Time.unscaledDeltaTime;
                minigameUI.SetCountdownText(Delay - t);
                await UniTask.Yield(PlayerLoopTiming.Update);
            }
            minigameUI.ShowCountdownText(false);
            minigameUI.ShowTimeSlider(true);
            minigameUI.SetTimeSlider(Duration, Duration);
            
            CreateSockets();
            IsCounting = false;
            startTime = Time.unscaledTime;
        }
        
        protected override async UniTask EndGame_Async(bool success, float duration)
        {
            Service.Log(success ? "Cleared MiniGame!" : "Better Luck NextTime");
            minigameUI.ShowClearText(true);
            minigameUI.SetClearText(success, success ? "CLEAR!" : "FAIL");
            
            minigameUI.ShowEnterText(false);
            minigameUI.ShowTimeSlider(false);
            minigameUI.ShowDescriptionText(false);
            wireConnectionUI.Hide();

            await UniTask.WaitForSeconds(duration, true);
            
            minigameUI.Hide();
            
            console.OnCleared(success);
            Cursor.lockState = CursorLockMode.Locked; 
            Cursor.visible = false;
            enabled = false;
        }

        private static void Shuffle<T>(List<T> list)
        {
            var rng = new System.Random();
            int n = list.Count;
            while (n > 1)
            {
                n--;
                int k = rng.Next(n + 1);
                (list[n], list[k]) = (list[k], list[n]);
            }
        }
        
        private List<SocketColor> GetRandomColors()
        {
            Array colors = Enum.GetValues(typeof(SocketColor));
            var colorList = colors.Cast<SocketColor>().ToList();
            for (var i = 0; i < colors.Length - SocketCount; i++)
                colorList.RemoveAt(Random.Range(0, colorList.Count));
            return colorList;
        }
    }
}