using System;
using System.Collections;
using System.Collections.Generic;
using _1.Scripts.Manager.Core;
using UnityEngine;

namespace _1.Scripts.Map.SpawnTriggers
{
    public class SpawnEnemyByIndex : MonoBehaviour
    {
        [SerializeField] private int spawnIndex;
        private bool isSpawned;

        private void Start()
        {
            var save = CoreManager.Instance.gameManager.SaveData;
            if (save == null ||
                !save.stageInfos.TryGetValue(CoreManager.Instance.sceneLoadManager.CurrentScene, out var info) || 
                !info.completionDict.TryGetValue(spawnIndex + 1, out var val)) return;
            if (val)
            {
                isSpawned = true;
                enabled = false;
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (isSpawned) return;
            if (other.CompareTag("Player"))
            {
                Debug.Log("스폰됨");
                CoreManager.Instance.spawnManager.SpawnEnemyBySpawnData(spawnIndex);
                isSpawned = true;
                enabled = false;
            }
        }
    }
}
