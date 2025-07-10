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
        
        private void OnTriggerEnter(Collider other)
        {
            if (!isSpawned)
            {
                CoreManager.Instance.spawnManager.SpawnEnemyBySpawnData(spawnIndex);
                isSpawned = true;
            }
        }
    }
}
