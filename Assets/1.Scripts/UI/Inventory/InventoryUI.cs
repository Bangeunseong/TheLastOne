using System.Collections.Generic;
using System.Linq;
using _1.Scripts.Entity.Scripts.Player.Core;
using _1.Scripts.Manager.Subs;
using _1.Scripts.UI.InGame;
using _1.Scripts.Util;
using _1.Scripts.Weapon.Scripts.Common;
using _1.Scripts.Weapon.Scripts.Grenade;
using _1.Scripts.Weapon.Scripts.Guns;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace _1.Scripts.UI.Inventory
{
    public class InventoryUI : UIBase
    {
        [Header("SlotType")]
        [SerializeField] private SlotType[] slotType;
        
        [Header("Slot Buttons")]
        [SerializeField] private List<Button> slotButtons;
        
        [Header("Preview Image")]
        [SerializeField] private List<GameObject> weaponPrefabs;
        [SerializeField] private Transform previewSpawnPoint;
        private Dictionary<SlotType, GameObject> previewPrefabs;
        private GameObject currentPreviewWeapon;

        [Header("StatsUI")]  
        [SerializeField] private Slider damageSlider;
        [SerializeField] private Slider rpmSlider;
        [SerializeField] private Slider recoilSlider;
        [SerializeField] private Slider weightSlider;
        [SerializeField] private Slider ammoSlider;

        [SerializeField] private TextMeshProUGUI damageText;
        [SerializeField] private TextMeshProUGUI rpmText;
        [SerializeField] private TextMeshProUGUI recoilText;
        [SerializeField] private TextMeshProUGUI weightText;
        [SerializeField] private TextMeshProUGUI ammoText;

        
        [Header("Description")]
        [SerializeField] private TextMeshProUGUI titleText;
        [SerializeField] private TextMeshProUGUI descriptionText;
        
        private PlayerCondition playerCondition;

        private int maxDamage = 1000;
        private float maxRPM = 100f;
        private float maxRecoil = 100f;
        private float maxWeight = 1f;
        private int maxAmmo;

        public override void Init(UIManager manager)
        {
            base.Init(manager);
            previewPrefabs = new Dictionary<SlotType, GameObject>();

            foreach (var prefab in weaponPrefabs)
            {
                var info = prefab.GetComponent<PreviewWeaponHandler>();
                if (info) previewPrefabs[info.slotType] = prefab;
            }
            Hide();
        }


        public override void ResetUI()
        {
            playerCondition = null;
            ClearSlotButtons();
            ClearPreviewWeapon();
            ClearText();
        }

        public override void Initialize(object param = null)
        {
            if (param is PlayerCondition newPlayerCondition)
            {
                playerCondition = newPlayerCondition;
                CalculateMaxStats();
                InitializeSlots();
                RefreshInventoryUI();
            }
        }
        
        private void ClearSlotButtons()
        {
            foreach (var button in slotButtons)
            {
                button.gameObject.SetActive(false);
                button.onClick.RemoveAllListeners();
            }
        }

        private void ClearPreviewWeapon()
        {
            if (currentPreviewWeapon != null)
            {
                Destroy(currentPreviewWeapon);
                currentPreviewWeapon = null;
            }
        }

        private void ClearText()
        {
            titleText.text = "";
            descriptionText.text = "";
        }

        private void CalculateMaxStats()
        {
            if (playerCondition == null) return;
            foreach (var w in playerCondition.Weapons)
            {
                var stat = SlotUtility.GetWeaponStat(w);
                maxDamage = Mathf.Max(maxDamage, stat.Damage);
                maxRPM = Mathf.Max(maxRPM, stat.Rpm);
                maxRecoil = Mathf.Max(maxRecoil, stat.Recoil);
                maxAmmo = Mathf.Max(maxAmmo, stat.MaxAmmoCountInMagazine);
            }
        }


        private void InitializeSlots()
        {
            if (playerCondition == null) return;

            for (int i = 0; i < slotType.Length && i < slotButtons.Count; i++)
            {
                var button = slotButtons[i];
                var slotWeapon = playerCondition.Weapons
                    .Where((w, idx) => playerCondition.AvailableWeapons[idx] && SlotUtility.IsMatchSlot(w, slotType[i]))
                    .FirstOrDefault();

                button.gameObject.SetActive(slotWeapon != null);
                if (slotWeapon != null)
                {
                    var label = button.GetComponentInChildren<TextMeshProUGUI>();
                    label.text = SlotUtility.GetWeaponName(slotWeapon);
                    int idx = playerCondition.Weapons.IndexOf(slotWeapon);
                    button.onClick.RemoveAllListeners();
                    button.onClick.AddListener(() => ShowWeapon(idx));
                }
            }
        }
        public void ShowWeapon(int index)
        {
            if (playerCondition == null || index < 0 || index >= playerCondition.Weapons.Count) return;
            
            var weapon = playerCondition.Weapons[index];
            var stat = SlotUtility.GetWeaponStat(weapon);
            
            UpdateStats(stat.Damage, stat.MaxAmmoCountInMagazine, stat.Rpm, stat.Recoil, stat.Weight);
            titleText.text = SlotUtility.GetWeaponName(weapon);
            descriptionText.text = titleText.text + " needs Description";

            ClearPreviewWeapon();
            var slot = SlotUtility.GetSlotTypeFromWeapon(weapon);
            if (previewPrefabs.TryGetValue(slot, out var prefab))
                currentPreviewWeapon = Instantiate(prefab, previewSpawnPoint.position, previewSpawnPoint.rotation);
        }


        
        public void RefreshInventoryUI()
        {
            CalculateMaxStats();
            InitializeSlots();
        }
        
        private void UpdateStats(int damage, int ammoCount, float rpm, float recoil, float weight)
        {
            damageSlider.value = (maxDamage > 0) ? damage / (float)maxDamage : 0f;
            rpmSlider.value = (maxRPM > 0) ? rpm / maxRPM : 0f;
            recoilSlider.value = (maxRecoil > 0) ? recoil / maxRecoil : 0f;
            ammoSlider.value = (maxAmmo > 0) ? ammoCount / (float)maxAmmo : 0f;
            weightSlider.value = (maxWeight > 0) ? weight / maxWeight : 0f;

            damageText.text = damage.ToString();
            rpmText.text = Mathf.RoundToInt(rpm).ToString();
            recoilText.text = Mathf.RoundToInt(recoil).ToString();
            ammoText.text = ammoCount.ToString();
            weightText.text = weight.ToString("F1");
        }
    }
}