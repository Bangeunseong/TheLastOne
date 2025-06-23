using System;
using System.IO;
using System.Threading.Tasks;
using _1.Scripts.Entity.Scripts.Player;
using _1.Scripts.Entity.Scripts.Player.Core;
using _1.Scripts.Manager.Core;
using _1.Scripts.Manager.Data;
using UnityEngine;
using Newtonsoft.Json;
using Unity.Collections;

namespace _1.Scripts.Manager.Subs
{
    [Serializable] public class GameManager
    {
        [Header("Save Settings")] 
        [SerializeField, ReadOnly] private string SaveFilePath = "Assets/Data/SaveData.json";
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
            var save = new DataTransferObject
            {
                maxHealth = Player.PlayerCondition.MaxHealth, health = Player.PlayerCondition.CurrentHealth, 
                maxStamina = Player.PlayerCondition.MaxStamina, stamina = Player.PlayerCondition.CurrentStamina,
                attackRate = Player.PlayerCondition.AttackRate, damage = Player.PlayerCondition.Damage,
                CurrentSceneId = coreManager.sceneLoadManager.CurrentScene,
                CurrentCharacterPosition = new SerializableVector3(Vector3.zero), 
                CurrentCharacterRotation = new SerializableQuaternion(Quaternion.identity),
            };
            
            var json = JsonConvert.SerializeObject(save, Formatting.Indented);
            Debug.Log(json);
            await File.WriteAllTextAsync(SaveFilePath, json);
        }

        public async Task TryLoadData()
        {
            var str = await File.ReadAllTextAsync(SaveFilePath);
            SaveData = JsonConvert.DeserializeObject<DataTransferObject>(str);
        }
    }
}
