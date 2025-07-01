using _1.Scripts.Entity.Scripts.Player.Core;
using _1.Scripts.UI.Setting;
using _1.Scripts.UI.Inventory;
using _1.Scripts.Weapon.Scripts.Common;
using _1.Scripts.Weapon.Scripts.Grenade;
using _1.Scripts.Weapon.Scripts.Guns;
using Michsky.UI.Shift;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UIManager = _1.Scripts.Manager.Subs.UIManager;


namespace _1.Scripts.UI.InGame
{
    public class InGameUI : UIBase
    {
        [Header("플레이어 상태")] [SerializeField] private Slider healthSlider;
        [SerializeField] private Slider staminaSlider;
        [SerializeField] private TextMeshProUGUI healthText;
        [SerializeField] private TextMeshProUGUI staminaText;
        [SerializeField] private TextMeshProUGUI levelText;

        [Header("포커스 게이지")] [SerializeField] private Image focusGaugeImage;

        [Header("인스팅트 게이지")] [SerializeField] private Image instinctGaugeImage;

        [Header("크로스 헤어")] [SerializeField] private Image crosshairImage;

        [Header("무기 정보")] [SerializeField] private TextMeshProUGUI weaponNameText;
        [SerializeField] private TextMeshProUGUI ammoText;
        [SerializeField] private Image weaponImage;
        [SerializeField] private Image ammoImage;


        private PlayerCondition playerCondition;
        private bool isPaused = false;


        public override void Init(UIManager manager)
        {
            base.Init(manager);

            playerCondition = FindObjectOfType<PlayerCondition>();
            if (playerCondition != null)
            {
                UpdateStateUI();
            }
        }

        public override void SetActive(bool active)
        {
            gameObject.SetActive(active);
        }

        void Update()
        {
            if (playerCondition != null)
            {
                UpdateStateUI();
            }
        }

        private void UpdateStateUI()
        {
            if (playerCondition == null) return;
            UpdateHealthSlider(playerCondition.CurrentHealth, playerCondition.MaxHealth);
            UpdateStaminaSlider(playerCondition.CurrentStamina, playerCondition.MaxStamina);
            UpdateLevelUI(playerCondition.Level);
            UpdateInstinct(playerCondition.CurrentInstinctGauge);
            UpdateFocus(playerCondition.CurrentFocusGauge);
            UpdateWeaponInfo();
        }

        public void UpdateHealthSlider(float current, float max)
        {
            if (healthSlider != null)
                healthSlider.value = current / max;

            if (healthText != null)
                healthText.text = current + "/" + max;
        }

        private void UpdateStaminaSlider(float current, float max)
        {
            if (staminaSlider != null)
                staminaSlider.value = current / max;

            if (staminaText != null)
                staminaText.text = current + "/" + max;
        }

        public void UpdateInstinct(float value)
        {
            if (instinctGaugeImage != null)
                instinctGaugeImage.fillAmount = Mathf.Clamp01(value);
        }

        public void UpdateFocus(float value)
        {
            if (focusGaugeImage != null)
                focusGaugeImage.fillAmount = Mathf.Clamp01(value);
        }

        private void UpdateLevelUI(int level)
        {
            levelText.text = level.ToString();
        }

        public void UpdateWeaponInfo()
        {
            int idx = playerCondition.EquippedWeaponIndex;
            if (idx >= 0 && idx < playerCondition.Weapons.Count && playerCondition.AvailableWeapons[idx])
            {
                BaseWeapon baseWeapon = playerCondition.Weapons[idx];
                string name;
                int left = 0;
                int max = 0;

                if (baseWeapon is Gun gun)
                {
                    name = gun.GunData.GunStat.Type.ToString();
                    left = gun.CurrentAmmoCountInMagazine;
                    max = gun.GunData.GunStat.MaxAmmoCountInMagazine;
                }
                else if (baseWeapon is GrenadeLauncher grenadeLauncher)
                {
                    name = grenadeLauncher.GrenadeData.GrenadeStat.Type.ToString();
                    left = grenadeLauncher.CurrentAmmoCountInMagazine;
                    max = grenadeLauncher.GrenadeData.GrenadeStat.MaxAmmoCountInMagazine;
                }
                else
                {
                    name = baseWeapon.GetType().Name;
                }
                
                weaponNameText.text = name;
                ammoText.text = $"{left}/{max}";
            }
            else
            {
                weaponNameText.text = "empty";
                ammoText.text = "empty";
            }
        }
    }
}