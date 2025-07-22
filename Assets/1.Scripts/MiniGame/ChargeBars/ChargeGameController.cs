using System;
using System.Collections.Generic;
using System.Linq;
using _1.Scripts.Entity.Scripts.Player.Core;
using _1.Scripts.Manager.Core;
using _1.Scripts.Manager.Subs;
using Cysharp.Threading.Tasks;
using UnityEngine;
using Console = _1.Scripts.Map.Console.Console;

namespace _1.Scripts.MiniGame.ChargeBars
{
    public class ChargeGameController : BaseMiniGame
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
        [field: SerializeField] public float Speed { get; private set; } = 5f;
        [field: SerializeField] public int CurrentBarIndex { get; private set; }
        
        private List<Bar> bars;
        private Vector2 direction = Vector2.right;

        protected override void Reset()
        {
            if (!BarLayout) BarLayout = this.TryGetChildComponent<RectTransform>("BarLayout");
            if (!ControlLayout) ControlLayout = this.TryGetChildComponent<RectTransform>("ControlLayout");
            if (!TargetObj) TargetObj = this.TryGetChildComponent<RectTransform>("TargetObj");
            if (!ControlObj) ControlObj = this.TryGetChildComponent<RectTransform>("ControlObj");
        }
        
        public void Initialize(RectTransform parent)
        {
            var transforms = parent.GetComponentsInChildren<RectTransform>();
            if (!BarLayout) BarLayout = transforms.First(val => val.gameObject.name.Equals("BarLayout"));
            if (!ControlLayout) ControlLayout = transforms.First(val => val.gameObject.name.Equals("ControlLayout"));
            if (!TargetObj) TargetObj = transforms.First(val => val.gameObject.name.Equals("TargetObj"));
            if (!ControlObj) ControlObj = transforms.First(val => val.gameObject.name.Equals("ControlObj"));
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
            
            if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Mouse0))
            {
                if (IsOverlapping(TargetObj, ControlObj))
                {
                    bars[CurrentBarIndex].IncreaseValue(ChargeRate);
                    RepositionTargetObj();
                }
            }
            if (CurrentBarIndex < BarCount)
                bars[CurrentBarIndex].DecreaseValue(LossRate * Time.unscaledDeltaTime);
            MoveControlObj();
            
            if (!(Time.time - startTime >= Duration)) return;
            FinishGame(false, 0f);
        }
        
        public override void StartMiniGame(Console con, Player ply)
        {
            base.StartMiniGame(con, ply);
            
            // Initialize MiniGame
            enabled = true;
        }
        
        public override void CancelMiniGame()
        {
            base.CancelMiniGame();
            
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

        private void MoveControlObj()
        {
            var offset = ControlObj.rect.width * 0.5f;
            var farLeft = ControlLayout.rect.xMin + offset;
            var farRight = ControlLayout.rect.xMax - offset;

            var delta = direction * (Speed * Time.unscaledDeltaTime);
            
            if ((ControlObj.anchoredPosition + delta).x <= farLeft) {ControlObj.anchoredPosition = new Vector2(farLeft, ControlObj.anchoredPosition.y); direction = Vector2.right;}
            else if ((ControlObj.anchoredPosition + delta).x >= farRight) {ControlObj.anchoredPosition = new Vector2(farRight, ControlObj.anchoredPosition.y); direction = Vector2.left;}
            else ControlObj.anchoredPosition += delta;
        }

        private void RepositionTargetObj()
        {
            var offset = TargetObj.rect.width * 0.5f;
            var xPos = UnityEngine.Random.Range(ControlLayout.rect.xMin + offset,  ControlLayout.rect.xMax - offset);
            var yPos = ControlLayout.rect.center.y;
            TargetObj.anchoredPosition = new Vector2(xPos, yPos);
        }
        
        private bool IsOverlapping(RectTransform rect1, RectTransform rect2)
        {
            // 바운드를 월드 공간으로 변환
            var worldCorners1 = GetWorldRect(rect1);
            var worldCorners2 = GetWorldRect(rect2);

            return worldCorners1.Overlaps(worldCorners2);
        }
        
        private static Rect GetWorldRect(RectTransform rt)
        {
            var corners = new Vector3[4];
            rt.GetWorldCorners(corners); // 순서: 좌하, 좌상, 우상, 우하

            float x = corners[0].x;
            float y = corners[0].y;
            float width = corners[2].x - corners[0].x;
            float height = corners[2].y - corners[0].y;

            return new Rect(x, y, width, height);
        }
        
        protected override async UniTask StartCountdown_Async()
        {
            var t = 0f;
            while (t < Delay)
            {
                if (!coreManager.gameManager.IsGamePaused) 
                    t += Time.unscaledDeltaTime;
                await UniTask.Yield(PlayerLoopTiming.Update);
            }
            
            CreateBars();
            RepositionTargetObj();
            IsCounting = false; 
            startTime = Time.unscaledTime;
        }

        protected override async UniTask EndGame_Async(bool success, float duration)
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