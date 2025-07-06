using System;
using System.Collections.Generic;
using _1.Scripts.Item.Common;
using _1.Scripts.Item.Items;
using _1.Scripts.Manager.Data;
using UnityEngine;

namespace _1.Scripts.Entity.Scripts.Player.Core
{
    public class PlayerInventory : MonoBehaviour
    {
        [field: Header("Items")]
        [field: SerializeField] public List<BaseItem> Items { get; private set; }

        private void Start()
        {
            
        }

        private void Initialize(DataTransferObject dto = null)
        {
            Items = new List<BaseItem>();
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
                item.Initialize(dto);
            }
        }
    }
}