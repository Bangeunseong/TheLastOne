using System.Collections;
using System.Collections.Generic;
using System.Linq;
using _1.Scripts.Entity.Scripts.Player.Core;
using _1.Scripts.Manager.Subs;
using _1.Scripts.Util;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using _1.Scripts.Weapon.Scripts.Common;
using _1.Scripts.Weapon.Scripts.Guns;


namespace _1.Scripts.UI.InGame
{
    public enum SlotType
    {
        Main,
        Pistol,
        Crossbow,
        GrenadeLauncher,
    }
    public class WeaponUI : UIBase
    {
        
        [Header("SlotType")]
        [SerializeField] private SlotType[] slotType;
        
        [Header("Slot")]
        [SerializeField] private RectTransform[] slotTransform;
        [SerializeField] private Animator[] slotAnimator;
 
        [Header("WeaponInfo")]
        [SerializeField] private Image[] slotImage; 
        [SerializeField] private TextMeshProUGUI[] slotText;
        [SerializeField] private TextMeshProUGUI[] slotAmmoText;

        [Header("Scale 세팅")]
        [SerializeField] private Vector3 normalScale = Vector3.one;
        [SerializeField] private Vector3 selectedScale = new Vector3(1.5f,1.2f,1f);
        [SerializeField] private float scaleSpeed = 10f;
        private Vector3[] targetScales;
        
        [Header("컬러 세팅")] [SerializeField] private Color selectedAmmoColor = Color.white;
        [SerializeField] private Color selectedColor = Color.black;
        [SerializeField] private Image[] selectedSlotImage;
        [SerializeField] private float idleAlpha = 0.5f;
        [SerializeField] private float selectedSlotAlpha = 1f;

        [Header("애니메이터")] [SerializeField] private Animator panelAnimator;
        [SerializeField] private float panelHideDelay = 3f;
        private Coroutine hideCoroutine;

        private PlayerCondition  playerCondition;
        private int lastSelectedIndex = -1;

        public override void Init(UIManager manager)
        {
            base.Init(manager);
            targetScales = new Vector3[slotType.Length];
            for (int i = 0; i < slotType.Length; i++)
            {
                slotTransform[i].localScale = normalScale;
                targetScales[i] = normalScale;
                SetSlotAlpha(i, idleAlpha);
                if (slotAnimator[i]) slotAnimator[i].enabled = false;
            }
            
            if (panelAnimator) panelAnimator.Play("Hidden", 0, 1f);
            Hide();
        }
        
        public override void ResetUI()
        {
            playerCondition = null;
            lastSelectedIndex = -1;
        }

        public override void Initialize(object param = null)
        {
            if (param is PlayerCondition newPlayerCondition)
            {
                playerCondition = newPlayerCondition;
                Refresh();
            }
        }

        private void Update()
        {
            for (int i = 0; i < slotTransform.Length; i++)
            {
                slotTransform[i].localScale = Vector3.Lerp(slotTransform[i].localScale, targetScales[i], Time.deltaTime * scaleSpeed);
            }
        }

        public void Initialize(PlayerCondition newPlayerCondition)
        {
            playerCondition = newPlayerCondition;
            Refresh();
        }
        
        
        public void Refresh()
        { 
            var weapons = playerCondition?.Weapons;
            var available = playerCondition?.AvailableWeapons;
            int selectedIndex = playerCondition?.EquippedWeaponIndex ?? -1;

            if (weapons == null || available == null) return;

            bool selectionChanged = selectedIndex != lastSelectedIndex;
            lastSelectedIndex = selectedIndex;

            for (int i = 0; i < slotType.Length; i++)
            {      
                BaseWeapon slotWeapon = null;
                for (int idx = 0; idx < weapons.Count; idx++)
                {
                    if (available[idx] && SlotUtility.IsMatchSlot(weapons[idx], slotType[i]))
                    {
                        slotWeapon = weapons[idx];
                        break;
                    }
                }
                slotImage[i].color = slotWeapon ? Color.white : Color.clear;
                slotText[i].text = slotWeapon ? SlotUtility.GetWeaponName(slotWeapon) : string.Empty;

                var (mag, total) = SlotUtility.GetWeaponAmmo(slotWeapon);
                slotAmmoText[i].text = (mag > 0 || total > 0) ? $"{mag}/{total}" : string.Empty;
                slotAmmoText[i].color = slotWeapon is Gun ? selectedColor : selectedAmmoColor;

                bool isSelected = slotWeapon != null && weapons[selectedIndex] == slotWeapon;
                slotText[i].enabled = isSelected;
                slotAmmoText[i].enabled = isSelected && (mag > 0 || total > 0);

                SetSlotAlpha(i, isSelected ? selectedSlotAlpha : idleAlpha);
                targetScales[i] = isSelected ? selectedScale : normalScale;
            }
            if (selectionChanged && selectedIndex >= 0 && selectedIndex < slotAnimator.Length)
            {
                slotAnimator[selectedIndex]?.Rebind();
                slotAnimator[selectedIndex]?.Play(0);
                panelAnimator?.SetTrigger("Show");

                if (hideCoroutine != null) StopCoroutine(hideCoroutine);
                hideCoroutine = StartCoroutine(HidePanelCoroutine());
            }
        }        

        private IEnumerator HidePanelCoroutine()
        {
            yield return new WaitForSeconds(panelHideDelay);
            panelAnimator?.SetTrigger("Hide");
            hideCoroutine = null;
        }
        
        private void SetSlotAlpha(int index, float alpha)
        {
            if (selectedSlotImage[index])
            {
                var color = selectedSlotImage[index].color;
                color.a = alpha;
                selectedSlotImage[index].color = color;
            }
        }
    }
}
