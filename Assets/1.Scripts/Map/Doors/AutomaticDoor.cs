using System;
using System.Collections;
using UnityEngine;

namespace _1.Scripts.Map.Doors
{
    public class AutomaticDoor : MonoBehaviour
    {
        [field: Header("Doors")]
        [field: SerializeField] public Transform LowerDoor { get; private set; }
        [field: SerializeField] public Transform UpperDoor { get; private set; }
        [field: SerializeField] public Transform LowerTarget { get; private set; }
        [field: SerializeField] public Transform UpperTarget { get; private set; }
        
        [field: Header("Door Settings")]
        [field: SerializeField] public AnimationCurve DoorAnimationCurve { get; private set; }
        [field: SerializeField] public float Duration { get; private set; }
        
        [field: Header("Detective Targets")]
        [field: SerializeField]
        public LayerMask TargetMask { get; private set; }
        
        private bool isOpen;
        private Vector3 originalUpperPosition;
        private Vector3 originalLowerPosition;
        private Coroutine doorCoroutine;

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
            originalUpperPosition = UpperDoor.localPosition;
            originalLowerPosition = LowerDoor.localPosition;
        }

        private void OnTriggerStay(Collider other)
        {
            if (isOpen || ((1 << other.gameObject.layer) & TargetMask.value) == 0) return;
            if (doorCoroutine != null) return;
            isOpen = !isOpen;
            doorCoroutine = StartCoroutine(Door_Coroutine());
        }

        private void OnTriggerExit(Collider other)
        {
            if (!isOpen || ((1 << other.gameObject.layer) & TargetMask.value) == 0) return;
            if (doorCoroutine != null) return;
            isOpen = !isOpen;
            doorCoroutine = StartCoroutine(Door_Coroutine());
        }

        private IEnumerator Door_Coroutine()
        {
            var time = 0f;
            Vector3 upperDoorPosition = UpperDoor.localPosition;
            Vector3 lowerDoorPosition = LowerDoor.localPosition;
            
            while (time < Duration)
            {
                time += Time.deltaTime;
                float t = time / Duration;

                // 곡선을 적용한 비율
                float curveT = DoorAnimationCurve.Evaluate(t);
                if (isOpen)
                {
                    UpperDoor.localPosition = Vector3.Lerp(upperDoorPosition, UpperTarget.localPosition, curveT);
                    LowerDoor.localPosition = Vector3.Lerp(lowerDoorPosition, LowerTarget.localPosition, curveT);
                }
                else
                {
                    UpperDoor.localPosition = Vector3.Lerp(upperDoorPosition, originalUpperPosition, curveT);
                    LowerDoor.localPosition = Vector3.Lerp(lowerDoorPosition, originalLowerPosition, curveT);
                }
                yield return null;
            }

            doorCoroutine = null;
        }
    }
}