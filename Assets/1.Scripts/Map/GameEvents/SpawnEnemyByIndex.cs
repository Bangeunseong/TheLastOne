using System;
using System.Collections.Generic;
using _1.Scripts.Manager.Core;
using _1.Scripts.Quests.Core;
using _1.Scripts.Util;
using Unity.Collections;
using UnityEngine;

namespace _1.Scripts.Map.GameEvents
{
    public class SpawnEnemyByIndex : MonoBehaviour, IGameEventListener
    {
        [Header("Spawn Trigger Id")]
        [Tooltip("It should be same with corresponding Save Point Id")]
        [Range(1, 50)] [SerializeField] private int spawnIndex;

        [Header("Target Count")]
        [Tooltip("Target Count of Killed Enemies which corresponding with spawn index")]
        [SerializeField] private int targetCount;
        [Tooltip("This is for debugging. Do not touch this value!")]
        [SerializeField] private int killedCount;

        [Header("Invisible Wall")] 
        [SerializeField] private List<BoxCollider> invisibleWall = new();
        
        private bool isSpawned;

        private void Awake()
        {
            if (invisibleWall.Count <= 0) invisibleWall.AddRange(this.TryGetChildComponents<BoxCollider>("InvisibleWalls"));
        }

        private void Reset()
        {
            if (invisibleWall.Count <= 0) invisibleWall.AddRange(this.TryGetChildComponents<BoxCollider>("InvisibleWalls"));
        }

        private void Start()
        {
            var save = CoreManager.Instance.gameManager.SaveData;
            if (save == null ||
                !save.stageInfos.TryGetValue(CoreManager.Instance.sceneLoadManager.CurrentScene, out var info) || 
                !info.completionDict.TryGetValue(spawnIndex + BaseEventIndex.BaseSavePointIndex + 1, out var val)) return;

            if (!val) return;
            isSpawned = true;
            enabled = false;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (isSpawned || !other.CompareTag("Player")) return;

            if (!CoreManager.Instance.spawnManager.CurrentSpawnData.EnemySpawnPoints.TryGetValue(spawnIndex,
                    out var spawnPoints))
            {
                Debug.LogError("Couldn't find spawn point, Target Count is currently zero!");
                return;
            }
            
            Debug.Log("스폰됨");
            
            targetCount = spawnPoints.Count;
            GameEventSystem.Instance.RegisterListener(this);
            CoreManager.Instance.spawnManager.SpawnEnemyBySpawnData(spawnIndex);
            if (invisibleWall.Count > 0)
                foreach (var wall in invisibleWall) wall.gameObject.SetActive(true);
            
            isSpawned = true;
            enabled = false;
        }
        
        public void OnEventRaised(int eventID)
        {
            if (eventID != BaseEventIndex.BaseSpawnEnemyIndex + spawnIndex) return;
            
            killedCount++;
            if (killedCount < targetCount) return;
            
            if (invisibleWall.Count > 0)
                foreach (var wall in invisibleWall) wall.gameObject.SetActive(false);
            GameEventSystem.Instance.UnregisterListener(this);
        }
    }
}
