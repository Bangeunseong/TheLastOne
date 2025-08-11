using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace _1.Scripts.Util
{
    public class TimelineSkipBehavior : MonoBehaviour
    {
        [Header("Timeline")]
        [SerializeField] private PlayableDirector director;

        private List<(string name, double time)> markers = new();

        private void Start()
        {
            LoadMarkers();
        }

        private void LoadMarkers()
        {
            markers.Clear();

            if (director.playableAsset is not TimelineAsset timeline) return;
            
            foreach (var track in timeline.GetOutputTracks())
            {
                foreach (var marker in track.GetMarkers())
                {
                    if (marker is not SignalEmitter signal) continue;
                    markers.Add((signal.name, marker.time));
                }
            }

            // 시간 순 정렬
            markers = markers.OrderBy(m => m.time).ToList();
        }

        public void SkipToNextMarker()
        {
            if (markers.Count == 0 || !director) return;

            double currentTime = director.time;

            // 현재 시간보다 뒤에 있는 첫 번째 마커
            var nextMarker = markers.FirstOrDefault(m => m.time > currentTime);

            if (nextMarker == default)
            {
                Debug.Log("마지막 마커 이후입니다. 더 이상 스킵할 마커 없음.");
                return;
            }
            director.time = nextMarker.time;
            director.Evaluate();
        }
    }
}
