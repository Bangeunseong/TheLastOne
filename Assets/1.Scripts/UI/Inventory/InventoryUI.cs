using System.Collections.Generic;
using System.Linq;
using _1.Scripts.Entity.Scripts.Player.Core;
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
    public class InventoryUI : MonoBehaviour
    {
        [Header("SlotType")]
        [SerializeField] private SlotType[] slotType;
        
        [Header("Slot Buttons")]
        public List<Button> slotButtons;
        
        [Header("Preview Image")]
        [SerializeField] private List<GameObject> weaponPrefabs;
        [SerializeField] private Transform previewSpawnPoint;
        private Dictionary<SlotType, GameObject> previewPrefabs;
        private GameObject currentPreviewWeapon;

        [Header("StatsUI")] public Slider damageSlider;
        public Slider rpmSlider;
        public Slider recoilSlider;
        public Slider weightSlider;
        public Slider ammoSlider;
        
        public TextMeshProUGUI damageText;
        public TextMeshProUGUI rpmText;
        public TextMeshProUGUI recoilText;
        public TextMeshProUGUI weightText;
        public TextMeshProUGUI ammoText;
        
        [Header("Description")]
        public TextMeshProUGUI titleText;
        public TextMeshProUGUI descriptionText;
        
        private PlayerCondition playerCondition;

        private int maxDamage = 1000;
        private float maxRPM = 100f;
        private float maxRecoil = 100f;
        private float maxWeight = 1f;
        private int maxAmmo;
        
        private void Awake()
        {
            previewPrefabs = new Dictionary<SlotType, GameObject>();

            foreach (var prefab in weaponPrefabs)
            {
                var info = prefab.GetComponent<PreviewWeaponHandler>();
                if (info) previewPrefabs[info.slotType] = prefab;
            }
        }


        public void ResetUI()
        {
            playerCondition = null;

            foreach (var button in slotButtons)
            {
                button.gameObject.SetActive(false);
                button.onClick.RemoveAllListeners();
            }
            titleText.text = "";
            descriptionText.text = "";
            if (currentPreviewWeapon != null)
            {
                Destroy(currentPreviewWeapon);
                currentPreviewWeapon = null;
            }
        }

        public void Initialize(PlayerCondition newPlayerCondition)
        {
            playerCondition = newPlayerCondition;
            CalculateMaxStats();
            InitializeSlots();
            RefreshInventoryUI();
        }
        
        private void CalculateMaxStats()
        {
            if (!playerCondition) return;
            foreach (var w in playerCondition.Weapons)
            {
                if (w is Gun gun)
                {
                    var s = gun.GunData.GunStat;
                    maxDamage = Mathf.Max(maxDamage, s.Damage);
                    maxRPM = Mathf.Max(maxRPM, s.Rpm);
                    maxRecoil = Mathf.Max(maxRecoil, s.Recoil);
                    maxAmmo = Mathf.Max(maxAmmo, s.MaxAmmoCountInMagazine);
                }
                else if (w is GrenadeLauncher gl)
                {
                    var s = gl.GrenadeData.GrenadeStat;
                    maxDamage = Mathf.Max(maxDamage, s.Damage);
                    maxRPM = Mathf.Max(maxRPM, s.Rpm);
                    maxRecoil = Mathf.Max(maxRecoil, s.Recoil);
                    maxAmmo = Mathf.Max(maxAmmo, s.MaxAmmoCountInMagazine);
                }
            }
        }

        private void InitializeSlots()
        {
            if (!playerCondition) return;
            var weapons = playerCondition.Weapons;
            var available = playerCondition.AvailableWeapons;
            
            for (int i = 0; i < slotType.Length && i < slotButtons.Count; i++)
            {
                var slotWeapon = weapons.Where((w, idx) => available[idx] && SlotUtility.IsMatchSlot(w, slotType[i]))
                    .FirstOrDefault();

                var button = slotButtons[i];
                if (slotWeapon)
                {
                    button.gameObject.SetActive(true);
                    var label = button.GetComponentInChildren<TextMeshProUGUI>();
                    if (label) label.text = SlotUtility.GetWeaponName(slotWeapon);

                    int idx = weapons.IndexOf(slotWeapon);
                    button.onClick.RemoveAllListeners();
                    button.onClick.AddListener(() => ShowWeapon(idx));
                }
                else
                {
                    button.gameObject.SetActive(false);
                }
            }

            for (int j = slotType.Length; j < slotButtons.Count; j++)
            {
                slotButtons[j].gameObject.SetActive(false);
            }
        }
        public void ShowWeapon(int index)
        {
            if (playerCondition == null) return;
            var weapons = playerCondition.Weapons;
            var available = playerCondition.AvailableWeapons;
            if (index < 0 || index >= weapons.Count || !available[index]) return;

            var weapon = weapons[index];
            if (weapon == null) return;

            var stat = SlotUtility.GetWeaponStat(weapon);
            UpdateStats(stat.Damage, stat.MaxAmmoCountInMagazine, stat.Rpm, stat.Recoil, stat.Weight);

            if (titleText != null) titleText.text = SlotUtility.GetWeaponName(weapon);
            if (descriptionText != null) descriptionText.text = titleText.text + " needs Description";
            
            if (currentPreviewWeapon != null)
                Destroy(currentPreviewWeapon);

            var slot = SlotUtility.GetSlotTypeFromWeapon(weapon);
            if (previewPrefabs.TryGetValue(slot, out var prefab))
                currentPreviewWeapon = Instantiate(prefab, previewSpawnPoint.position, previewSpawnPoint.rotation);
            else
                currentPreviewWeapon = null;
        }
        
        public void RefreshInventoryUI()
        {
            CalculateMaxStats();
            InitializeSlots();
        }
        
        private void UpdateStats(int damage, int ammoCount, float rpm, float recoil, float weight)
        {
            if (damageSlider != null) damageSlider.value = (maxDamage > 0) ? damage / (float)maxDamage : 0f;
            if (rpmSlider != null) rpmSlider.value = (maxRPM > 0) ? rpm / maxRPM : 0f;
            if (recoilSlider != null) recoilSlider.value = (maxRecoil > 0) ? recoil / maxRecoil : 0f;
            if (ammoSlider != null) ammoSlider.value = (maxAmmo > 0) ? ammoCount / (float)maxAmmo : 0f;
            if (weightSlider != null) weightSlider.value = (maxWeight > 0) ? weight / maxWeight : 0f;

            if (damageText != null) damageText.text = damage.ToString();
            if (rpmText != null) rpmText.text = Mathf.RoundToInt(rpm).ToString();
            if (recoilText != null) recoilText.text = Mathf.RoundToInt(recoil).ToString();
            if (ammoText != null) ammoText.text = ammoCount.ToString();
            if (weightText != null) weightText.text = weight.ToString("F1");
        }
    }
}