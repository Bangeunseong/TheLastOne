using System.Collections;
using System.Collections.Generic;
using _1.Scripts.Entity.Scripts.Player.Core;
using _1.Scripts.Manager.Core;
using _1.Scripts.Manager.Subs;
using _1.Scripts.Util;
using _1.Scripts.Weapon.Scripts.Common;
using _1.Scripts.Weapon.Scripts.Grenade;
using _1.Scripts.Weapon.Scripts.Guns;
using _1.Scripts.Weapon.Scripts.Hack;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace _1.Scripts.UI.InGame.HUD
{
    public class WeaponUI : UIBase
    {
        [Header("Slot")]
        [SerializeField] private List<RectTransform> slotTransforms;
        [SerializeField] private List<Animator> slotAnimators;
 
        [Header("WeaponInfo")]
        [SerializeField] private List<Image> slotImages; 
        [SerializeField] private List<TextMeshProUGUI> slotTexts;
        [SerializeField] private List<TextMeshProUGUI> slotAmmoTexts;
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
        
        [Header("컬러 세팅")]
        [SerializeField] private Color selectedAmmoColor = Color.white;
        [SerializeField] private Color selectedColor = Color.black;
        [SerializeField] private List<Image> selectedSlotImages;
        [SerializeField] private float idleAlpha = 0.5f;
        [SerializeField] private float selectedSlotAlpha = 1f;

        [Header("애니메이터")] 
        [SerializeField] private Animator panelAnimator;
        [SerializeField] private float panelHideDelay = 3f;
        
        private PlayerCondition playerCondition;
        private PlayerWeapon playerWeapon;
        private Dictionary<WeaponType, int> weaponSlotIndexMap;
        private Dictionary<WeaponType, BaseWeapon> ownedWeapons = new();
        private Vector3[] targetScales;
        private int lastSelectedIndex = -1;
        private Vector3 originalLocalPosition;
        
        private Coroutine hideCoroutine;
        private Coroutine shakeCoroutine;
        private int lastMag = -1;
        private bool isPanelVisible = false;
        
        public override void Initialize(UIManager manager, object param = null)
        {
            base.Initialize(manager, param);
            targetScales = new Vector3[slotTransforms.Count];
            
            if (currentAmmoRectTransform) originalLocalPosition = currentAmmoRectTransform.localPosition;
            
            weaponSlotIndexMap = new();
            for (int i = 0; i < slotTransforms.Count; i++)
                weaponSlotIndexMap[(WeaponType)i] = i;
            gameObject.SetActive(false);
        }
        
        public override void ResetUI()
        {
            lastSelectedIndex = -1;
            for (int i = 0; i < slotTransforms.Count; i++)
            {
                slotTransforms[i].localScale = normalScale;
                slotImages[i].color = Color.clear;
                slotTexts[i].text = string.Empty;
                slotAmmoTexts[i].text = string.Empty;
                slotAmmoTexts[i].enabled = false;
                slotTexts[i].enabled = false;
                SetSlotAlpha(i, idleAlpha);

                if (targetScales != null && i < targetScales.Length) targetScales[i] = normalScale;
            }

            if (selectedSlotImages != null)
            {
                foreach (var image in selectedSlotImages)
                {
                    if (!image) continue;
                    var color = image.color;
                    color.a = idleAlpha;
                    image.color = color;
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

            if (!panelAnimator) return;
            panelAnimator.ResetTrigger("Show");
            panelAnimator.ResetTrigger("Hide");
        }

        private void Update()
        {
            if (targetScales == null) return;
            for (int i = 0; i < slotTransforms.Count; i++) slotTransforms[i].localScale = Vector3.Lerp(slotTransforms[i].localScale, targetScales[i], Time.deltaTime * scaleSpeed);
        }
        
        public void Refresh(bool playShowAnimation = true)
        {
            playerCondition = CoreManager.Instance.gameManager.Player.PlayerCondition;
            playerWeapon = CoreManager.Instance.gameManager.Player.PlayerWeapon;
            
            ownedWeapons.Clear();
            if (!playerWeapon) return;
            var weapons = playerWeapon.Weapons;
            var available = playerWeapon.AvailableWeapons;
            for (int i = 0; i < weapons.Count; i++)
            {
                if (!weapons[i]) continue;
                if (available.Count > i && !available[i]) continue;
                if (!SlotUtility.TryGetWeaponType(weapons[i], out var type)) continue;
                ownedWeapons[type] = weapons[i];
            }
            
            int selectedIndex = playerCondition.EquippedWeaponIndex;
            bool selectionChanged = selectedIndex != lastSelectedIndex;
            lastSelectedIndex = selectedIndex;
            
            UpdateCurrentAmmoText(selectedIndex);

            foreach (var kvp in weaponSlotIndexMap)
            {
                WeaponType weaponType = kvp.Key;
                int slotIdx = kvp.Value;

                BaseWeapon weapon = ownedWeapons.GetValueOrDefault(weaponType);
                
                slotImages[slotIdx].color = weapon ? Color.white : Color.clear;
                slotTexts[slotIdx].text = weapon ? SlotUtility.GetWeaponName(weapon) : string.Empty;

                var (mag, total) = SlotUtility.GetWeaponAmmo(weapon);
                slotAmmoTexts[slotIdx].text = (mag > 0 || total > 0) ? $"{mag}/{total}" : string.Empty;
                slotAmmoTexts[slotIdx].color = weapon is Gun ? selectedColor : selectedAmmoColor;

                bool isSelected = weapon && selectedIndex >= 0 && playerWeapon.Weapons[selectedIndex] == weapon;
                slotTexts[slotIdx].enabled = isSelected;
                slotAmmoTexts[slotIdx].enabled = isSelected && (mag > 0 || total > 0);

                SetSlotAlpha(slotIdx, isSelected ? selectedSlotAlpha : idleAlpha);
                targetScales[slotIdx] = isSelected ? selectedScale : normalScale;
            }

            if (!selectionChanged || !playShowAnimation || selectedIndex < 0 ||
                selectedIndex >= slotAnimators.Count) return;
            if (isPanelVisible) return;
            slotAnimators[selectedIndex]?.Rebind();
            slotAnimators[selectedIndex]?.Play(0);
            panelAnimator?.ResetTrigger("Show");
            panelAnimator?.ResetTrigger("Hide");
            panelAnimator?.SetTrigger("Show");
            if (hideCoroutine != null) StopCoroutine(hideCoroutine);
            hideCoroutine = StartCoroutine(HidePanelCoroutine());
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
            if (selectedSlotImages[index])
            {
                var color = selectedSlotImages[index].color;
                color.a = alpha;
                selectedSlotImages[index].color = color;
            }
        }

        private void UpdateCurrentAmmoText(int selectedIndex)
        {
            if (ownedWeapons == null || playerWeapon.Weapons == null || selectedIndex < 0 || selectedIndex >= playerWeapon.Weapons.Count)
            {
                ammoSlotFrame.gameObject.SetActive(false);
                currentAmmoText.text = string.Empty;
                currentTotalAmmoText.text = string.Empty;
                return;
            }

            var currentWeapon = playerWeapon.Weapons[selectedIndex];
            if (!SlotUtility.TryGetWeaponType(currentWeapon, out var type) || !ownedWeapons.ContainsKey(type))
            {
                ammoSlotFrame.gameObject.SetActive(false);
                currentAmmoText.text = string.Empty;
                currentTotalAmmoText.text = string.Empty;
                return;
            }

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
