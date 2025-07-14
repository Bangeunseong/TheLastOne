using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace _1.Scripts.UI.InGame.Mission
{
    public class DistanceUI : MonoBehaviour
    {
        [SerializeField] private Transform player;
        [SerializeField] private Transform target;
        [SerializeField] private TextMeshProUGUI distanceText;
        [SerializeField] private float updateInterval = 0.1f;

        private float nextUpdateTime;

        private void Start()
        {
            if (player == null)
            {
                var go = GameObject.FindWithTag("Player");
                if (go != null) player = go.transform;
            }

            if (distanceText == null) distanceText = GetComponent<TextMeshProUGUI>();
        }
        
        private void Update()
        {
            if (player == null || target == null || distanceText == null) return;

            if (Time.time >= nextUpdateTime)
            {
                nextUpdateTime = Time.time + updateInterval;
                float dist = Vector3.Distance(player.position, target.position);
                
                distanceText.text = $"{dist:F1} m";
            }
        }

        public void SetTarget(Transform target)
        {
            this.target = target;
        }
    }
}