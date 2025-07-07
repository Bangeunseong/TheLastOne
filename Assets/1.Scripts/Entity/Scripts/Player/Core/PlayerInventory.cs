using System;
using _1.Scripts.Item.Common;
using _1.Scripts.Item.Items;
using _1.Scripts.Manager.Core;
using _1.Scripts.Manager.Data;
using AYellowpaper.SerializedCollections;
using UnityEngine;

namespace _1.Scripts.Entity.Scripts.Player.Core
{
    public class PlayerInventory : MonoBehaviour
    {
        [field: Header("Items")]
        [field: SerializeField] public SerializedDictionary<ItemType, BaseItem> Items { get; private set; }
        [field: SerializeField] public ItemType CurrentItem { get; private set; }
        
        [field: Header("QuickSlot Settings")]
        [field: SerializeField] public float HoldDurationToOpen { get; private set; }
        [field: SerializeField] public bool IsOpenUIAction { get; private set; }
        
        private CoreManager coreManager;
        private Player player;
        private bool isPressed;
        private float timeSinceLastPressed;
        
        private void Start()
        {
            coreManager = CoreManager.Instance;
            player = coreManager.gameManager.Player;
            
            Initialize(coreManager.gameManager.SaveData);
        }

        private void Update()
        {
            if (!isPressed || IsOpenUIAction) return;
            IsOpenUIAction = Time.unscaledTime - timeSinceLastPressed >= HoldDurationToOpen;

            if (!IsOpenUIAction) return;
            coreManager.uiManager?.InGameUI.QuickSlotUI.OpenQuickSlot();
            player.Pov.m_HorizontalAxis.Reset();
            player.Pov.m_VerticalAxis.Reset();
            player.InputProvider.enabled = false;
        }

        private void Initialize(DataTransferObject dto = null)
        {
            Items = new SerializedDictionary<ItemType, BaseItem>();
            foreach (ItemType type in Enum.GetValues(typeof(ItemType)))
            {
                BaseItem item = type switch
                {
                    ItemType.Medkit => new Medkit(),
                    ItemType.EnergyBar => new EnergyBar(),
                    ItemType.NanoAmple => new NanoAmple(),
                    ItemType.Shield => new Shield(),
                    _ => throw new ArgumentOutOfRangeException()
                };
                item.Initialize(coreManager, dto);
                if (Items.TryAdd(type, item)) Service.Log($"Successfully added {type}.");
            }
        }

        public void OnItemActionStarted()
        {
            IsOpenUIAction = false;
            isPressed = true;
            timeSinceLastPressed = Time.unscaledTime;
        }
        
        public void OnItemActionCanceled()
        {
            isPressed = false;
            switch (IsOpenUIAction)
            {
                case true:
                    coreManager.uiManager.InGameUI.QuickSlotUI.CloseAndUse(); 
                    player.InputProvider.enabled = true; break;
                case false: OnUseItem(); break;
            }
            IsOpenUIAction = false;
        }

        public void OnSelectItem(ItemType itemType)
        {
            Service.Log($"Attempting to select {itemType}");
            CurrentItem = itemType;
        }
        
        private void OnUseItem()
        {
            Service.Log($"Attempting to use {CurrentItem}.");
            Items[CurrentItem].OnUse(gameObject);
        }
    }
}