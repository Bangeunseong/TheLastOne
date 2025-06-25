using System.Collections;
using System.Collections.Generic;
using _1.Scripts.Manager.Subs;
using _1.Scripts.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace _1.Scripts.UI.Inventory
{
    public struct DummyWeaponData
    {
        public string weaponName;
        public Sprite weaponIcon;
        public string description;
        public int damage;
        public int ammo;
        public int recoil;
        public int RPM;
        public int weight;
    }
    public class InventoryUI : UIPopup
    {
        [Header("무기 슬롯")] 
        [SerializeField] private Image weaponImage;
        [SerializeField] private Button weaponButton;
        
        [Header("권총 슬롯")]
        [SerializeField] private Image pistolImage;
        [SerializeField] private Button pistolButton;
        
        [Header("해킹건 슬롯")]
        [SerializeField] private Image hackingGunImage;
        [SerializeField] private Button hackingGunButton;

        [Header("EMP탄 슬롯")] 
        [SerializeField] private Image empBombImage;
        [SerializeField] private Button empBombButton;

        [Header("무기 정보")] 
        [SerializeField] private TextMeshProUGUI weaponNameText;
        [SerializeField] private TextMeshProUGUI damageText;
        [SerializeField] private TextMeshProUGUI ammoText;
        [SerializeField] private TextMeshProUGUI recoilText;
        [SerializeField] private TextMeshProUGUI rpmText;
        [SerializeField] private TextMeshProUGUI weightText;
        [SerializeField] private TextMeshProUGUI descriptionText;
        
        [SerializeField] private Button closeButton;

        private DummyWeaponData weapon;
        private DummyWeaponData pistol;
        private DummyWeaponData hackingGun;
        private DummyWeaponData empBomb;
        
        public override void Init(UIManager manager)
        {
            base.Init(manager);
            
            CreateDummyData();
            
            weaponButton.onClick.AddListener(()=> OnWeaponSlotClicked(weapon));
            pistolButton.onClick.AddListener(() => OnWeaponSlotClicked(pistol));
            hackingGunButton.onClick.AddListener(() => OnWeaponSlotClicked(hackingGun));
            empBombButton.onClick.AddListener(() => OnWeaponSlotClicked(empBomb));
            
            closeButton.onClick.AddListener(ClosePopup);
            
            InitializeDummyData();
        }
        
        private void CreateDummyData()
        {
            weapon = new DummyWeaponData { weaponName = "돌격소총" , description = "더미 데이터: 돌격소총"};
            pistol = new DummyWeaponData { weaponName = "권총", description = "더미 데이터: 권총"};
            hackingGun = new DummyWeaponData { weaponName = "해킹건", description = "더미 데이터: 해킹건"};
            empBomb = new DummyWeaponData { weaponName = "emp폭탄", description = "더미 데이터: emp폭탄"};
        }

        private void InitializeDummyData()
        {
            weaponImage.sprite = weapon.weaponIcon;
            pistolImage.sprite = pistol.weaponIcon;
            hackingGunImage.sprite = hackingGun.weaponIcon;
            empBombImage.sprite = empBomb.weaponIcon;
            
            OnWeaponSlotClicked(weapon);
        }

        private void OnWeaponSlotClicked(DummyWeaponData data)
        {
            weaponNameText.text = data.weaponName;
            descriptionText.text = data.description;
        }
    }
}