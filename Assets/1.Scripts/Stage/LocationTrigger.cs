using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace _1.Scripts.Map
{
    public class LocationTrigger : MonoBehaviour
    {
        public event Action OnEnter;

        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Player"))
                OnEnter?.Invoke();
        }
    }
}