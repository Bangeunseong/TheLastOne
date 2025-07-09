using System;
using _1.Scripts.Entity.Scripts.Player.Core;
using _1.Scripts.Interfaces.Player;
using _1.Scripts.Item.Common;
using _1.Scripts.Manager.Core;
using UnityEngine;

namespace _1.Scripts.Item.Items
{
    public class DummyItem : MonoBehaviour, IInteractable
    {
        [field: Header("Dummy Item Settings")]
        [field: SerializeField] public ItemType ItemType { get; private set; }
        [field: SerializeField] public Transform Body { get; private set; }
        
        public event Action OnPicked;

        private void Awake()
        {
            if (!Body) Body = this.TryGetChildComponent<Transform>("Body");
        }

        private void Reset()
        {
            if (!Body) Body = this.TryGetChildComponent<Transform>("Body");
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
            Body.gameObject.layer = isTransparent ? LayerMask.NameToLayer("Stencil_Key") : LayerMask.NameToLayer("Default");
        }

        public void RemoveSelfFromSpawnedList()
        {
            CoreManager.Instance.spawnManager.RemoveItemFromSpawnedList(gameObject);
        }

        public void OnInteract(GameObject ownerObj)
        {
            if (!ownerObj.TryGetComponent(out Player player)) return;
            if (player.PlayerInventory.OnRefillItem(ItemType))
            {
                OnPicked?.Invoke();
                CoreManager.Instance.objectPoolManager.Release(gameObject);
            }
            else
            {
                // Service.Log($"Failed to refill {ItemType}");
                CoreManager.Instance.uiManager.InGameUI.ShowMessage("Failed to refill {ItemType}");
            }
        }
    }
}
