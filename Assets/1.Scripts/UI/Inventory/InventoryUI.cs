using System.Collections;
using System.Collections.Generic;
using System.Linq;
using _1.Scripts.Entity.Scripts.Player.Core;
using _1.Scripts.Manager.Core;
using _1.Scripts.Manager.Subs;
using _1.Scripts.Util;
using _1.Scripts.Weapon.Scripts.Common;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace _1.Scripts.UI.Inventory
{
    public class InventoryUI : UIBase
    {
        [Header("UI")]
        [SerializeField] private GameObject panel;
        [SerializeField] private Animator panelAnimator;
        
        [Header("Preview Image")]
        [SerializeField] private List<Button> weaponButtons;
        
        [Header( "Preview")]
        [SerializeField] private Transform previewSpawnPoint;
        private Dictionary<WeaponType, Button> weaponButtonMap;
        private Dictionary<WeaponType, GameObject> weaponPreviewMap;

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
        private PlayerWeapon playerWeapon;

        private Dictionary<WeaponType, BaseWeapon> ownedWeapons = new();
        private int maxDamage = 1000;
        private float maxRPM = 100f;
        private float maxRecoil = 100f;
        private float maxWeight = 1f;
        private int maxAmmo;

        public override void Initialize(UIManager manager, object param = null)
        {
            base.Initialize(manager, param);
            weaponButtonMap = new();
            for (int i = 0; i < weaponButtons.Count; i++) weaponButtonMap[(WeaponType)i] = weaponButtons[i];

            weaponPreviewMap = new();
            foreach (Transform child in previewSpawnPoint)
            {
                var handler = child.GetComponent<PreviewWeaponHandler>();
                if (handler) { weaponPreviewMap[handler.weaponType] = child.gameObject; }
                else { Debug.LogWarning($"PreviewWeaponHandler가 없습니다: {child.name}"); }
                child.gameObject.SetActive(false);
            }
            playerCondition = CoreManager.Instance.gameManager.Player.PlayerCondition;
            playerWeapon = CoreManager.Instance.gameManager.Player.PlayerWeapon;
            SyncOwnedWeapons();
            gameObject.SetActive(false);
        }

        public override void Show()
        {
            base.Show();
            panelAnimator?.Rebind();
            panelAnimator?.Play("Panel In");
            
            RefreshInventoryUI();

            playerCondition.OnDisablePlayerMovement();
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
        public override void Hide() 
        {
            if (panelAnimator && panelAnimator.isActiveAndEnabled)
            {
                panelAnimator.Rebind();
                panelAnimator.Play("Panel Out");
                StartCoroutine(HideCoroutine());
            }
            else { base.Hide(); }
            
            playerCondition.OnEnablePlayerMovement();
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        public override void ResetUI()
        {
            foreach (var button in weaponButtonMap.Values)
            {
                button.gameObject.SetActive(false);
                button.onClick.RemoveAllListeners();
                var label = button.GetComponentInChildren<TextMeshProUGUI>();
                if (label) label.text = string.Empty;
            }
            foreach (var go in weaponPreviewMap.Values) go.SetActive(false);
            titleText.text = descriptionText.text = damageText.text = rpmText.text = recoilText.text = weightText.text = ammoText.text = string.Empty;
            damageSlider.value = rpmSlider.value = recoilSlider.value = weightSlider.value = ammoSlider.value = 0f;
        }

        private void SyncOwnedWeapons()
        {
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
        }

        private void CalculateMaxStats()
        {
            maxDamage = 0;
            maxRPM = 0f;
            maxRecoil = 0f;
            maxWeight = 0f;
            maxAmmo = 0;
            
            if (!playerWeapon) return;

            foreach (var stat in ownedWeapons.Values.Select(SlotUtility.GetWeaponStat))
            {
                maxDamage = Mathf.Max(maxDamage, stat.Damage);
                maxRPM = Mathf.Max(maxRPM, stat.Rpm);
                maxRecoil = Mathf.Max(maxRecoil, stat.Recoil);
                maxWeight = Mathf.Max(maxWeight, stat.Weight);
                maxAmmo = Mathf.Max(maxAmmo, stat.MaxAmmoCountInMagazine);
            }
        }

        private void InitializeWeaponButtons()
        {
            foreach (var (weaponType, button) in weaponButtonMap)
            {
                button.onClick.RemoveAllListeners();
                bool owned = ownedWeapons.ContainsKey(weaponType);
                button.gameObject.SetActive(owned);
                if (!owned) continue;
                BaseWeapon weapon = ownedWeapons[weaponType];
                var label = button.GetComponentInChildren<TextMeshProUGUI>();
                if (label) label.text = SlotUtility.GetWeaponName(weapon);
                button.onClick.AddListener(() => ShowWeapon(weaponType));
            }
        }

        private void ShowWeapon(WeaponType weaponType)
        {
            foreach (var go in weaponPreviewMap.Values)
                go.SetActive(false);
            
            if (!ownedWeapons.TryGetValue(weaponType, out var weapon)) return;
            var stat = SlotUtility.GetWeaponStat(weapon);
            UpdateStats(stat.Damage, stat.MaxAmmoCountInMagazine, stat.Rpm, stat.Recoil, stat.Weight);
            titleText.text = SlotUtility.GetWeaponName(weapon);
            descriptionText.text = titleText.text + " needs Description";
            if (weaponPreviewMap.TryGetValue(weaponType, out var previewGo))
                previewGo.SetActive(true);
        }
        
        public void RefreshInventoryUI()
        {
            SyncOwnedWeapons();
            CalculateMaxStats();
            InitializeWeaponButtons();
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

        private IEnumerator HideCoroutine()
        {
            yield return new WaitForSeconds(0.5f);
            base.Hide();
        }
    }
}