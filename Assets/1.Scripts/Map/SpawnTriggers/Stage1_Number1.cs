using System;
using System.Collections;
using System.Collections.Generic;
using _1.Scripts.Manager.Core;
using UnityEngine;

namespace _1.Scripts.Map.SpawnTriggers
{
    public class Stage1_Number1 : MonoBehaviour
    {
        private bool isSpawned;
        
        private void OnTriggerEnter(Collider other)
        {
            CoreManager.Instance.spawnManager.SpawnEnemyBySpawnData(1);
            isSpawned = true;
        }
    }
}
