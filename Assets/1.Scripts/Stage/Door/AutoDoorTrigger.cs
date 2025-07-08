using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace _1.Scripts.Map.Door
{
    public class AutoDoorTrigger : MonoBehaviour
    {
        [SerializeField] private DoorController door;
        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Player"))
            {
                door.OpenDoor();
                enabled = false;
            }
        }
    }
}