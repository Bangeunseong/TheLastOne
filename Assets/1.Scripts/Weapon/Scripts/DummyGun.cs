using System;
using _1.Scripts.Entity.Scripts.Player.Core;
using _1.Scripts.Interfaces;
using _1.Scripts.Manager.Core;
using UnityEngine;
using UnityEngine.Events;

namespace _1.Scripts.Weapon.Scripts
{
    public class DummyGun : MonoBehaviour, IInteractable
    {
        [field: Header("DummyGun Settings")]
        [field: SerializeField] public GunType Type { get; private set; }

        public event Action OnPicked;

        public void OnInteract(GameObject ownerObj)
        {
            if (!ownerObj.TryGetComponent(out Player player)) return;
            
            var index = -1;
            for (var i = 0; i < player.Weapons.Count; i++)
            {
                if (player.Weapons[i] is not Gun gun || gun.WeaponData.WeaponStat.Type != Type) continue;
                index = i; break;
            }
            
            player.AvailableWeapons[index] = true;
            player.OnSwitchWeapon(index, 0.5f);
            player.PlayerCondition.LastSavedPosition = player.transform.position;
            player.PlayerCondition.LastSavedRotation = player.transform.rotation;
            
            OnPicked?.Invoke();
            Destroy(gameObject);
        }
    }
}