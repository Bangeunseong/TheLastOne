using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using _1.Scripts.Entity.Scripts.Player.Core;
using _1.Scripts.Manager.Core;
using _1.Scripts.Manager.Data;
using _1.Scripts.Weapon.Scripts;
using UnityEngine;
using Newtonsoft.Json;
using Unity.Collections;
using CharacterInfo = _1.Scripts.Manager.Data.CharacterInfo;

namespace _1.Scripts.Manager.Subs
{
    [Serializable] public class GameManager
    {
        [Header("Save Settings")] 
        [SerializeField, ReadOnly] private string SaveDirectoryPath = "Assets/Data/";
        [SerializeField, ReadOnly] private string FileName = "SaveData.json";
        [field: SerializeField] public DataTransferObject SaveData { get; private set; }
        [field: SerializeField] public Player Player { get; private set; }
        
        private CoreManager coreManager;
        
        public void Start()
        {
            coreManager = CoreManager.Instance;
            SaveData = null;
        }
        
        // Methods
        public void Initialize_Player(Player player) { Player = player; }
        
        public async Task TrySaveData()
        {
            if (!Directory.Exists(SaveDirectoryPath)) Directory.CreateDirectory(SaveDirectoryPath);
            
            var save = new DataTransferObject
            {
                characterInfo = new CharacterInfo
                {
                    maxHealth = Player.PlayerCondition.MaxHealth, health = Player.PlayerCondition.CurrentHealth,
                    maxStamina = Player.PlayerCondition.MaxStamina, stamina = Player.PlayerCondition.CurrentStamina,
                    attackRate = Player.PlayerCondition.AttackRate, damage = Player.PlayerCondition.Damage,
                    level = Player.PlayerCondition.Level, experience = Player.PlayerCondition.Experience,
                },
                currentSceneId = coreManager.sceneLoadManager.CurrentScene,
                currentCharacterPosition = new SerializableVector3(Player.PlayerCondition.LastSavedPosition), 
                currentCharacterRotation = new SerializableQuaternion(Player.PlayerCondition.LastSavedRotation),
            };

            var newWeaponInfo = new List<WeaponInfo>();
            var newAvailableWeapons = Player.PlayerCondition.AvailableWeapons.ToList();
            foreach (var weapon in Player.PlayerCondition.Weapons)
            {
                if (weapon is Gun gun)
                    newWeaponInfo.Add(new WeaponInfo
                    {
                        currentAmmoCount = gun.CurrentAmmoCount,
                        currentAmmoCountInMagazine = gun.CurrentAmmoCountInMagazine
                    });
                // else if (weapon is Grenade grenade) ...
            }

            save.Weapons = newWeaponInfo.ToArray();
            save.AvailableWeapons = newAvailableWeapons.ToArray();
            
            var json = JsonConvert.SerializeObject(save, Formatting.Indented);
            await File.WriteAllTextAsync(SaveDirectoryPath + FileName, json);
        }

        public async Task TryLoadData()
        {
            if (File.Exists(SaveDirectoryPath + FileName))
            {
                var str = await File.ReadAllTextAsync(SaveDirectoryPath + FileName);
                SaveData = JsonConvert.DeserializeObject<DataTransferObject>(str);
            } else SaveData = null;
        }
    }
}
