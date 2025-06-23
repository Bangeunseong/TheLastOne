using System;
using UnityEngine;

namespace _1.Scripts.Weapon.Scripts
{
    public class Bullet : MonoBehaviour
    {
        [Header("Components")] 
        [SerializeField] private Rigidbody rigidBody;

        [Header("Bullet Settings")] 
        [SerializeField] private float appliedForce;
        [SerializeField] private float maxMoveDistance;
        [SerializeField] private float currentMoveDistance;
        [SerializeField] private LayerMask hittableLayer;
        [SerializeField] private Vector3 direction;

        private void Awake()
        {
            if (!rigidBody) rigidBody = this.TryGetComponent<Rigidbody>();
        }

        private void OnEnable()
        {
            rigidBody.useGravity = false;
            
        }

        public void Initialize(float force, float maxDistance, Vector3 position, Vector3 dir, LayerMask layerToRemove)
        {
            transform.position = position;
            transform.rotation = Quaternion.LookRotation(dir);
            
            appliedForce = force; 
            maxMoveDistance = maxDistance;
            direction = dir;
            hittableLayer &= ~layerToRemove;
            
            rigidBody.AddForce(direction * appliedForce);
        }

        private void OnTriggerEnter(Collider other)
        {
            
        }
    }
}