using System;
using System.Collections;
using UnityEngine;

namespace _1.Scripts.Map.Doors
{
    public class ConsoleDoor : MonoBehaviour
    {
        [field: Header("Doors")]
        [field: SerializeField] public Transform LowerDoor { get; private set; }
        [field: SerializeField] public Transform UpperDoor { get; private set; }
        [field: SerializeField] public Transform LowerTarget { get; private set; }
        [field: SerializeField] public Transform UpperTarget { get; private set; }
        
        [field: Header("Door Settings")]
        [field: SerializeField] public AnimationCurve DoorAnimationCurve { get; private set; }
        [field: SerializeField] public float Duration { get; private set; }
        [field: SerializeField] public bool IsOpened { get; private set; }
        
        private void Awake()
        {
            if (!LowerDoor) LowerDoor = this.TryGetChildComponent<Transform>("LowerDoor");
            if (!UpperDoor) UpperDoor = this.TryGetChildComponent<Transform>("UpperDoor");
            if (!LowerTarget) LowerTarget = this.TryGetChildComponent<Transform>("LowerTarget");
            if (!UpperTarget) UpperTarget = this.TryGetChildComponent<Transform>("UpperTarget");
        }

        private void Reset()
        {
            if (!LowerDoor) LowerDoor = this.TryGetChildComponent<Transform>("LowerDoor");
            if (!UpperDoor) UpperDoor = this.TryGetChildComponent<Transform>("UpperDoor");
            if (!LowerTarget) LowerTarget = this.TryGetChildComponent<Transform>("LowerTarget");
            if (!UpperTarget) UpperTarget = this.TryGetChildComponent<Transform>("UpperTarget");
        }

        private void Start()
        {
            // TODO: Get Console Info.
            // If Cleared, Open Doors
            // OpenDoor();
        }

        public void OpenDoor()
        {
            StartCoroutine(OpenDoor_Coroutine());
        }

        private IEnumerator OpenDoor_Coroutine()
        {
            var time = 0f;
            Vector3 upperDoorPosition = UpperDoor.localPosition;
            Vector3 lowerDoorPosition = LowerDoor.localPosition;
            
            IsOpened = true;
            while (time < Duration)
            {
                time += Time.deltaTime;
                float t = time / Duration;

                // 곡선을 적용한 비율
                float curveT = DoorAnimationCurve.Evaluate(t);
                if (IsOpened)
                {
                    UpperDoor.localPosition = Vector3.Lerp(upperDoorPosition, UpperTarget.localPosition, curveT);
                    LowerDoor.localPosition = Vector3.Lerp(lowerDoorPosition, LowerTarget.localPosition, curveT);
                }
                else
                {
                    // UpperDoor.localPosition = Vector3.Lerp(upperDoorPosition, originalUpperPosition, curveT);
                    // LowerDoor.localPosition = Vector3.Lerp(lowerDoorPosition, originalLowerPosition, curveT);
                }
                yield return null;
            }
        }
    }
}