using System.Collections;
using System.Collections.Generic;
using _1.Scripts.Entity.Scripts.Player.Core;
using _1.Scripts.Manager.Core;
using _1.Scripts.Manager.Subs;
using _1.Scripts.Util;
using _1.Scripts.Weapon.Scripts.Common;
using _1.Scripts.Weapon.Scripts.Guns;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace _1.Scripts.UI.InGame.HUD
{
    public enum SlotType
    {
        Main,
        Pistol,
        HackGun,
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
        [SerializeField] private TextMeshProUGUI currentAmmoText;
        [SerializeField] private TextMeshProUGUI currentTotalAmmoText;
        [SerializeField] private Image ammoSlotFrame;
        [SerializeField] private RectTransform currentAmmoRectTransform;
        [SerializeField] private float shakeDuration = 0.2f;
        [SerializeField] private float shakeStrength = 3f;

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

        [Header("애니메이터")] 
        [SerializeField] private Animator panelAnimator;
        [SerializeField] private float panelHideDelay = 3f;
        private Coroutine hideCoroutine;
        private bool isPanelVisible = false;

        private PlayerCondition playerCondition;
        private int lastSelectedIndex = -1;
        
        private Vector3 originalLocalPosition;
        private Coroutine shakeCoroutine;
        private int lastMag = -1;

        public override void Initialize(UIManager manager, object param = null)
        {
            base.Initialize(manager, param);
            targetScales = new Vector3[slotTransform.Length];
            gameObject.SetActive(false);
            
            if (currentAmmoRectTransform) originalLocalPosition = currentAmmoRectTransform.localPosition;
        }
        
        public override void ResetUI()
        {
            lastSelectedIndex = -1;
            for (int i = 0; i < slotTransform.Length; i++)
            {
                slotTransform[i].localScale = normalScale;
                slotImage[i].color = Color.clear;
                slotText[i].text = string.Empty;
                slotAmmoText[i].text = string.Empty;
                slotAmmoText[i].enabled = false;
                slotText[i].enabled = false;
                SetSlotAlpha(i, idleAlpha);

                if (targetScales != null && i < targetScales.Length) targetScales[i] = normalScale;
            }

            if (selectedSlotImage != null)
            {
                foreach (var image in selectedSlotImage)
                {
                    if (image)
                    {
                        var color = image.color;
                        color.a = idleAlpha;
                        image.color = color;
                    }
                }
            }

            currentAmmoText.text = string.Empty;
            currentTotalAmmoText.text = string.Empty;
            if (ammoSlotFrame) ammoSlotFrame.gameObject.SetActive(false);

            isPanelVisible = false;

            if (hideCoroutine != null)
            {
                StopCoroutine(hideCoroutine);
                hideCoroutine = null;
            }

            if (panelAnimator)
            {
                panelAnimator.ResetTrigger("Show");
                panelAnimator.ResetTrigger("Hide");
            }
        }

        private void Update()
        {
            if (targetScales == null) return;
            for (int i = 0; i < slotTransform.Length; i++)
            {
                slotTransform[i].localScale = Vector3.Lerp(slotTransform[i].localScale, targetScales[i], Time.deltaTime * scaleSpeed);
            }
        }
        
        public void Refresh(bool playShowAnimation = true)
        { 
            playerCondition = CoreManager.Instance.gameManager.Player.PlayerCondition;
            
            var weapons = playerCondition.Weapons;
            var available = playerCondition.AvailableWeapons;
            int selectedIndex = playerCondition.EquippedWeaponIndex;

            if (weapons == null || available == null) return;
            if (weapons.Count <= 0 || available.Count <= 0) return;

            bool selectionChanged = selectedIndex != lastSelectedIndex;
            lastSelectedIndex = selectedIndex;
            
            UpdateCurrentAmmoText(weapons, available, selectedIndex);

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

                bool isSelected = slotWeapon && weapons[selectedIndex] == slotWeapon;
                slotText[i].enabled = isSelected;
                slotAmmoText[i].enabled = isSelected && (mag > 0 || total > 0);

                SetSlotAlpha(i, isSelected ? selectedSlotAlpha : idleAlpha);
                targetScales[i] = isSelected ? selectedScale : normalScale;
            }
            if (selectionChanged && playShowAnimation && selectedIndex >= 0 && selectedIndex < slotAnimator.Length)
            {
                if (!isPanelVisible)
                {
                    slotAnimator[selectedIndex]?.Rebind();
                    slotAnimator[selectedIndex]?.Play(0);
                    panelAnimator?.ResetTrigger("Show");
                    panelAnimator?.ResetTrigger("Hide");
                    panelAnimator?.SetTrigger("Show");

                    if (hideCoroutine != null) StopCoroutine(hideCoroutine);
                    hideCoroutine = StartCoroutine(HidePanelCoroutine());
                }
            }
        }        

        private IEnumerator HidePanelCoroutine()
        {
            yield return new WaitForSeconds(panelHideDelay);
            panelAnimator?.ResetTrigger("Show");
            panelAnimator?.ResetTrigger("Hide");
            panelAnimator?.SetTrigger("Hide");
            isPanelVisible = false;
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

        private void UpdateCurrentAmmoText(List<BaseWeapon> weapons, List<bool> available, int selectedIndex)
        {
            if (weapons == null || available == null || selectedIndex < 0 || selectedIndex >= weapons.Count)
            {
                ammoSlotFrame.gameObject.SetActive(false);
                currentAmmoText.text = string.Empty;
                currentTotalAmmoText.text = string.Empty;
                return;
            }

            var currentWeapon = weapons[selectedIndex];
            var (mag, total) = SlotUtility.GetWeaponAmmo(currentWeapon);

            if (lastMag != -1 && mag < lastMag)
            {
                if (shakeCoroutine != null)
                    StopCoroutine(shakeCoroutine);
                shakeCoroutine = StartCoroutine(ShakeCoroutine());
            }
            
            lastMag = mag;
            
            if (mag > 0 || total > 0)
            {
                ammoSlotFrame.gameObject.SetActive(true);
                currentAmmoText.text = $"{mag}";
                currentTotalAmmoText.text = $"{total}";
            }
            else
            {
                ammoSlotFrame.gameObject.SetActive(false);
                currentAmmoText.text = string.Empty;
                currentTotalAmmoText.text = string.Empty;
            }
        }

        private IEnumerator ShakeCoroutine()
        {
            float timer = 0f;
            while (timer < shakeDuration)
            {
                float offsetX = Random.Range(-shakeStrength, shakeStrength);
                float offsetY = Random.Range(-shakeStrength, shakeStrength);
                currentAmmoRectTransform.localPosition = originalLocalPosition + new Vector3(offsetX, offsetY, 0f);
                timer += Time.deltaTime;
                yield return null;
            }
            currentAmmoRectTransform.localPosition = originalLocalPosition;
        }
    }
}
