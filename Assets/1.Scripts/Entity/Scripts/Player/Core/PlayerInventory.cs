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
        
        private CoreManager coreManager;
        
        private void Start()
        {
            coreManager = CoreManager.Instance;
            
            Initialize(coreManager.gameManager.SaveData);
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

        public void OnSelectItem(ItemType itemType)
        {
            CurrentItem = itemType;
        }
        
        public void OnUseItem()
        {
            Items[CurrentItem].OnUse(gameObject);
        }
    }
}