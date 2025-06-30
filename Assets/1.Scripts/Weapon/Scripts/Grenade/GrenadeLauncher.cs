using _1.Scripts.Entity.Scripts.Player.Core;
using _1.Scripts.Interfaces.Weapon;
using _1.Scripts.Manager.Core;
using _1.Scripts.Weapon.Scripts.Common;
using Unity.VisualScripting;
using UnityEngine;

namespace _1.Scripts.Weapon.Scripts.Grenade
{
    public class GrenadeLauncher : BaseWeapon, IReloadable
    {
        [field: Header("Gun Data")]
        [field: SerializeField] public GrenadeData GrenadeData { get; protected set; }
        
        [field: Header("Current Weapon Settings")]
        [field: SerializeField] public Transform ThrowPoint { get; private set; }
        [field: SerializeField] public int CurrentAmmoCount { get; private set; }
        [field: SerializeField] public int MaxAmmoCountInMagazine { get; private set; }
        [field: SerializeField] public int CurrentAmmoCountInMagazine { get; private set; }
        
        [SerializeField] private Transform face;
        
        [field: Header("Current Weapon State")]
        [SerializeField] private bool isEmpty;
        [SerializeField] private bool isRecoiling;
        [field: SerializeField] public bool IsReloading { get; set; }
        
        private float timeSinceLastShotFired;
        
        public bool IsReady => !isEmpty && !IsReloading && !isRecoiling;
        public bool IsReadyToReload => MaxAmmoCountInMagazine > CurrentAmmoCountInMagazine && !IsReloading;

        private void Awake()
        {
            if (!ThrowPoint) ThrowPoint = this.TryGetChildComponent<Transform>("ThrowPoint");
        }

        private void Reset()
        {
            if (!ThrowPoint) ThrowPoint = this.TryGetChildComponent<Transform>("ThrowPoint");
        }

        private void Start()
        {
            timeSinceLastShotFired = 0f;
            isRecoiling = false;
            MaxAmmoCountInMagazine = GrenadeData.GrenadeStat.MaxAmmoCountInMagazine;
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
            if (!ownerObj.TryGetComponent(out Player user)) return;
            
            player = user;
            isOwnedByPlayer = true;
            if (CoreManager.Instance.gameManager.SaveData != null)
            {
                var weapon = CoreManager.Instance.gameManager.SaveData.Weapons[(int)GrenadeData.GrenadeStat.Type];
                CurrentAmmoCount = weapon.currentAmmoCount;
                CurrentAmmoCountInMagazine = weapon.currentAmmoCountInMagazine;
                if (CurrentAmmoCountInMagazine <= 0) isEmpty = true;
            }
            else
            {
                CurrentAmmoCount = 0;
                CurrentAmmoCountInMagazine = GrenadeData.GrenadeStat.MaxAmmoCountInMagazine;
            }
                
            face = user.CameraPivot;
        }

        public override void OnShoot()
        {
            if (!IsReady) return;
            
            var obj = CoreManager.Instance.objectPoolManager.Get(GrenadeData.GrenadeStat.GrenadePrefabId); 
            if (!obj.TryGetComponent(out Grenade grenade)) return;
            grenade.Initialize(ThrowPoint.position, GetDirectionOfBullet(), HittableLayer,
                GrenadeData.GrenadeStat.Damage, GrenadeData.GrenadeStat.ThrowForce, GrenadeData.GrenadeStat.Force, 
                GrenadeData.GrenadeStat.Radius, GrenadeData.GrenadeStat.Delay);
            
            isRecoiling = true;
            
            CurrentAmmoCountInMagazine--;
            if (CurrentAmmoCountInMagazine <= 0) isEmpty = true;
            if (player != null) player.PlayerCondition.IsAttacking = false;
        }

        public override bool OnRefillAmmo(int ammo)
        {
            if (CurrentAmmoCount >= GrenadeData.GrenadeStat.MaxAmmoCount) return false;
            CurrentAmmoCount = Mathf.Min(CurrentAmmoCount + ammo, GrenadeData.GrenadeStat.MaxAmmoCount);
            return true;
        }

        public void OnReload()
        {
            int reloadableAmmoCount;
            if (isOwnedByPlayer) 
                reloadableAmmoCount = Mathf.Min(MaxAmmoCountInMagazine - CurrentAmmoCountInMagazine, CurrentAmmoCount);
            else reloadableAmmoCount = MaxAmmoCountInMagazine - CurrentAmmoCount;
            
            if (reloadableAmmoCount <= 0) return;
            
            if (isOwnedByPlayer) CurrentAmmoCount -= reloadableAmmoCount;
            CurrentAmmoCountInMagazine += reloadableAmmoCount;
            isEmpty = CurrentAmmoCountInMagazine <= 0;
        }

        private void GetOrthonormalBasis(Vector3 forward, out Vector3 right, out Vector3 up)
        {
            right = Vector3.Cross(forward, Vector3.up);
            if (right.sqrMagnitude < 0.001f)
                right = Vector3.Cross(forward, Vector3.right);
            right.Normalize();
    
            up = Vector3.Cross(right, forward).normalized;
        }
        
        private Vector3 GetDirectionOfBullet()
        {
            Vector3 targetPoint;
            Vector2 randomCirclePoint = Random.insideUnitCircle * GrenadeData.GrenadeStat.Accuracy;
            
            if (Physics.Raycast(face.position, face.forward, out var hit, GrenadeData.GrenadeStat.MaxWeaponRange,
                    HittableLayer))
            {
                GetOrthonormalBasis(hit.normal, out var right, out var up);
                if (isOwnedByPlayer)
                {
                    if (!player!.PlayerCondition.IsAiming)
                    {
                        targetPoint = hit.point + right * randomCirclePoint.x + up * randomCirclePoint.y;
                    } else targetPoint = hit.point;
                }
                else targetPoint = hit.point + right * randomCirclePoint.x + up * randomCirclePoint.y;
            }
            else
            {
                GetOrthonormalBasis(face.forward, out var right, out var up);
                
                if (isOwnedByPlayer)
                {
                    if (!player!.PlayerCondition.IsAiming)
                    {
                        targetPoint = face.position + face.forward * GrenadeData.GrenadeStat.MaxWeaponRange + right * randomCirclePoint.x + up * randomCirclePoint.y;
                    } else targetPoint = face.position + face.forward * GrenadeData.GrenadeStat.MaxWeaponRange;
                }
                else targetPoint = face.position + face.forward * GrenadeData.GrenadeStat.MaxWeaponRange + right * randomCirclePoint.x + up * randomCirclePoint.y;
            }

            return (targetPoint - ThrowPoint.position).normalized;
        }
    }
}