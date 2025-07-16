using System.Collections;
using _1.Scripts.Entity.Scripts.Player.Core;
using _1.Scripts.Item.Common;
using _1.Scripts.Manager.Core;
using Michsky.UI.Shift;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Cursor = UnityEngine.Cursor;

namespace _1.Scripts.UI.InGame
{
    public class QuickSlotUI : MonoBehaviour
    {
        [Header("QuickSlotUI")]
        [SerializeField] private GameObject quickSlotPanel;
        [SerializeField] private Animator quickSlotPanelAnimator;
        [SerializeField] private CanvasGroup quickSlotGroup;
        [SerializeField] private PointerEnterEvents[] slotEvents;
        
        [Header("QuickSlot Elements")] 
        [SerializeField] public Image[] slotIcons;
        [SerializeField] private TextMeshProUGUI[] slotCounts;
        private PlayerInventory inventory;
        
        private int currentSlot = -1;

        private void Start()
        {
            inventory = CoreManager.Instance.gameManager.Player.PlayerInventory;
            
            for (int i = 0; i < slotEvents.Length; i++)
            {
                int idx = i;
                slotEvents[i].enterEvent.AddListener(() => currentSlot = idx);
                slotEvents[i].exitEvent.AddListener(() => currentSlot = -1);
            }
            quickSlotGroup.alpha = 0;
            quickSlotPanel.SetActive(false);
            
            RefreshQuickSlot();
        }

        public void ResetUI()
        {
            inventory = null;
            currentSlot = -1;
            foreach (var icon in slotIcons)
            {
                icon.sprite = null;
                icon.enabled = false;
            }

            foreach (var count in slotCounts)
            {
                count.text = "";
                count.gameObject.SetActive(false);
            }
            quickSlotPanel.SetActive(false);
            quickSlotGroup.alpha = 0;
        }

        public void Initialize(PlayerInventory newInventory)
        {
            inventory = newInventory;
            RefreshQuickSlot();
        }

        public void OpenQuickSlot()
        {
            currentSlot = -1;
            quickSlotPanel.SetActive(true);
            quickSlotGroup.alpha = 1;
            quickSlotGroup.blocksRaycasts = true;
            quickSlotPanelAnimator.Play("Panel In");
            foreach (var slot in slotEvents)
                slot.exitEvent.Invoke();
            RefreshQuickSlot();
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
        }

        public void CloseAndUse()
        {
            if (!quickSlotPanel.activeInHierarchy) return;
            
            quickSlotPanelAnimator.Play("Panel Out");
            quickSlotGroup.alpha = 0;
            quickSlotGroup.blocksRaycasts = false;
            StartCoroutine(CloseQuickSlot());
            
            if (currentSlot != -1) UseSlot(currentSlot);
            else if (currentSlot == -1)
            {
                Service.Log($"선택된 아이템 없음");
            }
            
            RefreshQuickSlot(); 
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
        }

        public void PlayOpenAnimation()
        {
            quickSlotPanelAnimator.Play("Panel In");
        }

        private IEnumerator CloseQuickSlot()
        {
            yield return new WaitForSeconds(0.1f);
            quickSlotPanel.SetActive(false);
        }

        private void UseSlot(int idx)
        {
            Service.Log($"{idx} 번째 슬롯의 아이템 사용");
            CoreManager.Instance.gameManager.Player.PlayerInventory.OnSelectItem((ItemType)currentSlot);
        }

        private void RefreshQuickSlot()
        {
            var items = inventory.Items;
            
            for (int i = 0; i < slotIcons.Length; i++)
            {
                ItemType type = (ItemType)i;

                BaseItem item = null;
                foreach (var it in items)
                {
                    if (it.Key == type)
                    {
                        item = it.Value;
                        break;
                    }
                }
                
                if (item != null && item.CurrentItemCount > 0)
                {
                    slotIcons[i].enabled = true;
                    slotIcons[i].sprite = item.ItemData.Icon;
                    slotCounts[i].text = item.CurrentItemCount.ToString();
                    slotCounts[i].gameObject.SetActive(true);
                }
                else
                {
                    slotIcons[i].sprite = null;
                    slotIcons[i].enabled = false;
                    slotCounts[i].text = "";
                    slotCounts[i].gameObject.SetActive(false);
                }

            }
        }
    }
}