using System.Collections;
using System.Collections.Generic;
using _1.Scripts.Entity.Scripts.Player.Core;
using _1.Scripts.Manager.Subs;
using _1.Scripts.UI;
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
        [Header("Slot Buttons")]
        public List<Button> slotButtons;
        
        [Header("Preview")]
        public Transform previewContainer;
        public RawImage previewRawImage;
        public RenderTexture previewTexture;

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
        private GameObject currentPreview;

        private int maxDamage = 1000;
        private float maxRPM = 100f;
        private float maxRecoil = 100f;
        private float maxWeight = 1f;
        private int maxAmmo;
        
        public Vector3 rotationSpeed = new Vector3(0f, 10f, 0f);

        private const string PreviewLayerName = "WeaponPreview";
        
        

        private void Start()
        {
            playerCondition = FindObjectOfType<PlayerCondition>();
            if (previewContainer == null)
            {
                var containerGO = GameObject.Find("PreviewContainer");
                if (containerGO != null)
                    previewContainer = containerGO.transform;
                else
                {
                    Service.Log("InventoryUI: 씬에 previewContainer 없음.");
                }
            }

            if (previewRawImage != null)
            {
                previewRawImage.maskable = false;
            }

            if (previewRawImage != null && previewTexture != null)
            {
                previewRawImage.texture = previewTexture;
            }
            else
            {
                Service.Log("InventoryUI: Texture가 할당되지 않음");
            }
            CalculateMaxStats();
            InitializeSlots();
        }

        private void Update()
        {
            if (currentPreview != null)
                currentPreview.transform.Rotate(rotationSpeed * Time.unscaledDeltaTime, Space.World);
        }

        private void CalculateMaxStats()
        {
            if (playerCondition == null) return;
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
            if (playerCondition == null) return;
            var weapons = playerCondition.Weapons;
            var available = playerCondition.AvailableWeapons;

            int slotIndex = 0;
            for (int i = 0; i < weapons.Count && slotIndex < slotButtons.Count; i++)
            {
                var w = weapons[i];
                if (w == null || w.name == "Hand" || !available[i])
                    continue;

                var button = slotButtons[slotIndex];
                button.gameObject.SetActive(true);
                var label = button.GetComponentInChildren<TextMeshProUGUI>();
                if (label != null)
                    label.text = w.name;

                int idx = i;
                button.onClick.RemoveAllListeners();
                button.onClick.AddListener(() => ShowWeapon(idx));
                slotIndex++;
            }

            for (int j = slotIndex; j < slotButtons.Count; j++)
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
            if (weapon == null || weapon.name == "Hand")
                return;
            
            if (currentPreview != null) Destroy(currentPreview);

            if (previewContainer == null)
            {
                Service.Log("InventoryUI: previewContainer 할당되지 않음");
                return;
            }
            
            currentPreview = Instantiate(weapon.gameObject, previewContainer);
            currentPreview.transform.localPosition = Vector3.zero;
            currentPreview.transform.localRotation = Quaternion.identity;
            currentPreview.SetActive(true);
            
            int layer = LayerMask.NameToLayer(PreviewLayerName);
            SetLayerRecursively(currentPreview, layer);

            if (weapon is Gun gun)
                UpdateStats(gun.GunData.GunStat, gun.MaxAmmoCountInMagazine, gun.GunData.GunStat.Rpm,
                    gun.GunData.GunStat.Recoil, maxWeight);
            else if (weapon is GrenadeLauncher gl)
                UpdateStats(gl.GrenadeData.GrenadeStat, gl.MaxAmmoCountInMagazine, gl.GrenadeData.GrenadeStat.Rpm,
                    gl.GrenadeData.GrenadeStat.Recoil, maxWeight);

            if (titleText != null) titleText.text = weapon.name;
            if (descriptionText != null) descriptionText.text = weapon.name + " needs Descriptions";
        }
        private void UpdateStats(WeaponStat stat, int ammoCount, float rpm, float recoil, float weight)
        {
            if (damageSlider != null) { damageSlider.value = (maxDamage > 0) ? stat.Damage / (float)maxDamage : 0f; }
            if (rpmSlider != null) { rpmSlider.value = (maxRPM   > 0) ? rpm / maxRPM   : 0f; }
            if (recoilSlider != null) { recoilSlider.value = (maxRecoil> 0) ? recoil / maxRecoil: 0f; }
            if (ammoSlider != null) { ammoSlider.value = (maxAmmo   > 0) ? ammoCount / (float)maxAmmo : 0f; }
            if (weightSlider != null) { weightSlider.value = (maxWeight > 0) ? weight / maxWeight: 0f; }

            if (damageText != null) damageText.text = stat.Damage.ToString();
            if (rpmText != null) rpmText.text = Mathf.RoundToInt(rpm).ToString();
            if (recoilText != null) recoilText.text = Mathf.RoundToInt(recoil).ToString();
            if (ammoText != null) ammoText.text = ammoCount.ToString();
            if (weightText != null) weightText.text = weight.ToString("F1");
        }
        
        private void SetLayerRecursively(GameObject obj, int layer)
        {
            obj.layer = layer;
            foreach (Transform child in obj.transform)
                SetLayerRecursively(child.gameObject, layer);
        }
    }
}