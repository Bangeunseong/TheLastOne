using _1.Scripts.Entity.Scripts.Player.Core;
using _1.Scripts.Manager.Subs;
using _1.Scripts.UI.Setting;
using _1.Scripts.UI.Inventory;
using UnityEngine;
using UnityEngine.UI;
using TMPro;


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

        [Header("크로스 헤어")] [SerializeField] private Image crossHairImage;

        [Header("무기 정보")] [SerializeField] private TextMeshProUGUI weaponName;
        [SerializeField] private TextMeshProUGUI ammoText;
        [SerializeField] private Image weaponImage;
        [SerializeField] private Image ammoImage;

        [Header("설정 버튼")] [SerializeField] private Button settingButton;
        [Header("인벤토리 버튼")] [SerializeField] private Button inventoryButton;

        private PlayerCondition playerCondition;

        public override void Init(UIManager manager)
        {
            base.Init(manager);
            if (settingButton != null)
            {
                settingButton.onClick.AddListener(OnSettingButtonClicked);
            }

            if (inventoryButton != null)
            {
                inventoryButton.onClick.AddListener(OnInventoryButtonClicked);
            }
        }

        public override void SetActive(bool active)
        {
            gameObject.SetActive(active);

            if (active)
            {
                if (playerCondition == null)
                {
                    playerCondition = FindObjectOfType<PlayerCondition>();
                }

                if (playerCondition != null)
                {
                    //playerCondition.OnHealthChanged += UpdateHealthSlider;
                    //playerCondition.OnStaminaChanged += UpdateStaminaSlider;
                    //playerCondition.OnLevelChanged += UpdateLevelUI;
                    playerCondition.OnDamage += UpdateStateUI;
                    UpdateStateUI();
                }
            }
            else
            {
                if (playerCondition != null)
                {
                    playerCondition.OnDamage -= UpdateStateUI;
                    playerCondition = null;
                }
            }
        }
        
        private void UpdateStateUI()
        {
            if (playerCondition == null) return;
            UpdateHealthSlider(playerCondition.CurrentHealth, playerCondition.MaxHealth);
            UpdateStaminaSlider(playerCondition.CurrentStamina, playerCondition.MaxStamina);
            UpdateLevelUI(playerCondition.Level);
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

        private void UpdateLevelUI(int level)
        {
            levelText.text = level.ToString();
        }
        
        private async void OnSettingButtonClicked()
        {
            await uiManager.ShowPopup<SettingUI>("SettingUI");
        }

        private async void OnInventoryButtonClicked()
        {
            await uiManager.ShowPopup<InventoryUI>("InventoryUI");
        }
    }
}