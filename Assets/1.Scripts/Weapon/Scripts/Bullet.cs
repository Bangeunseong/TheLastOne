using System;
using _1.Scripts.Interfaces;
using _1.Scripts.Manager.Core;
using RaycastPro.Bullets;
using UnityEngine;

namespace _1.Scripts.Weapon.Scripts
{
    public class Bullet : MonoBehaviour
    {
        [Header("Components")] 
        [SerializeField] private Rigidbody rigidBody;
        [SerializeField] private PhysicalBullet bullet;

        [Header("Bullet Presets")] 
        [SerializeField] private float appliedForce;
        [SerializeField] private float maxMoveDistance;
        [SerializeField] private Vector3 initializedPosition;
        [SerializeField] private int damage;
        [SerializeField] private LayerMask hittableLayer;
        [SerializeField] private Vector3 direction;
        
        [Header("Bullet Settings")]
        [SerializeField] private float drag;

        private bool isAlreadyReached;

        private void Awake()
        {
            if (!rigidBody) rigidBody = this.TryGetComponent<Rigidbody>();
        }

        private void Reset()
        {
            if (!rigidBody) rigidBody = this.TryGetComponent<Rigidbody>();
        }

        private void OnEnable()
        {
            isAlreadyReached = false;
            rigidBody.useGravity = false;
            rigidBody.drag = 0f;
            rigidBody.velocity = Vector3.zero;
            rigidBody.angularVelocity = Vector3.zero;
        }

        private void FixedUpdate()
        {
            if (isAlreadyReached) return;
            if (!((transform.position - initializedPosition).sqrMagnitude > maxMoveDistance * maxMoveDistance)) return;
            rigidBody.useGravity = true;
            rigidBody.drag = drag;
            isAlreadyReached = true;
        }

        public void Initialize(Vector3 position, Vector3 dir, float maxDistance, float force, int dealtDamage, LayerMask hitLayer)
        {
            transform.position = position;
            initializedPosition = position;
            transform.rotation = Quaternion.LookRotation(dir);
            
            damage = dealtDamage;
            bullet.damage = damage;
            appliedForce = force; 
            maxMoveDistance = maxDistance;
            direction = dir;
            hittableLayer = 0;
            hittableLayer |= hitLayer;

            rigidBody.velocity = direction * appliedForce;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (((1 << other.gameObject.layer) & hittableLayer) != 0)
            {
                if (other.TryGetComponent(out IDamagable damagable)){ damagable.OnTakeDamage(damage); }
            }
            CoreManager.Instance.objectPoolManager.Release(gameObject);
        }
    }
}