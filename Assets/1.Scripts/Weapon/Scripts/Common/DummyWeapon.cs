using System;
using _1.Scripts.Entity.Scripts.Player.Core;
using _1.Scripts.Interfaces.Player;
using _1.Scripts.Manager.Core;
using _1.Scripts.Quests.Core;
using _1.Scripts.UI.Inventory;
using _1.Scripts.Weapon.Scripts.Grenade;
using _1.Scripts.Weapon.Scripts.Guns;
using _1.Scripts.Weapon.Scripts.Hack;
using UnityEngine;

namespace _1.Scripts.Weapon.Scripts.Common
{
    public class DummyWeapon : MonoBehaviour, IInteractable
    {
        [field: Header("DummyGun Settings")]
        [field: SerializeField] public int Id { get; private set; }
        [field: SerializeField] public WeaponType Type { get; private set; }
        [field: SerializeField] public Transform[] Renderers { get; private set; }
        
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
        
        private void OnEnable()
        {
            OnPicked += CoreManager.Instance.SaveData_QueuedAsync;
            OnPicked += RemoveSelfFromSpawnedList;
        }

        private void OnDisable()
        {
            OnPicked -= CoreManager.Instance.SaveData_QueuedAsync;
            OnPicked -= RemoveSelfFromSpawnedList;
        }

        public void ChangeLayerOfBody(bool isTransparent)
        {
            foreach (var render in Renderers)
            {
                render.gameObject.layer = isTransparent ? LayerMask.NameToLayer("Stencil_Key") : LayerMask.NameToLayer("Default");
            }
        }

        private void RemoveSelfFromSpawnedList()
        {
            CoreManager.Instance.spawnManager.RemoveWeaponFromSpawnedList(gameObject);
        }

        public void OnInteract(GameObject ownerObj)
        {
            if (!ownerObj.TryGetComponent(out Player player)) return;
            
            var index = -1;
            for (var i = 0; i < player.PlayerCondition.Weapons.Count; i++)
            {
                if (player.PlayerCondition.Weapons[i] is Gun gun && gun.GunData.GunStat.Type == Type || 
                    player.PlayerCondition.Weapons[i] is GrenadeLauncher grenadeThrower && grenadeThrower.GrenadeData.GrenadeStat.Type == Type || 
                    player.PlayerCondition.Weapons[i] is HackGun crossbow && crossbow.HackData.HackStat.Type == Type)
                {
                    index = i; break;
                }
            }

            if (!player.PlayerCondition.AvailableWeapons[index])
            {
                player.PlayerCondition.AvailableWeapons[index] = true;
                player.PlayerCondition.OnSwitchWeapon(index, 0.5f);
            }
            else
            {
                var result = player.PlayerCondition.Weapons[index].OnRefillAmmo(
                    player.PlayerCondition.Weapons[index] is HackGun ? 5 :
                    player.PlayerCondition.Weapons[index] is GrenadeLauncher ? 6 : 
                        player.PlayerCondition.Weapons[index] is Gun gun && gun.GunData.GunStat.Type == WeaponType.Pistol ? 30 : 60);
                if (!result) return;
            }
            
            player.PlayerCondition.LastSavedPosition = player.transform.position;
            player.PlayerCondition.LastSavedRotation = player.transform.rotation;
            
            OnPicked?.Invoke();
            GameEventSystem.Instance.RaiseEvent(Id);
            CoreManager.Instance.uiManager.GetUI<InventoryUI>()?.RefreshInventoryUI();
            CoreManager.Instance.objectPoolManager.Release(gameObject);
        }
        public void OnCancelInteract() { }
    }
}