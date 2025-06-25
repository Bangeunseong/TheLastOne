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

        public UnityEvent OnPicked;

        public void OnInteract(GameObject ownerObj)
        {
            if (!ownerObj.TryGetComponent(out Player player)) return;
            
            var index = -1;
            for (var i = 0; i < player.Guns.Count; i++)
            {
                if (player.Guns[i].WeaponData.WeaponStat.Type != Type) continue;
                index = i; break;
            }
            Service.Log(index.ToString());
            player.AvailableGuns[index] = true;
            player.OnSwitchWeapon(index, 0.5f);
                
            OnPicked?.Invoke();
            Destroy(gameObject);
        }
    }
}