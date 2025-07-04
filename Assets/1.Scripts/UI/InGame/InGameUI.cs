using System.Collections;
using System.Collections.Generic;
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
        [Header("플레이어 상태")] 
        [SerializeField] private Slider healthSlider;
        [SerializeField] private Slider staminaSlider;
        [SerializeField] private Slider armorSlider;
        [SerializeField] private TextMeshProUGUI healthText;
        [SerializeField] private TextMeshProUGUI staminaText;
        [SerializeField] private TextMeshProUGUI levelText;

        [Header("게이지")] 
        [SerializeField] private Image focusGaugeImage;
        [SerializeField] private Image focusGaugeFrame;
        [SerializeField] private Image instinctGaugeImage;
        [SerializeField] private Image instinctGaugeFrame;
        [SerializeField] private Image instinctGaugeEffect;
        [SerializeField] private Image focusGaugeEffect;
        [SerializeField] private Animator instinctEffectAnimator;
        [SerializeField] private Animator focusEffectAnimator;
        private Coroutine focusEffectCoroutine;
        private Coroutine instinctEffectCoroutine;

        [Header("크로스 헤어")] 
        [SerializeField] private Image crosshairImage;

        [Header("무기 정보")] 
        [SerializeField] private WeaponUI weaponUI;


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
            //UpdateArmorSlider(playerCondition.CurrentArmor, playerCondition.MaxArmor);
            UpdateLevelUI(playerCondition.Level);
            UpdateInstinct(playerCondition.CurrentInstinctGauge);
            UpdateFocus(playerCondition.CurrentFocusGauge);
            UpdateWeaponInfo();
        }

        private void UpdateHealthSlider(float current, float max)
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

        private void UpdateArmorSlider(float current, float max)
        {
            if (armorSlider != null && max > 0)
            {
                armorSlider.enabled = true;
                armorSlider.value = current / max;
            }
            else if (armorSlider == null || max == 0)
                armorSlider.enabled = false;
        }

        private void UpdateInstinct(float value)
        {
            float instinct = Mathf.Clamp01(value);

            if (instinctGaugeImage != null)
                instinctGaugeImage.fillAmount = instinct;

            if (instinct >= 1f && instinctEffectCoroutine == null)
            {
                instinctEffectCoroutine = StartCoroutine(InstinctEffectCoroutine());
            }
            else if (instinct < 1f && instinctEffectCoroutine != null)
            {
                StopCoroutine(instinctEffectCoroutine);
                instinctEffectCoroutine = null;
            }
        }

        private void UpdateFocus(float value)
        {
            float focus = Mathf.Clamp01(value);

            if (focusGaugeImage != null)
                focusGaugeImage.fillAmount = focus;
            
            if (focus >= 1f && focusEffectCoroutine == null)
            {
                focusEffectCoroutine = StartCoroutine(FocusEffectCoroutine());
            }
            else if (focus < 1f && focusEffectCoroutine != null)
            {
                StopCoroutine(focusEffectCoroutine);
                focusEffectCoroutine = null;
            }
        }

        private void UpdateLevelUI(int level)
        {
            levelText.text = $"Lvl. {level}";
        }

        private void UpdateWeaponInfo()
        {
           var weapons = playerCondition.Weapons;
           var available = playerCondition.AvailableWeapons;
           int idx = playerCondition.EquippedWeaponIndex;
           
           weaponUI.Refresh(weapons, available, idx);
        }

        private IEnumerator FocusEffectCoroutine()
        {
            while (true)
            {
                focusEffectAnimator.SetTrigger("Full");
                focusEffectAnimator.ResetTrigger("Full");
                yield return new WaitForSeconds(2f);
            }
        }

        private IEnumerator InstinctEffectCoroutine()
        {
            while (true)
            {
                instinctEffectAnimator.ResetTrigger("Full");
                instinctEffectAnimator.SetTrigger("Full");
                yield return new WaitForSeconds(2f);
            }
        }
    }
}