using System;
using _1.Scripts.Entity.Scripts.Player.Core;
using _1.Scripts.Interfaces.Weapon;
using _1.Scripts.Manager.Core;
using _1.Scripts.Weapon.Scripts.Common;
using _1.Scripts.Weapon.Scripts.Guns;
using UnityEngine;

namespace _1.Scripts.Weapon.Scripts.Grenade
{
    public class GrenadeThrower : BaseWeapon
    {
        [field: Header("Gun Data")]
        [field: SerializeField] public GrenadeData GrenadeData { get; protected set; }
        
        [field: Header("Current Weapon Settings")]
        [field: SerializeField] public Transform ThrowPoint { get; private set; }
        [field: SerializeField] public int CurrentAmmoCount { get; private set; }
        
        [SerializeField] private Transform face;
        
        [field: Header("Current Weapon State")]
        [SerializeField] private bool isEmpty;
        [SerializeField] private bool isRecoiling;

        private float timeSinceLastShotFired;
        
        public bool IsReady => !isEmpty && !isRecoiling;

        private void Awake()
        {
            if (!ThrowPoint) ThrowPoint = this.TryGetChildComponent<Transform>("ThrowPoint");
        }

        private void Reset()
        {
            if (!ThrowPoint) ThrowPoint = this.TryGetChildComponent<Transform>("ThrowPoint");
        }
        
        private void Update()
        {
            if (!isRecoiling) return;
            timeSinceLastShotFired += Time.deltaTime;
            
            if (!(timeSinceLastShotFired >= 60f / GrenadeData.GrenadeStat.Rpm)) return;
            timeSinceLastShotFired = 0f;
            isRecoiling = false;
        }

        public override void Initialize(GameObject ownerObj)
        {
            owner = ownerObj;
            if (ownerObj.TryGetComponent(out Player user))
            {
                player = user;
                isOwnedByPlayer = true;
                if (CoreManager.Instance.gameManager.SaveData != null)
                {
                    var weapon = CoreManager.Instance.gameManager.SaveData.Weapons[(int)GrenadeData.GrenadeStat.Type];
                    CurrentAmmoCount = weapon.currentAmmoCount;
                    if (CurrentAmmoCount <= 0) isEmpty = true;
                }
                else
                {
                    CurrentAmmoCount = 0;
                }
                
                face = user.CameraPivot;
            }
        }

        public override void OnShoot()
        {
            if (!IsReady) return;
            
            var obj = CoreManager.Instance.objectPoolManager.Get(GrenadeData.GrenadeStat.GrenadePrefabId); 
            if(obj == null) Service.Log("Grenade Object is null!");
            if (!obj.TryGetComponent(out Grenade grenade)) return;
            grenade.Initialize(ThrowPoint.position, face.forward, HittableLayer,
                GrenadeData.GrenadeStat.Damage, GrenadeData.GrenadeStat.ThrowForce, GrenadeData.GrenadeStat.Force, 
                GrenadeData.GrenadeStat.Radius, GrenadeData.GrenadeStat.Delay);
            
            isRecoiling = true;
            
            CurrentAmmoCount--;
            if (CurrentAmmoCount <= 0) isEmpty = true;
            if (player != null) player.PlayerCondition.IsAttacking = false;
        }

        public override bool OnRefillAmmo(int ammo)
        {
            if (CurrentAmmoCount >= GrenadeData.GrenadeStat.MaxAmmoCount) return false;
            CurrentAmmoCount = Mathf.Min(CurrentAmmoCount + ammo, GrenadeData.GrenadeStat.MaxAmmoCount);
            return true;
        }
    }
}