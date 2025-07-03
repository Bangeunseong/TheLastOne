using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using _1.Scripts.Weapon.Scripts.Common;
using _1.Scripts.Weapon.Scripts.Grenade;
using _1.Scripts.Weapon.Scripts.Guns;

namespace _1.Scripts.UI.InGame
{
    public enum SlotType
    {
        Main,
        Pistol,
        HackGun,
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
        
        [Header("컬러 세팅")] [SerializeField] private Color selectedAmmoColor = Color.white;
        [SerializeField] private Color selectedColor = Color.black;
        [SerializeField] private Image[] selectedSlotImage;
        [SerializeField] private float idleAlpha = 0.5f;
        [SerializeField] private float selectedSlotAlpha = 1f;

        [Header("애니메이터")] [SerializeField] private Animator panelAnimator;
        [SerializeField] private float panelHideDelay = 3f;
        private Coroutine hideCoroutine;

        private int lastSelectedIndex = -1;
        
        private void Start()
        {
            int n = slotTransform.Length;
            targetScales = new Vector3[n];
            
            for (int i = 0; i < n; i++)
            {
                slotTransform[i].localScale = normalScale;
                targetScales[i] = normalScale;

                if (selectedSlotImage != null && i < selectedSlotImage.Length && selectedSlotImage[i] != null)
                {
                    var color = selectedSlotImage[i].color;
                    color.a = idleAlpha;
                    selectedSlotImage[i].color = color;
                }
                
                if (slotAnimator != null && i < slotAnimator.Length && slotAnimator[i] != null)
                {
                    slotAnimator[i].enabled = false;
                }
            }
            
            if (panelAnimator != null)
                panelAnimator.Play("Hidden", 0, 1f);
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
        
        
        public void Refresh(List<BaseWeapon> weapons, List<bool> available, int selectedIndex)
        {
            BaseWeapon selectedWeapon = (selectedIndex >= 0 && selectedIndex < weapons.Count && available[selectedIndex]) ? weapons[selectedIndex] : null;
            int currentSlot = -1;
            
            bool selectionChanged = selectedIndex != lastSelectedIndex;
            lastSelectedIndex = selectedIndex;
            
            for (int i = 0; i < slotType.Length; i++)
            {      
                BaseWeapon slotWeapon = weapons.Where((w, idx) => available[idx] && IsMatchSlot(w, slotType[i]))
                    .FirstOrDefault();

                if (slotWeapon != null)
                {
                    //slotImage[i].sprite = slotWeapon.IconSprite;
                    slotImage[i].color  = Color.white;
                }
                else
                {
                    slotImage[i].color = Color.gray;
                }
                slotText[i].text = slotWeapon != null ? GetWeaponName(slotWeapon) : string.Empty;

                bool hasAmmo = false;
                
                if (slotWeapon is Gun g)
                {
                    slotAmmoText[i].text = $"{g.CurrentAmmoCountInMagazine}/{g.CurrentAmmoCount}";
                    slotAmmoText[i].color = selectedColor;
                    hasAmmo = true;
                }
                else if (slotWeapon is GrenadeLauncher gl)
                {
                    slotAmmoText[i].text = $"{gl.CurrentAmmoCountInMagazine}/{gl.CurrentAmmoCount}";
                    slotAmmoText[i].color = selectedAmmoColor;
                    hasAmmo = true;
                }
                else
                {
                    slotAmmoText[i].text = string.Empty;
                }

                bool isSelected = (slotWeapon != null && slotWeapon == selectedWeapon);
                if (isSelected) 
                    currentSlot = i;
                
                slotText[i].enabled = isSelected;
                slotAmmoText[i].enabled = isSelected && hasAmmo;

                if (selectedSlotImage != null && i < selectedSlotImage.Length && selectedSlotImage[i] != null)
                {
                    var color = selectedSlotImage[i].color;
                    color.a = isSelected ? selectedSlotAlpha : idleAlpha;
                    selectedSlotImage[i].color = color;
                }
                
                targetScales[i] = isSelected ? selectedScale : normalScale;
            }
            BaseWeapon sel = (selectedIndex >= 0 && selectedIndex < weapons.Count && available[selectedIndex])
                ? weapons[selectedIndex] : null;

            if (sel is Gun gun)
                ammoUI.UpdateAmmoUI(gun.CurrentAmmoCountInMagazine);
            else if (sel is GrenadeLauncher gl)
                ammoUI.UpdateAmmoUI(gl.CurrentAmmoCountInMagazine);
            else
                ammoUI.UpdateAmmoUI(0);
            
            if (selectionChanged)
            {
                if (currentSlot >= 0 && currentSlot < slotAnimator.Length && slotAnimator[currentSlot] != null)
                {
                    slotAnimator[currentSlot].enabled = false;
                    slotAnimator[currentSlot].enabled = true;
                }
                
                if (panelAnimator != null)
                {
                    panelAnimator.ResetTrigger("Hide");
                    panelAnimator.SetTrigger("Show");
                    if (hideCoroutine != null) StopCoroutine(hideCoroutine);
                    hideCoroutine = StartCoroutine(HidePanelCoroutine());
                }
            }
        }
        private bool IsMatchSlot(BaseWeapon w, SlotType slot)
        {
            switch (slot)
            {
                case SlotType.Main:
                    return w is Gun g1 && g1.GunData.GunStat.Type == WeaponType.Rifle;
                case SlotType.Pistol:
                    return w is Gun g2 && g2.GunData.GunStat.Type == WeaponType.Pistol;
                //case SlotType.Hack:
                    // return w is Gun g3 && g3.GunData.GunStat.Type == WeaponType.HackGun;
                case SlotType.GrenadeLauncher:
                    return w is GrenadeLauncher;
                default:
                    return false;
            }
        }

        private IEnumerator HidePanelCoroutine()
        {
            yield return new WaitForSeconds(panelHideDelay);
            if (panelAnimator != null)
                panelAnimator.ResetTrigger("Show");
                panelAnimator.SetTrigger("Hide");
            hideCoroutine = null;
        }

        private string GetWeaponName(BaseWeapon w)
        {
            if (w is Gun g) return g.GunData.GunStat.Type.ToString();
            if (w is GrenadeLauncher gl)  return gl.GrenadeData.GrenadeStat.Type.ToString();
            return w.GetType().Name;
        }
    }
}
