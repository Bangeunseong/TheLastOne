using System;
using System.Net;
using _1.Scripts.Item.Common;
using _1.Scripts.Manager.Core;
using _1.Scripts.Util;
using _1.Scripts.Weapon.Scripts.Common;
using UnityEngine;

namespace _1.Scripts.Manager.Subs
{
    [Serializable] public class SpawnManager
    {
        [field: Header("Spawn Point Data")]
        [field: SerializeField] public SpawnData CurrentSpawnData { get; private set; }

        private CoreManager coreManager;

        public void Start()
        {
            coreManager = CoreManager.Instance;
        }
        
        public void ChangeSpawnDataAndInstantiate(SceneType sceneType)
        {
            CurrentSpawnData = sceneType switch
            {
                SceneType.Stage1 => coreManager.resourceManager.GetAsset<SpawnData>("SpawnPoints_Stage1"),
                SceneType.Stage2 => coreManager.resourceManager.GetAsset<SpawnData>("SpawnPoints_Stage2"),
                _ => null
            };
            SpawnPropsBySpawnData();
        }

        public void SpawnEnemyBySpawnData(int index)
        {
            if (CurrentSpawnData.EnemySpawnPoints.TryGetValue(index, out var spawnPoints))
            {
                foreach (var pair in spawnPoints)
                {
                    foreach (var val in pair.Value)
                    {
                        GameObject enemy = coreManager.objectPoolManager.Get(pair.Key.ToString());
                        enemy.transform.position = val.position;
                        enemy.transform.rotation = val.rotation;
                    }
                }
            }
        }

        private void SpawnPropsBySpawnData()
        {
            if (CurrentSpawnData == null) return;
            foreach (var pair in CurrentSpawnData.WeaponSpawnPoints)
            {
                foreach (var val in pair.Value)
                {
                    UnityEngine.Object.Instantiate(
                        coreManager.resourceManager.GetAsset<GameObject>(pair.Key + "_Dummy"), 
                        val.position, val.rotation);
                }
            }
            foreach (var pair in CurrentSpawnData.ItemSpawnPoints)
            {
                foreach (var val in pair.Value)
                {
                    UnityEngine.Object.Instantiate(
                        coreManager.resourceManager.GetAsset<GameObject>(pair.Key + "_Prefab"),
                        val.position, val.rotation);
                }
            }
        }
    }
}
