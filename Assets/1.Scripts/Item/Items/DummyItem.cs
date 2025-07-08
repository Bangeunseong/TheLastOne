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

        public void OnInteract(GameObject ownerObj)
        {
            if (!ownerObj.TryGetComponent(out Player player)) return;
            if (player.PlayerInventory.OnRefillItem(ItemType)) Destroy(gameObject);
            else
            {
                Service.Log($"Failed to refill {ItemType}");
                CoreManager.Instance.uiManager.InGameUI.ShowMessage("Failed to refill {ItemType}");
            } 
            // TODO: UI로 더 이상 아이템을 가질 수 없다는 문구 띄우기
        }
    }
}
