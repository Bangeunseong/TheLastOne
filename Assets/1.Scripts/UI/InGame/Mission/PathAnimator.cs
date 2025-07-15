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
            Debug.Log("[PathAnimator] OnEnable");
            DistanceUI.OnTargetChanged += OnTargetChanged;
            cts = new CancellationTokenSource();
            PathUpdateLoop(cts.Token).Forget();
        }

        void OnDisable()
        {
            Debug.Log("[PathAnimator] OnDisable");
            DistanceUI.OnTargetChanged -= OnTargetChanged;
            if (cts != null)
            {
                cts.Cancel();
                cts.Dispose();
                cts = null;
            }
        }
        
        
        private void OnTargetChanged(Transform newTarget)
        {
            Debug.Log($"[PathAnimator] OnTargetChanged 호출! newTarget: {newTarget?.name} ({newTarget?.position})");
            target = newTarget;
            target = newTarget;
            if (player != null && target != null && pathFinder != null)
            {
                var corners = pathFinder.GetPathCorners(player.position, target.position);
                Debug.Log($"[PathAnimator] 경로 코너 개수: {corners.Length}");
                if (corners.Length > 1)
                {
                    AnimateMarkerAlongPath(corners, cts?.Token ?? CancellationToken.None).Forget();
                    Debug.Log("[PathAnimator] 마커 애니메이션 실행!");
                }
                else
                {
                    Debug.LogWarning("[PathAnimator] corners.Length <= 1 : 경로가 없습니다. NavMesh, Target, Player 위치 확인 필요");
                }
                
            }
            else
            {
                Debug.LogWarning("[PathAnimator] player, target, pathFinder 중 하나가 null!");
            }
        }
        

        async UniTaskVoid PathUpdateLoop(CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                if (player == null || target == null)
                {
                    Debug.LogWarning("[PathAnimator] 플레이어나 타겟이 null입니다.");
                    await UniTask.Delay(TimeSpan.FromSeconds(updateInterval), cancellationToken: token);
                    continue;
                }
                var corners = pathFinder.GetPathCorners(player.position, target.position);
                if (corners.Length > 1)
                    AnimateMarkerAlongPath(corners, token).Forget();
                else
                {
                    Debug.LogWarning("[PathAnimator] corners.Length <= 1 : 경로가 없습니다. NavMesh, Target, Player 위치 확인 필요");
                }
                await UniTask.Delay(TimeSpan.FromSeconds(updateInterval), cancellationToken: token);
            }
        }

        async UniTask AnimateMarkerAlongPath(Vector3[] corners, CancellationToken token)
        {
            corners[0] = player.position;
            var marker = Instantiate(markerPrefab, corners[0], Quaternion.identity);

            var lt = marker.GetComponent<Light>() ?? marker.AddComponent<Light>();
            lt.type = LightType.Point;
            lt.range = 1f;
            lt.intensity = 1f;
            lt.color = Color.cyan;

            var trail = marker.GetComponent<TrailRenderer>() ?? marker.AddComponent<TrailRenderer>();
            trail.material = new Material(Shader.Find("Sprites/Default"));
            trail.time = 1.0f;
            trail.startWidth = 0.1f;
            trail.endWidth = 0.0f;
            var grad = new Gradient();
            grad.SetKeys(
                new[] { new GradientColorKey(Color.cyan, 0f), new GradientColorKey(Color.cyan, 1f) },
                new[] { new GradientAlphaKey(0.2f, 0f), new GradientAlphaKey(0f, 1f) }
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
            if (marker != null) Destroy(marker);
        }
    }
}