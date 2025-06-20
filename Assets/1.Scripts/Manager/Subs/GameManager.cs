using System;
using System.IO;
using System.Threading.Tasks;
using _1.Scripts.Manager.Core;
using _1.Scripts.Manager.Data;
using UnityEngine;
using Newtonsoft.Json;
using Unity.Collections;

namespace _1.Scripts.Manager.Subs
{
    [Serializable] public partial class GameManager
    {
        // Fields
        [Header("Core")]
        [SerializeField] private Managers coreManager;
        
        [Header("Save Settings")] 
        [SerializeField, ReadOnly] private string SaveFilePath = "Assets/Data/SaveData.json";
        
        [field: Header("Loaded Save Data")]
        [field: SerializeField] public DataTransferObject SaveData { get; private set; }

        // Constructor
        public GameManager(Managers core){ coreManager = core; }
        
        // Methods
        public async Task TrySaveData()
        {
            var save = new DataTransferObject
            {
                MaxHealth = 100,  Health = 100, CurrentStageId = 0,
                CurrentCharacterPosition = new SerializableVector3(Vector3.zero), 
                CurrentCharacterRotation = new SerializableQuaternion(Quaternion.identity),
            };
            
            var json = JsonConvert.SerializeObject(save, Formatting.Indented);
            Debug.Log(json);
            await File.WriteAllTextAsync(SaveFilePath, json);
        }

        public async Task TryLoadData()
        {
            var str = File.ReadAllTextAsync(SaveFilePath);
            while(!str.IsCompleted) { await Task.Yield(); }

            if (str.IsCompletedSuccessfully)
            {
                SaveData = JsonConvert.DeserializeObject<DataTransferObject>(str.Result);
                Debug.Log(SaveData);
                // TODO: Apply Loaded Data to Character
            }
            else
            {
                Debug.LogWarning("Failed to load character data!");
                SaveData = null;
                // TODO: Apply Base Data to Character
            }
        }
    }
}
