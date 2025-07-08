using System;
using _1.Scripts.Entity.Scripts.Player.Core;
using _1.Scripts.Interfaces.Player;
using _1.Scripts.Weapon.Scripts.Grenade;
using _1.Scripts.Weapon.Scripts.Guns;
using UnityEngine;

namespace _1.Scripts.Weapon.Scripts.Common
{
    public class DummyWeapon : MonoBehaviour, IInteractable
    {
        [field: Header("DummyGun Settings")]
        [field: SerializeField] public LayerMask TargetLayer { get; private set; }
        [field: SerializeField] public WeaponType Type { get; private set; }
        [field: SerializeField] public Transform[] Renderers { get; private set; }

        private LayerMask originalMask;
        
        public event Action OnPicked;

        private void Awake()
        {
            if (Renderers.Length > 0) return;
            Renderers = this.TryGetChildComponents<Transform>("Gun");
        }

        private void Reset()
        {
            Renderers = this.TryGetChildComponents<Transform>("Gun");
        }

        private void Start()
        {
            originalMask = gameObject.layer;
        }

        public void ChangeLayerOfBody(bool isTransparent)
        {
            gameObject.layer = isTransparent ? TargetLayer : originalMask;
        }

        public void OnInteract(GameObject ownerObj)
        {
            if (!ownerObj.TryGetComponent(out Player player)) return;
            
            var index = -1;
            for (var i = 0; i < player.PlayerCondition.Weapons.Count; i++)
            {
                if (player.PlayerCondition.Weapons[i] is Gun gun && gun.GunData.GunStat.Type == Type)
                {
                    index = i; break;
                } else if (player.PlayerCondition.Weapons[i] is GrenadeLauncher grenadeThrower &&
                           grenadeThrower.GrenadeData.GrenadeStat.Type == Type)
                {
                    index = i; break;
                }
            }
            
            Service.Log($"{index}");

            if (!player.PlayerCondition.AvailableWeapons[index])
            {
                player.PlayerCondition.AvailableWeapons[index] = true;
                player.PlayerCondition.OnSwitchWeapon(index, 0.5f);
            }
            else
            {
                var result = player.PlayerCondition.Weapons[index].OnRefillAmmo(
                    player.PlayerCondition.Weapons[index] is GrenadeLauncher ? 6 : 
                        player.PlayerCondition.Weapons[index] is Gun gun && gun.GunData.GunStat.Type == WeaponType.Pistol ? 30 : 60);
                if (!result) return;
            }
            
            player.PlayerCondition.LastSavedPosition = player.transform.position;
            player.PlayerCondition.LastSavedRotation = player.transform.rotation;
            
            OnPicked?.Invoke();
            Destroy(gameObject);
        }
    }
}