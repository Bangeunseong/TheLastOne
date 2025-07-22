using System;
using System.Collections;
using System.Collections.Generic;
using _1.Scripts.Manager.Core;
using _1.Scripts.Manager.Subs;
using TMPro;
using UnityEngine;

namespace _1.Scripts.UI.InGame.Mission
{
    public class DistanceUI : UIBase
    {
        [SerializeField] private TextMeshProUGUI distanceText;
        [SerializeField] private PathAnimator pathAnimator;

        private Transform player;
        private Transform target;

        public static Transform CurrentTarget { get; private set; }
        public static event Action<Transform> OnTargetChanged;

        public override void Init(UIManager manager)
        {
            base.Init(manager);
            player = CoreManager.Instance.gameManager.Player?.transform;
            Hide();
        }
        
        public override void ResetUI() { SetTarget(null); }

        public override void Initialize(object param = null)
        {
            if (param is (Transform playerTransform, Transform targetTransform))
            {
                player = playerTransform;
                SetTarget(targetTransform);
            }
        }
        
        public void SetTarget(Transform newTarget)
        {
            if (!player) player = CoreManager.Instance.gameManager.Player?.transform;
            target = newTarget;
            CurrentTarget = newTarget;
            OnTargetChanged?.Invoke(newTarget);

            if (CoreManager.Instance.uiManager.IsCutscene) return;
            
            if (newTarget) Show();
            else Hide();
        }
        
        private void Update()
        {
            if (!player || !target) return;

            float distance = Vector3.Distance(player.position, target.position);
            distanceText.text = $"{distance:0.0}m";
        }
    }
     
}