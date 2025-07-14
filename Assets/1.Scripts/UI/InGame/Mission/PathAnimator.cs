using System.Collections;
using System.Collections.Generic;
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
        }

        void OnDisable()
        {
            DistanceUI.OnTargetChanged -= OnTargetChanged;
        }
        
        private void OnTargetChanged(Transform newTarget)
        {
            target = newTarget;
        }
        
        void Start()
        {
            StartCoroutine(PathUpdateLoop());
        }

        IEnumerator PathUpdateLoop()
        {
            while (true)
            {
                if (player == null || target == null)
                {
                    yield return new WaitForSeconds(updateInterval);
                    continue;
                }
                var corners = pathFinder.GetPathCorners(player.position, target.position);
                if (corners.Length > 1)
                    StartCoroutine(AnimateMarkerAlongPath(corners));
                yield return new WaitForSeconds(updateInterval);
            }
        }

        IEnumerator AnimateMarkerAlongPath(Vector3[] corners)
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
                    yield return null;
                }
            }

            Destroy(marker, markerLifetime);
        }
    }
}