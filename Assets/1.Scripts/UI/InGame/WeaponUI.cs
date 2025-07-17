using System.Collections;
using System.Collections.Generic;
using System.Linq;
using _1.Scripts.Entity.Scripts.Player.Core;
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
    public class WeaponUI : MonoBehaviour
    {
        [SerializeField] private AmmoUI ammoUI;
        
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
        
        [Header("컬러 세팅")] 
        [SerializeField] private Color selectedAmmoColor = Color.white;
        [SerializeField] private Color selectedColor = Color.black;
        [SerializeField] private Image[] selectedSlotImage;
        [SerializeField] private float idleAlpha = 0.5f;
        [SerializeField] private float selectedSlotAlpha = 1f;

        [Header("애니메이터")] [SerializeField] private Animator panelAnimator;
        [SerializeField] private float panelHideDelay = 3f;
        private Coroutine hideCoroutine;

        private PlayerCondition playerCondition;
        
        private int lastSelectedIndex = -1;

        private void Awake()
        {
            if (slotType != null) targetScales = new Vector3[slotType.Length];
            
            for (int i = 0; i < slotType.Length; i++)
            {
                if (slotTransform[i] != null) slotTransform[i].localScale = normalScale;
                targetScales[i] = normalScale;

                if (selectedSlotImage != null && i < selectedSlotImage.Length && selectedSlotImage[i] != null)
                {
                    var color = selectedSlotImage[i].color;
                    color.a = idleAlpha;
                    selectedSlotImage[i].color = color;
                }
                if (slotAnimator != null && i < slotAnimator.Length && slotAnimator[i] != null) slotAnimator[i].enabled = false;
            }
        }
        
        private void Update()
        {
            bool needsLayout = false;
            for (int i = 0; i < slotTransform.Length; i++)
            {
                Vector3 current = slotTransform[i].localScale;
                Vector3 target = targetScales[i];
                if ((current - target).sqrMagnitude > 0.0001f)
                {
                    slotTransform[i].localScale = Vector3.Lerp(current, target, Time.deltaTime * scaleSpeed);
                    needsLayout = true;
                }
            }
            if (needsLayout)
            {
                var parentRect = slotTransform[0].parent as RectTransform;
                LayoutRebuilder.ForceRebuildLayoutImmediate(parentRect);
            }
        }

        public void ResetUI()
        {
            playerCondition = null;
            lastSelectedIndex = -1;
            ammoUI = null;
        }

        public void Initialize(PlayerCondition newPlayerCondition)
        {
            if (ammoUI == null) ammoUI = GetComponentInChildren<AmmoUI>(true);
            playerCondition = newPlayerCondition;
            Refresh(playerCondition?.Weapons ?? new List<BaseWeapon>(),
                playerCondition?.AvailableWeapons ?? new List<bool>(),
                playerCondition?.EquippedWeaponIndex ?? -1);
            if (panelAnimator != null)
                panelAnimator.Play("Hidden", 0, 1f);
        }
        
        
        public void Refresh(List<BaseWeapon> weapons, List<bool> available, int selectedIndex)
        { 
            if (targetScales == null || slotType == null || slotTransform == null)
            {
                Debug.LogError("WeaponUI: 필드 미할당 (targetScales/slotType/slotTransform)");
                return;
            }
            if (slotType.Length != slotTransform.Length || slotType.Length != targetScales.Length)
            {
                Debug.LogError($"WeaponUI: 배열 길이 불일치 - slotType:{slotType.Length}, slotTransform:{slotTransform.Length}, targetScales:{targetScales.Length}");
                return;
            }
            
            BaseWeapon selectedWeapon = (selectedIndex >= 0 && selectedIndex < weapons.Count && available[selectedIndex]) ? weapons[selectedIndex] : null;
            int currentSlot = -1;
            
            bool selectionChanged = selectedIndex != lastSelectedIndex;
            lastSelectedIndex = selectedIndex;
            
            for (int i = 0; i < slotType.Length; i++)
            {      
                BaseWeapon slotWeapon = weapons.Where((w, idx) => available[idx] && SlotUtility.IsMatchSlot(w, slotType[i]))
                    .FirstOrDefault();

                // slotImage[i].sprite = slotWeapon.iconSprite;
                slotImage[i].color = slotWeapon ? Color.white : Color.clear;
                slotText[i].text = slotWeapon ? SlotUtility.GetWeaponName(slotWeapon) : string.Empty;

                var (mag, total) = SlotUtility.GetWeaponAmmo(slotWeapon);
                bool hasAmmo = mag > 0 || total > 0;
                slotAmmoText[i].text = hasAmmo ? $"{mag}/{total}" : string.Empty;
                slotAmmoText[i].color = slotWeapon is Gun ? selectedColor : selectedAmmoColor;

                bool isSelected = slotWeapon && slotWeapon == selectedWeapon;
                if (isSelected) 
                    currentSlot = i;
                
                slotText[i].enabled = isSelected;
                slotAmmoText[i].enabled = isSelected && hasAmmo;

                if (selectedSlotImage != null && i < selectedSlotImage.Length && selectedSlotImage[i])
                {
                    var color = selectedSlotImage[i].color;
                    color.a = isSelected ? selectedSlotAlpha : idleAlpha;
                    selectedSlotImage[i].color = color;
                }
                
                targetScales[i] = isSelected ? selectedScale : normalScale;
            }
            BaseWeapon sel = (selectedIndex >= 0 && selectedIndex < weapons.Count && available[selectedIndex])
                ? weapons[selectedIndex]
                : null;
            int magVal = 0;
            if (sel)
            {
                magVal = SlotUtility.GetWeaponAmmo(sel).mag;
            }
            if (ammoUI && ammoUI.gameObject) ammoUI.UpdateAmmoUI(magVal);
            if (selectionChanged)
            {
                if (currentSlot >= 0 && currentSlot < slotAnimator.Length && slotAnimator[currentSlot] != null)
                {
                    slotAnimator[currentSlot].enabled = false;
                    slotAnimator[currentSlot].enabled = true;
                }

                if (!panelAnimator) return;
                
                panelAnimator.ResetTrigger("Hide");
                panelAnimator.SetTrigger("Show");
                if (hideCoroutine != null) StopCoroutine(hideCoroutine);
                hideCoroutine = StartCoroutine(HidePanelCoroutine());
            }
        }
        

        private IEnumerator HidePanelCoroutine()
        {
            yield return new WaitForSeconds(panelHideDelay);
            if (panelAnimator)
            {
                panelAnimator.ResetTrigger("Show");
                panelAnimator.SetTrigger("Hide");
            }
            hideCoroutine = null;
        }
    }
}
