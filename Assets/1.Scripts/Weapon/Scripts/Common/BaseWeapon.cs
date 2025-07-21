using _1.Scripts.Entity.Scripts.Player.Core;
using _1.Scripts.Interfaces.Weapon;
using _1.Scripts.Manager.Data;
using JetBrains.Annotations;
using UnityEngine;

namespace _1.Scripts.Weapon.Scripts.Common
{
    public enum WeaponType
    {
        Rifle,
        Pistol,
        GrenadeLauncher,
        Crossbow,
    }
    
    public abstract class BaseWeapon : MonoBehaviour, IShootable
    {
        [field: Header("Hittable Layer")]
        [field: SerializeField] public LayerMask HittableLayer { get; protected set; }
        
        [Header("Owner")] 
        [SerializeField] protected GameObject owner;
    
        // Fields
        [CanBeNull] protected Player player;
        // private Enemy enemy;
        protected bool isOwnedByPlayer;
        
        public abstract void Initialize(GameObject ownerObj, DataTransferObject dto = null);
        public abstract bool OnShoot();
        public abstract bool OnRefillAmmo(int ammo);
    }
}