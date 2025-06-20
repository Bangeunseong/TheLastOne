using System;
using System.IO;
using System.Threading.Tasks;
using _1.Scripts.Entity.Scripts.Player;
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
        [SerializeField] private CoreManager coreCoreManager;
        
        [Header("Save Settings")] 
        [SerializeField, ReadOnly] private string SaveFilePath = "Assets/Data/SaveData.json";
        [field: SerializeField] public DataTransferObject SaveData { get; private set; }
        
        [field: Header("Player")]
        [field: SerializeField] public Player Player { get; private set; }

        // Constructor
        public GameManager(CoreManager coreCore){ coreCoreManager = coreCore; }
        
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
                return JsonConvert.DeserializeObject<DataTransferObject>(str.Result);
            }
            Debug.LogWarning("Failed to load character data!");
            return null;
        }

        public void ApplyLoadedData(DataTransferObject data)
        {
            Debug.Log($"{data}");
        }
    }
}
