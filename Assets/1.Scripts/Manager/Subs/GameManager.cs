using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using _1.Scripts.Entity.Scripts.Player.Core;
using _1.Scripts.Item.Common;
using _1.Scripts.Manager.Core;
using _1.Scripts.Manager.Data;
using _1.Scripts.Weapon.Scripts.Grenade;
using _1.Scripts.Weapon.Scripts.Guns;
using _1.Scripts.Weapon.Scripts.Hack;
using UnityEngine;
using Newtonsoft.Json;
using Unity.Collections;
using CharacterInfo = _1.Scripts.Manager.Data.CharacterInfo;

namespace _1.Scripts.Manager.Subs
{
    [Serializable]
    public class GameManager
    {
        [Header("Save Settings")] [SerializeField, ReadOnly]
        private string SaveDirectoryPath = "Assets/Data/";

        [SerializeField, ReadOnly] private string SaveFileName = "SaveData.json";
        [field: SerializeField] public Player Player { get; private set; }
        [field: SerializeField] public bool IsGamePaused { get; set; }

        private CoreManager coreManager;
        
        public DataTransferObject SaveData { get; private set; }

        public void Start()
        {
            coreManager = CoreManager.Instance;
            SaveData = null;
        }

        // Methods
        public void Initialize_Player(Player player)
        {
            Player = player;
        }

        public async Task TrySaveData()
        {
            if (!Directory.Exists(SaveDirectoryPath)) Directory.CreateDirectory(SaveDirectoryPath);
            
            // Save Current Character Info.
            var save = new DataTransferObject
            {
                characterInfo = new CharacterInfo
                {
                    maxHealth = Player.PlayerCondition.MaxHealth, health = Player.PlayerCondition.CurrentHealth,
                    maxStamina = Player.PlayerCondition.MaxStamina, stamina = Player.PlayerCondition.CurrentStamina,
                    maxShield = Player.PlayerCondition.MaxShield, shield = Player.PlayerCondition.CurrentShield,
                    attackRate = Player.PlayerCondition.AttackRate, damage = Player.PlayerCondition.Damage,
                    level = Player.PlayerCondition.Level, experience = Player.PlayerCondition.Experience,
                    focusGauge = Player.PlayerCondition.CurrentFocusGauge, instinctGauge = Player.PlayerCondition.CurrentInstinctGauge,
                },
                currentSceneId = coreManager.sceneLoadManager.CurrentScene,
                currentCharacterPosition = new SerializableVector3(Player.PlayerCondition.LastSavedPosition),
                currentCharacterRotation = new SerializableQuaternion(Player.PlayerCondition.LastSavedRotation),
            };

            // Save Current Weapon Infos
            var newWeaponInfo = new List<WeaponInfo>();
            var newAvailableWeapons = Player.PlayerCondition.AvailableWeapons.ToList();
            foreach (var weapon in Player.PlayerCondition.Weapons)
            {
                switch (weapon)
                {
                    case Gun gun:
                        newWeaponInfo.Add(new WeaponInfo
                        {
                            currentAmmoCount = gun.CurrentAmmoCount,
                            currentAmmoCountInMagazine = gun.CurrentAmmoCountInMagazine,
                        });
                        break;
                    case GrenadeLauncher grenadeThrower:
                        newWeaponInfo.Add(new WeaponInfo
                        {
                            currentAmmoCount = grenadeThrower.CurrentAmmoCount,
                            currentAmmoCountInMagazine = grenadeThrower.CurrentAmmoCountInMagazine,

                        });
                        break;
                    case HackGun hackingGun:
                        newWeaponInfo.Add(new WeaponInfo
                        {
                            currentAmmoCount = hackingGun.CurrentAmmoCount,
                            currentAmmoCountInMagazine = hackingGun.CurrentAmmoCountInMagazine,

                        });
                        break;
                }
            }
            save.Weapons = newWeaponInfo.ToArray();
            save.AvailableWeapons = newAvailableWeapons.ToArray();

            // Save Current Item Infos
            var newItemCountList = (from ItemType type in Enum.GetValues(typeof(ItemType)) select Player.PlayerInventory.Items[type].CurrentItemCount).ToList();
            save.Items = newItemCountList.ToArray();
            
            // Quest List
            foreach (var quest in coreManager.questManager.activeQuests)
            {
                save.Quests[quest.Key] = new QuestInfo
                {
                    currentObjectiveIndex = quest.Value.currentObjectiveIndex,
                    progresses = quest.Value.Objectives.ToDictionary(val => val.Key, val => val.Value.currentAmount),
                    completionList = quest.Value.Objectives.ToDictionary(val => val.Key, val => val.Value.IsCompleted),
                };
            }
            
            var json = JsonConvert.SerializeObject(save, Formatting.Indented);
            await File.WriteAllTextAsync(SaveDirectoryPath + SaveFileName, json);
        }

        public async Task TryLoadData()
        {
            if (File.Exists(SaveDirectoryPath + SaveFileName))
            {
                var str = await File.ReadAllTextAsync(SaveDirectoryPath + SaveFileName);
                SaveData = JsonConvert.DeserializeObject<DataTransferObject>(str);
            }
            else SaveData = null;
        }

        public void TryRemoveSavedData()
        {
            if (!Directory.Exists(SaveDirectoryPath)) return;
            File.Delete(SaveDirectoryPath + SaveFileName);
            SaveData = null;
        }
        
        public void PauseGame()
        {
            if (!Player) return;

            IsGamePaused = true;
            
            coreManager.timeScaleManager.ChangeTimeScale(0);
            Player.Pov.m_HorizontalAxis.Reset();
            Player.Pov.m_VerticalAxis.Reset();
            Player.InputProvider.enabled = false;
            Player.PlayerInput.enabled = false;
            
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        public void ResumeGame()
        {
            if (!Player) return;

            IsGamePaused = false;
            
            if (Player.PlayerCondition.IsUsingFocus) coreManager.timeScaleManager.ChangeTimeScale(0.5f);
            else coreManager.timeScaleManager.ChangeTimeScale(1);

            if (Player.PlayerCondition.IsPlayerHasControl)
            {
                Player.PlayerInput.enabled = true;
                Player.InputProvider.enabled = true;
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }
        }

        public void ExitGame()
        {
            IsGamePaused = false;
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }
}