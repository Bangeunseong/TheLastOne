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
        // Fields
        [Header("Core")]
        [SerializeField] private CoreManager coreManager;
        
        [Header("Save Settings")] 
        [SerializeField, ReadOnly] private string SaveFilePath = "Assets/Data/SaveData.json";
        [field: SerializeField] public DataTransferObject SaveData { get; private set; }

        // Constructor
        public GameManager(CoreManager core)
        { 
            coreManager = core;
            SaveData = null;
        }
        
        // Methods
        public async Task TrySaveData()
        {
            var save = new DataTransferObject
            {
                maxHealth = 100, health = 100, attackRate = 1, damage = 10,
                CurrentSceneId = 0,
                CurrentCharacterPosition = new SerializableVector3(Vector3.zero), 
                CurrentCharacterRotation = new SerializableQuaternion(Quaternion.identity),
            };
            
            var json = JsonConvert.SerializeObject(save, Formatting.Indented);
            Debug.Log(json);
            await File.WriteAllTextAsync(SaveFilePath, json);
        }

        public async Task<DataTransferObject> TryLoadData()
        {
            var str = File.ReadAllTextAsync(SaveFilePath);
            while (!str.IsCompleted) { await Task.Yield(); }

            if (str.IsCompletedSuccessfully)
            {
                return SaveData = JsonConvert.DeserializeObject<DataTransferObject>(str.Result);
            }
            Debug.LogWarning("Failed to load character data!");
            return null;
        }
    }
}
