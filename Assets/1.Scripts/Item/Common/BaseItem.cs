using System;
using _1.Scripts.Interfaces.Item;
using _1.Scripts.Manager.Data;
using UnityEngine;

namespace _1.Scripts.Item.Common
{
    [Serializable] public class BaseItem : IItem
    {
        [field: Header("Item Data")]
        [field: SerializeField] public ItemData ItemData { get; protected set; }
        
        [field: Header("Current Item Stat.")]
        [field: SerializeField] public int CurrentItemCount { get; protected set; }
        
        public virtual void Initialize(DataTransferObject dto = null) { }
        public virtual void OnUse(GameObject interactor) { }
    }
}