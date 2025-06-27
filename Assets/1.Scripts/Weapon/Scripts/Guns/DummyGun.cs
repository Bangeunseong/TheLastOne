using System;
using _1.Scripts.Entity.Scripts.Player.Core;
using _1.Scripts.Interfaces;
using _1.Scripts.Interfaces.Player;
using UnityEngine;

namespace _1.Scripts.Weapon.Scripts.Guns
{
    public class DummyGun : MonoBehaviour, IInteractable
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
                if (player.PlayerCondition.Weapons[i] is not Gun gun || gun.GunData.GunStat.Type != Type) continue;
                index = i; break;
            }
            
            player.PlayerCondition.AvailableWeapons[index] = true;
            player.PlayerCondition.OnSwitchWeapon(index, 0.5f);
            player.PlayerCondition.LastSavedPosition = player.transform.position;
            player.PlayerCondition.LastSavedRotation = player.transform.rotation;
            
            OnPicked?.Invoke();
            Destroy(gameObject);
        }
    }
}