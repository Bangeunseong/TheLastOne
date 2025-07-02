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
        [Header("SlotType")]
        [SerializeField] private SlotType[] slotType;
        
        [Header("SlotTransform")]
        [SerializeField] private RectTransform[] slotTransform;
 
        [Header("WeaponInfo")]
        [SerializeField] private Image[] slotImage; 
        [SerializeField] private TextMeshProUGUI[] slotText;
        [SerializeField] private TextMeshProUGUI[] slotAmmoText;
        [SerializeField] private Image[] slotAmmoImage;

        [Header("Scale 세팅")]
        [SerializeField] private Vector2 normalSize = new Vector2(200, 80);
        [SerializeField] private Vector2 selectedSize = new Vector2(300, 120);
        [SerializeField] private float scaleSpeed = 10f;
        private float[] targetHeights;
        private float[] targetWidths;
        
        [Header("컬러 세팅")] [SerializeField] private Color selectedAmmoColor = Color.white;
        [SerializeField] private Color selectedColor = Color.black;

        private Vector3[] targetScales;
        
        private void Start()
        {
            int n = slotTransform.Length;
            targetScales = new Vector3[n];
            for (int i = 0; i < n; i++)
            {
                slotTransform[i].localScale = Vector3.one;
                targetScales[i] = Vector3.one;
            }
        }
        
        private void Update()
        {
            bool needsLayout = false;
            for (int i = 0; i < slotTransform.Length; i++)
            {
                Vector3 cur = slotTransform[i].localScale;
                Vector3 tgt = targetScales[i];
                if ((cur - tgt).sqrMagnitude > 0.01f)
                {
                    slotTransform[i].localScale = Vector3.Lerp(cur, tgt, Time.deltaTime * scaleSpeed);
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
                
                if (slotWeapon != null)
                {
                    slotText[i].text = $"{GetWeaponName(slotWeapon)}";
                    slotText[i].color = selectedColor;

                    if (slotWeapon is Gun g)
                    {
                        slotAmmoText[i].text = $"{g.CurrentAmmoCountInMagazine}/{g.CurrentAmmoCount}";
                        slotAmmoText[i].color = selectedAmmoColor;
                    }
                    else if (slotWeapon is GrenadeLauncher gl)
                    {
                        slotAmmoText[i].text = $"{gl.CurrentAmmoCountInMagazine}/{gl.CurrentAmmoCount}";
                        slotAmmoText[i].color = selectedAmmoColor;
                    }
                    else
                    {
                        slotAmmoText[i].text = string.Empty;
                    }
                }
                else
                {
                    slotText[i].text = "EMPTY";
                    slotText[i].color = Color.gray;
                    slotAmmoText[i].text = string.Empty;
                    slotAmmoImage[i].enabled = false;
                }

                bool isSelected = (slotWeapon != null && slotWeapon == selectedWeapon);
                slotText[i].enabled = isSelected;
                slotAmmoText[i].enabled = isSelected;
                if (slotAmmoImage[i] != null)
                {
                    slotAmmoImage[i].enabled = isSelected;
                }

                Vector2 size = isSelected ? selectedSize : normalSize;
                targetScales[i] = new Vector3(size.x / normalSize.x, size.y / normalSize.y, 1f);
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

        private string GetWeaponName(BaseWeapon w)
        {
            if (w is Gun g) return g.GunData.GunStat.Type.ToString();
            if (w is GrenadeLauncher gl)  return gl.GrenadeData.GrenadeStat.Type.ToString();
            return w.GetType().Name;
        }
    }
}
