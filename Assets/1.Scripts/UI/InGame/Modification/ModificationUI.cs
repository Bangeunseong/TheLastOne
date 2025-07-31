using System.Collections.Generic;
using System.Linq;
using _1.Scripts.Entity.Scripts.Player.Core;
using _1.Scripts.Manager.Core;
using _1.Scripts.Manager.Subs;
using _1.Scripts.UI.Inventory;
using _1.Scripts.Util;
using _1.Scripts.Weapon.Scripts.Common;
using _1.Scripts.Weapon.Scripts.Grenade;
using _1.Scripts.Weapon.Scripts.Guns;
using _1.Scripts.Weapon.Scripts.Hack;
using _1.Scripts.Weapon.Scripts.Melee;
using _1.Scripts.Weapon.Scripts.WeaponDetails;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace _1.Scripts.UI.InGame.Modification
{
    public class ModificationUI : UIBase
    {
        private static readonly WeaponType[] SlotOrder = new[]
        {
            WeaponType.Rifle,
            WeaponType.Pistol,
            WeaponType.GrenadeLauncher,
            WeaponType.HackGun
        };
        
        [Header("Preview")] 
        [SerializeField] private Transform previewSpawnPoint;
        private Dictionary<WeaponType, GameObject> weaponPreviewMap;
        private PreviewWeaponHandler previewWeaponHandler;
        
        [Header("Part Button")]
        [SerializeField] private List<Button> partButtons;
        [SerializeField] private List<TextMeshProUGUI> partButtonTexts;
        [SerializeField] private List<Image> partButtonImages;
        private readonly PartType[] allPartTypes = { PartType.Sight, PartType.FlameArrester, PartType.Suppressor, PartType.Silencer, PartType.ExtendedMag };
        
        [Header("Weapon & Part Name")]
        [SerializeField] private TextMeshProUGUI weaponNameText;
        [SerializeField] private TextMeshProUGUI partNameText;
        
        [Header("Part Highlight Material")]
        [SerializeField] private Material partHighlightMaterial;
        [SerializeField] private Material partNormalMaterial;
        
        [Header("Stat")]
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
        
        [Header("Required")]
        [SerializeField] private TextMeshProUGUI requiredText;
        
        [Header("Apply Modal")] 
        [SerializeField] private GameObject applyModal;
        [SerializeField] private Button applyButton;
        [SerializeField] private Button confirmButton;
        [SerializeField] private Button cancelButton;

        [Header("Buttons")]
        [SerializeField] private Button prevButton;
        [SerializeField] private Button nextButton;
        
        private Dictionary<WeaponType, BaseWeapon> ownedWeapons = new();
        private int currentWeaponIdx = 0;
        private WeaponType CurrentWeaponType => (currentWeaponIdx >= 0 && currentWeaponIdx < SlotOrder.Length) ? SlotOrder[currentWeaponIdx] : SlotOrder[0];
        private BaseWeapon CurrentWeapon => ownedWeapons.GetValueOrDefault(CurrentWeaponType);

        private PartType? selectedPartType;
        private WeaponPartData selectedPartData;

        private PlayerCondition playerCondition;
        private PlayerWeapon playerWeapon;

        private PartType? lastHighlightedPartType;
        private Material lastSelectedPartMaterial = null;

        public override void Initialize(UIManager manager, object param = null)
        {
            base.Initialize(manager, param);

            playerCondition = CoreManager.Instance.gameManager.Player.PlayerCondition;
            playerWeapon = CoreManager.Instance.gameManager.Player.PlayerWeapon;

            weaponPreviewMap = new();
            foreach (Transform child in previewSpawnPoint)
            {
                var handler = child.GetComponent<PreviewWeaponHandler>();
                if (handler)
                {
                    weaponPreviewMap[handler.weaponType] = child.gameObject;
                }
                child.gameObject.SetActive(false);
            }

            applyButton.onClick.AddListener(OnApplyButtonClicked);
            confirmButton.onClick.AddListener(OnApplyConfirmed);
            cancelButton.onClick.AddListener(HideModal);
            prevButton.onClick.AddListener(OnPrevWeaponClicked);
            nextButton.onClick.AddListener(OnNextWeaponClicked);

            HideModal();
            ResetUI();
            Refresh();
        }
        public override void Show()
        {
            base.Show();
            playerCondition.OnDisablePlayerMovement();
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            Refresh();
        }
        public override void Hide()
        {
            base.Hide();
            UnhighlightPart();
            playerCondition.OnEnablePlayerMovement();
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
        public override void ResetUI()
        {
            foreach (var go in weaponPreviewMap.Values)
                go.SetActive(false);
            previewWeaponHandler = null;
            selectedPartType = null;
            selectedPartData = null;
            UnhighlightPart();
            ResetStatUI();
            HideModal();
            currentWeaponIdx = 0;
            ResetPartButtons();
            
            if (weaponNameText) weaponNameText.text = "";
            if (partNameText) partNameText.text = "";
        }
        private void Refresh()
        {
            SyncOwnedWeapons();
            if (!ownedWeapons.ContainsKey(CurrentWeaponType))
                SetCurrentWeaponIdxToFirstOwned();
            ShowWeaponPreview(CurrentWeaponType);
            selectedPartType = null;
            selectedPartData = null;
            UnhighlightPart();
            GeneratePartSlots();
            UpdateStatUI();
            UpdateNameUI();
        }
        private void SyncOwnedWeapons()
        {
            ownedWeapons.Clear();

            if (!playerWeapon) return;
            foreach (var type in SlotOrder)
            {
                if (type == WeaponType.Punch) continue;
                if (playerWeapon.Weapons.TryGetValue(type, out var weapon)
                    && weapon
                    && playerWeapon.AvailableWeapons.TryGetValue(type, out var unlocked)
                    && unlocked)
                {
                    ownedWeapons[type] = weapon;
                }
            }
        }
        
        private void SetCurrentWeaponIdxToFirstOwned()
        {
            for (int i = 0; i < SlotOrder.Length; i++)
            {
                if (!ownedWeapons.ContainsKey(SlotOrder[i])) continue;
                currentWeaponIdx = i;
                return;
            }
            currentWeaponIdx = 0;
        }
        
        private void ShowWeaponPreview(WeaponType weaponType)
        {
            foreach (var go in weaponPreviewMap.Values)
                go.SetActive(false);
            if (weaponPreviewMap.TryGetValue(weaponType, out var previewGo))
            {
                previewGo.SetActive(true);
                previewWeaponHandler = previewGo.GetComponent<PreviewWeaponHandler>();
            }
            else { previewWeaponHandler = null; }
            lastHighlightedPartType = null;
            lastSelectedPartMaterial = null;
        }

        private void GeneratePartSlots()
        {
            for (int i = 0; i < partButtons.Count; i++)
            {
                partButtons[i].gameObject.SetActive(false);
                if (partButtonTexts.Count > i && partButtonTexts[i])
                    partButtonTexts[i].text = "";
                if (partButtonImages.Count > i && partButtonImages[i])
                    partButtonImages[i].sprite = null;
                partButtons[i].onClick.RemoveAllListeners();
            }

            if (!CurrentWeapon)
            {
                applyButton.gameObject.SetActive(false);
                return;
            }

            var partTypeList = GetPartTypes(CurrentWeaponType);

            bool anyPart = false;
            for (int i = 0; i < allPartTypes.Length; i++)
            {
                var partType = allPartTypes[i];
                int uiIdx = i;

                bool enabled = partTypeList.Contains(partType);
                partButtons[uiIdx].gameObject.SetActive(enabled);

                if (partButtonTexts.Count > uiIdx && partButtonTexts[uiIdx])
                    partButtonTexts[uiIdx].text = partType.ToString();
                if (partButtonImages.Count > uiIdx && partButtonImages[uiIdx])
                    partButtonImages[uiIdx].sprite = GetPartSprite(partType);

                partButtons[uiIdx].onClick.RemoveAllListeners();
                if (!enabled) continue;
                partButtons[uiIdx].onClick.AddListener(() => OnPartButtonClicked(partType));
                anyPart = true;
            }

            applyButton.gameObject.SetActive(!anyPart);
            requiredText.text = anyPart
                ? "Required Part:"
                : (CurrentWeaponType == WeaponType.Pistol ? "Required Condition:" : "Required Part:");
        }
        private void ResetPartButtons()
        {
            for (int i = 0; i < partButtons.Count; i++)
            {
                partButtons[i].gameObject.SetActive(false);
                if (partButtonTexts.Count > i && partButtonTexts[i])
                    partButtonTexts[i].text = "";
                if (partButtonImages.Count > i && partButtonImages[i])
                    partButtonImages[i].sprite = null;
                partButtons[i].onClick.RemoveAllListeners();
            }
        }
        
        private List<PartType> GetPartTypes(WeaponType weaponType)
        {
            switch (weaponType)
            {
                case WeaponType.Rifle:
                    return new List<PartType>{ PartType.FlameArrester,  PartType.Silencer, PartType.Suppressor, PartType.Sight };
                case WeaponType.GrenadeLauncher:
                    return new List<PartType>{ PartType.Sight };
                case WeaponType.HackGun:
                    return new List<PartType>{ PartType.ExtendedMag };
                case WeaponType.Pistol:
                case WeaponType.Punch:
                case WeaponType.SniperRifle:
                default:
                    return new List<PartType>();
            }
        }
        private void OnPartButtonClicked(PartType partType)
        {
            selectedPartType = partType;
            selectedPartData = CoreManager.Instance.resourceManager.GetAllAssetsOfType<WeaponPartData>().FirstOrDefault(x => x.Type == partType);

            requiredText.text = !selectedPartData ? $"{partType} is required" : "";
            HighlightPartInPreview(partType);
            UpdateStatUI();
            UpdateNameUI();
        }

        private Sprite GetPartSprite(PartType partType)
        {
            return null;
        }
        
        private void HighlightPartInPreview(PartType partType)
        {
            UnhighlightPart();
            if (!previewWeaponHandler) return;
            var renderer = previewWeaponHandler.GetRendererOfPart(partType);
            if (!renderer) return;
            lastHighlightedPartType = partType;
            lastSelectedPartMaterial = renderer.material;
            renderer.material = partHighlightMaterial;
        }

        private void UnhighlightPart()
        {
            if (!previewWeaponHandler || lastHighlightedPartType == null) return;
            var renderer = previewWeaponHandler.GetRendererOfPart(lastHighlightedPartType.Value);
            if (renderer && lastSelectedPartMaterial) renderer.material = lastSelectedPartMaterial;
            lastHighlightedPartType = null;
            lastSelectedPartMaterial = null;
        }

        private void OnApplyButtonClicked()
        {
            if (CurrentWeaponType == WeaponType.Pistol) { applyModal.SetActive(true); return; }
            if (selectedPartType == null || !selectedPartData) return;
            applyModal.SetActive(true);
        }

        private void OnApplyConfirmed()
        {
            applyModal.SetActive(false);
            bool applied = false;
            if (CurrentWeaponType == WeaponType.Pistol)  applied = TryUpgradePistol(); 
            else if (selectedPartType != null && selectedPartData)  applied = TryEquipPart(CurrentWeapon, selectedPartData); 
            if (applied) Refresh(); 
            else Debug.LogError("Failed to apply part");
        }
        
        private bool TryEquipPart(BaseWeapon weapon, WeaponPartData partData)
        {
            if (!weapon || !partData) return false;
            return weapon switch
            {
                Gun gun => gun.TryEquipWeaponPart(partData.Type, partData.Id),
                GrenadeLauncher grenade => grenade.TryEquipWeaponPart(partData.Type, partData.Id),
                HackGun hackGun => hackGun.TryEquipWeaponPart(partData.Type, partData.Id),
                _ => false
            };
        }
        private bool TryUpgradePistol()
        {
            //TODO: Check Upgrade Conditions
            return true;
        }

        private void HideModal() { applyModal.SetActive(false); }

        private void UpdateStatUI()
        {
            if (!CurrentWeapon)
            {
                ResetStatUI();
                return;
            }
            var stat = SlotUtility.GetWeaponStat(CurrentWeapon);
            int maxDamage = 1000;
            float maxRPM = 100f;
            float maxRecoil = 100f;
            float maxWeight = 10f;
            int maxAmmo = 60;

            damageSlider.value = (float)stat.Damage / maxDamage;
            rpmSlider.value = stat.Rpm / maxRPM;
            recoilSlider.value = stat.Recoil / maxRecoil;
            weightSlider.value = stat.Weight / maxWeight;
            ammoSlider.value = (float)stat.MaxAmmoCountInMagazine / maxAmmo;

            damageText.text = stat.Damage.ToString();
            rpmText.text = Mathf.RoundToInt(stat.Rpm).ToString();
            recoilText.text = Mathf.RoundToInt(stat.Recoil).ToString();
            weightText.text = stat.Weight.ToString("F1");
            ammoText.text = stat.MaxAmmoCountInMagazine.ToString();
        }
        private void ResetStatUI()
        {
            damageText.text = rpmText.text = recoilText.text = weightText.text = ammoText.text = "";
            damageSlider.value = rpmSlider.value = recoilSlider.value = weightSlider.value = ammoSlider.value = 0f;
        }
        
        private void UpdateNameUI()
        {
            if (weaponNameText)
                weaponNameText.text = CurrentWeapon ? SlotUtility.GetWeaponName(CurrentWeapon) : "";
            if (partNameText)
                partNameText.text = selectedPartType != null ? selectedPartType.ToString() : "";
        }

        private void OnPrevWeaponClicked()
        {
            int originalIdx = currentWeaponIdx;
            for (int i = 1; i <= SlotOrder.Length; i++)
            {
                int idx = (currentWeaponIdx - i + SlotOrder.Length) % SlotOrder.Length;
                if (!ownedWeapons.ContainsKey(SlotOrder[idx])) continue;
                currentWeaponIdx = idx;
                break;
            }
            Refresh();
        }

        private void OnNextWeaponClicked()
        {
            int originalIdx = currentWeaponIdx;
            for (int i = 1; i <= SlotOrder.Length; i++)
            {
                int idx = (currentWeaponIdx + i) % SlotOrder.Length;
                if (!ownedWeapons.ContainsKey(SlotOrder[idx])) continue;
                currentWeaponIdx = idx;
                break;
            }
            Refresh();
        }
    }
}