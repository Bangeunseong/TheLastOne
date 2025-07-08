using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace _1.Scripts.Map.Door
{
    public class DoorController : MonoBehaviour
    {
        public enum DoorState
        {
            Open,
            Close
        }
        [SerializeField] private Animator animator;

        private DoorState state = DoorState.Close;

        public void OpenDoor()
        {
            if (state == DoorState.Open) return;
            animator.SetTrigger("Open");
            state = DoorState.Open;
        }

        public void CloseDoor()
        {
            if (state == DoorState.Close) return;
            animator.SetTrigger("Close");
            state = DoorState.Close;
        }
    }
}
