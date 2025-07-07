using System;
using System.Collections;
using System.Collections.Generic;
using _1.Scripts.Entity.Scripts.Player.Core;
using Cinemachine;
using Michsky.UI.Shift;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Cursor = UnityEngine.Cursor;

namespace _1.Scripts.UI.InGame
{
    [Serializable]
    public class DummyItemData
    {
        public Sprite icon;
        public int count;
    }
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
        [SerializeField] private DummyItemData[] dummyItems = new DummyItemData[4]; 
        
        private int currentSlot = -1;

        private void Start()
        {
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
            if (!quickSlotPanel.activeSelf) return;
            
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
            // TODO: 아이템 사용 로직 호출
        }

        private void RefreshQuickSlot()
        {
            for (int i = 0; i < slotIcons.Length; i++)
            {
                var data = (i < dummyItems.Length) ? dummyItems[i] : null;

                if (data != null && data.icon != null)
                {
                    slotIcons[i].sprite = data.icon;

                    if (data.count > 0)
                    {
                        slotCounts[i].text = data.count.ToString();
                        slotCounts[i].gameObject.SetActive(true);
                    }
                    else
                    {
                        slotCounts[i].gameObject.SetActive(false);
                    }
                }
                else
                {
                    slotIcons[i].sprite = null;
                    slotCounts[i].gameObject.SetActive(false);
                }
            }
        }
    }
}