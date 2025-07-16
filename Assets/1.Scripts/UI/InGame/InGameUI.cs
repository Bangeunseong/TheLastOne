using System.Collections;
using System.Collections.Generic;
using _1.Scripts.Entity.Scripts.Player.Core;
using _1.Scripts.Manager.Core;
using _1.Scripts.UI.InGame.Mission;
using _1.Scripts.UI.Inventory;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
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

        [Header("체력바")] [SerializeField] private Image healthSegmentPrefab;
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
        [SerializeField] private Image focusGaugeFrame;
        [SerializeField] private Image instinctGaugeImage;
        [SerializeField] private Image instinctGaugeFrame;
        [SerializeField] private Animator instinctEffectAnimator;
        [SerializeField] private Animator focusEffectAnimator;
        private Coroutine focusEffectCoroutine;
        private Coroutine instinctEffectCoroutine;

        [Header("크로스 헤어")] 
        [SerializeField] private Image crosshairImage;

        [Header("무기 정보")] 
        [SerializeField] private WeaponUI weaponUI;
        
        [field: Header("퀵 슬롯 UI")]
        [field: SerializeField] public QuickSlotUI QuickSlotUI { get; private set; }
        
        [field: Header("미션 UI")]
        [field: SerializeField] public MissionUI MissionUI { get; private set; }
        
        [field: Header("Distance UI")]
        [field: SerializeField] public DistanceUI DistanceUI { get; private set; }
        
        [field: Header("Inventory UI")]
        [field: SerializeField] public InventoryUI InventoryUI { get; private set; }
        
        [field: Header("Game Control")]
        [SerializeField] private Button exitGameButton;
        [SerializeField] private Button loadGameButton;
        
        [Header("ItemUseUI")]
        [SerializeField] private Image progressFillImage;
        [SerializeField] private TextMeshProUGUI messageText;
        [SerializeField] private float messageDuration = 3f;
        
        private PlayerCondition playerCondition;
        private bool isPaused = false;

        private void Awake()
        {
            if (progressFillImage != null)
                progressFillImage.enabled = false;
            if (!QuickSlotUI) QuickSlotUI = GetComponentInChildren<QuickSlotUI>(true);
            if (!MissionUI) MissionUI = GetComponentInChildren<MissionUI>(true);
            if (!DistanceUI) DistanceUI = GetComponentInChildren<DistanceUI>(true);
            if (!InventoryUI) InventoryUI = GetComponentInChildren<InventoryUI>(true);
        }

        private void Start()
        {
            exitGameButton.onClick.AddListener(() => CoreManager.Instance.MoveToIntroScene());
            loadGameButton.onClick.AddListener(() => CoreManager.Instance.ReloadGame());
        }

        public override void Init(UIManager manager)
        {
            base.Init(manager);
            
            playerCondition = CoreManager.Instance.gameManager.Player.PlayerCondition;
            
            var questManager = CoreManager.Instance.questManager;
            foreach (var kv in questManager.activeQuests)
            {
                var quest = kv.Value;
                MissionUI.AddMission(quest.data.questID, quest.CurrentObjective.data.description, quest.CurrentObjective.currentAmount, quest.CurrentObjective.data.requiredAmount);
            }
            Debug.Log($"InGameUI Init 호출 / 퀘스트 개수: {questManager.activeQuests.Count}");
            foreach(var kv in questManager.activeQuests)
                Debug.Log($"퀘스트ID: {kv.Key} / {kv.Value.CurrentObjective.data.description}");
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
                    if (animator != null)
                        healthSegmentAnimators.Add(animator);
                }
                healthSegmentPrefab.gameObject.SetActive(false);
            }
            prevhealth = playerCondition.CurrentHealth;
        }

        public override void SetActive(bool active)
        {
            gameObject.SetActive(active);
        }

        void Update()
        {
            if (playerCondition) { UpdateStateUI(); }
        }

        private void UpdateStateUI()
        {
            UpdateHealthSlider(playerCondition.CurrentHealth, playerCondition.MaxHealth);
            UpdateStaminaSlider(playerCondition.CurrentStamina, playerCondition.MaxStamina);
            UpdateArmorSlider(playerCondition.CurrentShield, playerCondition.MaxShield);
            UpdateLevelUI(playerCondition.Level);
            UpdateInstinct(playerCondition.CurrentInstinctGauge);
            UpdateFocus(playerCondition.CurrentFocusGauge);
            UpdateWeaponInfo();
        }

        private void UpdateHealthSlider(float current, float max)
        {
            if (healthText != null)
                healthText.text = $"{current}";
            if (maxHealthText != null)
                maxHealthText.text = $"{max}";

            int full = Mathf.FloorToInt(current / healthSegmentValue);
            float partial = (current % healthSegmentValue) / healthSegmentValue;
            for (int i = 0; i < healthSegments.Count; i++)
            {
                if (i < full)
                {
                    healthSegments[i].fillAmount = 1f;
                }
                else if (i == full)
                {
                    healthSegments[i].fillAmount = partial;
                }
                else
                {
                    healthSegments[i].fillAmount = 0f;
                }
            }

            if (healthBackgroundAnimator != null && current < prevhealth)
            {
                healthBackgroundAnimator.SetTrigger("Damaged");
            }

            if (current < prevhealth && healthSegmentAnimators != null)
            {
                for (int i = 0; i < full && i < healthSegmentAnimators.Count; i++)
                {
                    healthSegmentAnimators[i].SetTrigger("Damaged");
                }
            }
            prevhealth = current;
        }

        private void UpdateStaminaSlider(float current, float max)
        {
            float ratio = current / max;
            if (staminaSlider != null)
                staminaSlider.value = ratio;
            
            if (staminaAnimator != null)
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

        public void ShowItemProgress()
        {
            if (progressFillImage != null)
                progressFillImage.enabled = true;
        }

        public void HideItemProgress()
        {
            if (progressFillImage != null)
                progressFillImage.enabled = false;
        }

        public void UpdateItemProgress(float progress)
        {
            if (progressFillImage != null)
                progressFillImage.fillAmount = Mathf.Clamp01(progress);
        }

        public void ShowMessage(string message)
        {
            if (messageText != null)
                messageText.text = message;
        }
    }
}