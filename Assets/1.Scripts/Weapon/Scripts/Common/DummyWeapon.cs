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
        [field: SerializeField] public WeaponType Type { get; private set; }

        public event Action OnPicked;

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
                if (index == (int)WeaponType.GrenadeThrow) player.PlayerCondition.Weapons[index].OnRefillAmmo(6);
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