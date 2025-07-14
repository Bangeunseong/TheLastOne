using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace _1.Scripts.UI.InGame.Mission
{
    public class PathAnimator : MonoBehaviour
    {
        [Header("참조")]
        [SerializeField] Transform player; 
        [SerializeField] GameObject markerPrefab; 
        [SerializeField] Transform target;

        [Header("설정")] 
        [SerializeField] float updateInterval = 5f;
        [SerializeField] float markerSpeed = 30f;
        [SerializeField] float markerLifetime = 3f;

        NavigationPathFinder pathFinder;
        CancellationTokenSource cts;
        

        void Awake()
        {
            pathFinder = GetComponent<NavigationPathFinder>();
            if (player == null)
            {
                var go = GameObject.FindWithTag("Player");
                if (go != null) player = go.transform;
            }

            target = DistanceUI.CurrentTarget;
        }
        void OnEnable()
        {
            DistanceUI.OnTargetChanged += OnTargetChanged;
            cts = new CancellationTokenSource();
            PathUpdateLoop(cts.Token).Forget();
        }

        void OnDisable()
        {
            DistanceUI.OnTargetChanged -= OnTargetChanged;
            cts = new CancellationTokenSource();
            PathUpdateLoop(cts.Token).Forget();
        }
        
        
        private void OnTargetChanged(Transform newTarget)
        {
            target = newTarget;
        }
        

        async UniTaskVoid PathUpdateLoop(CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                if (player == null || target == null)
                {
                    await UniTask.Delay(TimeSpan.FromSeconds(updateInterval), cancellationToken: token);
                    continue;
                }
                var corners = pathFinder.GetPathCorners(player.position, target.position);
                if (corners.Length > 1)
                    AnimateMarkerAlongPath(corners, token).Forget();
                await UniTask.Delay(TimeSpan.FromSeconds(updateInterval), cancellationToken: token);
            }
        }

        async UniTask AnimateMarkerAlongPath(Vector3[] corners, CancellationToken token)
        {
            corners[0] = player.position;
            var marker = Instantiate(markerPrefab, corners[0], Quaternion.identity);

            var lt = marker.GetComponent<Light>() ?? marker.AddComponent<Light>();
            lt.type = LightType.Point;
            lt.range = 4f;
            lt.intensity = 3f;
            lt.color = Color.cyan;

            var trail = marker.GetComponent<TrailRenderer>() ?? marker.AddComponent<TrailRenderer>();
            trail.material = new Material(Shader.Find("Sprites/Default"));
            trail.time = 1.0f;
            trail.startWidth = 0.2f;
            trail.endWidth = 0.0f;
            var grad = new Gradient();
            grad.SetKeys(
                new[] { new GradientColorKey(Color.cyan, 0f), new GradientColorKey(Color.cyan, 1f) },
                new[] { new GradientAlphaKey(0.5f, 0f), new GradientAlphaKey(0f, 1f) }
            );
            trail.colorGradient = grad;

            for (int i = 1; i < corners.Length; i++)
            {
                while (Vector3.Distance(marker.transform.position, corners[i]) > 0.05f)
                {
                    marker.transform.position = Vector3.MoveTowards(
                        marker.transform.position,
                        corners[i],
                        markerSpeed * Time.deltaTime
                    );
                    await UniTask.NextFrame(token);
                }
            }
            await UniTask.Delay(TimeSpan.FromSeconds(markerLifetime), cancellationToken: token);
            if (marker != null) Destroy(marker);
        }
    }
}