using System.Collections;
using System.Collections.Generic;
using System.Linq;
using _1.Scripts.Entity.Scripts.Player.Core;
using _1.Scripts.Manager.Core;
using _1.Scripts.UI.InGame.Mission;
using _1.Scripts.UI.Inventory;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Serialization;
using UIManager = _1.Scripts.Manager.Subs.UIManager;


namespace _1.Scripts.UI.InGame
{
    public class InGameUI : UIBase
    {
        [Header("플레이어 상태")] 
        [SerializeField] private Slider staminaSlider;
        [SerializeField] private Slider armorSlider;
        [SerializeField] private TextMeshProUGUI healthText;
        [SerializeField] private TextMeshProUGUI maxHealthText;
        [SerializeField] private TextMeshProUGUI levelText;

        [Header("체력바")] 
        [SerializeField] private Image healthSegmentPrefab;
        [SerializeField] private Transform healthSegmentContainer;
        [SerializeField] private int healthSegmentValue = 10;
        [SerializeField] private Animator healthBackgroundAnimator;
        private List<Animator> healthSegmentAnimators = new List<Animator>();
        private List<Image> healthSegments = new List<Image>();
        private float prevhealth;

        [Header("스테미나")] 
        [SerializeField] private Animator staminaAnimator;
        private float lackValue = 0.2f;
        
        [Header("게이지")] 
        [SerializeField] private Image focusGaugeImage;
        [SerializeField] private Image instinctGaugeImage;
        [SerializeField] private Animator instinctEffectAnimator;
        [SerializeField] private Animator focusEffectAnimator;
        private Coroutine focusEffectCoroutine;
        private Coroutine instinctEffectCoroutine;
        
        [field: Header("Handler")]
        [field: SerializeField] public InventoryHandler InventoryHandler { get; private set; }
        [field: SerializeField] public PauseHandler PauseHandler { get; private set; }
        
        [field: Header("Game Control")]
        [SerializeField] private Button exitGameButton;
        [SerializeField] private Button loadGameButton;
        
        [Header("ItemUseUI")]
        [SerializeField] private Image progressFillImage;
        [SerializeField] private TextMeshProUGUI messageText;
        [SerializeField] private float messageDuration = 3f;
        
        private PlayerCondition playerCondition;
        private bool isPaused = false;

        public override void Init(UIManager manager)
        {
            base.Init(manager);
            playerCondition = CoreManager.Instance.gameManager.Player.PlayerCondition;
            Initialize_HealthSegments();
            Hide();
        }
        
        public override void ResetUI()
        {
            playerCondition = null;
            Hide();
        }

        public override void Initialize(object param = null)
        {
            if (param is PlayerCondition newPlayerCondition)
            {
                playerCondition = newPlayerCondition;
                UpdateStateUI();
            }
        }

        private void Awake() { progressFillImage.enabled = false; }

        void Update() { if (playerCondition) { UpdateStateUI(); } }

        private void UpdateStateUI()
        {
            UpdateHealthSlider(playerCondition.CurrentHealth, playerCondition.MaxHealth);
            UpdateStaminaSlider(playerCondition.CurrentStamina, playerCondition.MaxStamina);
            UpdateArmorSlider(playerCondition.CurrentShield, playerCondition.MaxShield);
            UpdateLevelUI(playerCondition.Level);
            UpdateInstinct(playerCondition.CurrentInstinctGauge);
            UpdateFocus(playerCondition.CurrentFocusGauge);
        }
        public void Initialize_HealthSegments()
        {
            if (healthSegments.Count > 0) return;
            
            if (healthSegmentPrefab != null && healthSegmentContainer != null)
            {
                int count = playerCondition.MaxHealth / healthSegmentValue;
                for (int i = 0; i < count; i++)
                {
                    var segment = Instantiate(healthSegmentPrefab, healthSegmentContainer);
                    segment.type = Image.Type.Filled;
                    segment.fillAmount = 1f;
                    healthSegments.Add(segment);
                    segment.gameObject.SetActive(true);
                    var animator = segment.GetComponent<Animator>();
                    if (animator != null) healthSegmentAnimators.Add(animator);
                }
                healthSegmentPrefab.gameObject.SetActive(false);
            }
            prevhealth = playerCondition.CurrentHealth;
        }
        
        private void UpdateHealthSlider(float current, float max)
        {
            healthText.text = $"{current}";
            maxHealthText.text = $"{max}";
            int full = Mathf.FloorToInt(current / healthSegmentValue);
            float partial = (current % healthSegmentValue) / healthSegmentValue;
            for (int i = 0; i < healthSegments.Count; i++)
            {
                if (i < full) healthSegments[i].fillAmount = 1f;
                else if (i == full) healthSegments[i].fillAmount = partial;
                else healthSegments[i].fillAmount = 0f;
            }

            if (healthBackgroundAnimator != null && current < prevhealth) healthBackgroundAnimator.SetTrigger("Damaged");
            if (current < prevhealth && healthSegmentAnimators != null)
            {
                for (int i = 0; i < full && i < healthSegmentAnimators.Count; i++)
                    healthSegmentAnimators[i].SetTrigger("Damaged");
            }
            prevhealth = current;
        }

        private void UpdateStaminaSlider(float current, float max)
        {
            float ratio = current / max; 
            staminaSlider.value = ratio;
            staminaAnimator.SetBool("IsLack", ratio < lackValue);
        }

        private void UpdateArmorSlider(float current, float max)
        {
            if (armorSlider != null && max > 0)
            {
                armorSlider.enabled = true;
                armorSlider.value = current / max;
            }
            else if (armorSlider == null || max == 0 || current == 0)
                armorSlider.enabled = false;
        }

        private void UpdateInstinct(float value)
        {
            float instinct = Mathf.Clamp01(value);
            instinctGaugeImage.fillAmount = instinct;

            if (instinct >= 1f && instinctEffectCoroutine == null) instinctEffectCoroutine = StartCoroutine(InstinctEffectCoroutine());
            else if (instinct < 1f && instinctEffectCoroutine != null)
            {
                StopCoroutine(instinctEffectCoroutine);
                instinctEffectCoroutine = null;
            }
        }

        private void UpdateFocus(float value)
        {
            float focus = Mathf.Clamp01(value);
            focusGaugeImage.fillAmount = focus;
            
            if (focus >= 1f && focusEffectCoroutine == null) focusEffectCoroutine = StartCoroutine(FocusEffectCoroutine());
            else if (focus < 1f && focusEffectCoroutine != null)
            {
                StopCoroutine(focusEffectCoroutine);
                focusEffectCoroutine = null;
            }
        }

        private void UpdateLevelUI(int level) { levelText.text = $"Lvl. {level}"; }
        

        private IEnumerator FocusEffectCoroutine()
        {
            while (true)
            {
                focusEffectAnimator.SetTrigger("Full");
                focusEffectAnimator.ResetTrigger("Full");
                yield return new WaitForSeconds(0.5f);
            }
        }

        private IEnumerator InstinctEffectCoroutine()
        {
            while (true)
            {
                instinctEffectAnimator.ResetTrigger("Full");
                instinctEffectAnimator.SetTrigger("Full");
                yield return new WaitForSeconds(0.5f);
            }
        }

        public void ShowItemProgress() { progressFillImage.enabled = true; }

        public void HideItemProgress() { progressFillImage.enabled = false; }

        public void UpdateItemProgress(float progress) { progressFillImage.fillAmount = Mathf.Clamp01(progress); }

        public void ShowMessage(string message) { messageText.text = message; }
    }
}