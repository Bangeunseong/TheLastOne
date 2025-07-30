using System;
using System.Collections.Generic;
using _1.Scripts.Entity.Scripts.Npc.StatControllers.Base;
using _1.Scripts.Entity.Scripts.NPC.StencilAbles;
using _1.Scripts.Item.Items;
using _1.Scripts.Manager.Core;
using _1.Scripts.Manager.Data;
using _1.Scripts.Static;
using _1.Scripts.Util;
using _1.Scripts.Weapon.Scripts.Common;
using BehaviorDesigner.Runtime;
using UnityEngine;
using UnityEngine.AI;

namespace _1.Scripts.Manager.Subs
{
    [Serializable] public class SpawnManager
    {
        public static int BaseWeaponIndex { get; private set; } = 1000; 
        public static int BaseItemIndex { get; private set; } = 1100; 
        public static int BaseItemBoxIndex { get; private set; } = 1200;
        
        [field: Header("Spawn Point Data")]
        [field: SerializeField] public SpawnData CurrentSpawnData { get; private set; }
        
        private HashSet<GameObject> spawnedEnemies = new();
        private HashSet<GameObject> spawnedWeapons = new();
        private HashSet<GameObject> spawnedItems = new();
        private CoreManager coreManager;
        
        [field: Header("Visibility")]
        [field: SerializeField] public bool IsVisible { get; private set; }

        public void Start()
        {
            coreManager = CoreManager.Instance;
        }
        
        public void ChangeSpawnDataAndInstantiate(SceneType sceneType, DataTransferObject dto)
        {
            CurrentSpawnData = sceneType switch
            {
                SceneType.Stage1 => coreManager.resourceManager.GetAsset<SpawnData>("SpawnPoints_Stage1"),
                SceneType.Stage2 => coreManager.resourceManager.GetAsset<SpawnData>("SpawnPoints_Stage2"),
                _ => null
            };
            SpawnPropsBySpawnData(sceneType, dto);
        }

        public void SpawnEnemyBySpawnData(int index)
        {
            if (CurrentSpawnData == null) return;
            if (!CurrentSpawnData.EnemySpawnPoints.TryGetValue(index, out var spawnPoints)) return;
            
            foreach (var pair in spawnPoints)
            {
                foreach (var val in pair.Value)
                {
                    GameObject enemy = coreManager.objectPoolManager.Get(pair.Key.ToString());
                    if (enemy.TryGetComponent(out BehaviorTree behaviorTree)) behaviorTree.SetVariableValue(BehaviorNames.CanRun, false); // 위치조정 BT가 생성 시 위치조정을 무시할 가능성 있음

                    enemy.transform.position = val.position;
                    enemy.transform.rotation = val.rotation;
                        
                    if (enemy.TryGetComponent(out NavMeshAgent agent)) agent.enabled = true; // 적 객체마다 OnEnable에서 키면 위에서 Get()할때 켜져서 디폴트 위치로 가는 버그 재발함. 위치 지정 후 켜야함
                    if (behaviorTree != null) behaviorTree.SetVariableValue(BehaviorNames.CanRun, true);
                    
                    spawnedEnemies.Add(enemy);
                }
            }
        }

        private void SpawnPropsBySpawnData(SceneType sceneType, DataTransferObject dto = null)
        {
            if (CurrentSpawnData == null) return;
            int weaponIndex = 0, itemIndex = 0;
            foreach (var pair in CurrentSpawnData.WeaponSpawnPoints)
            {
                foreach (var val in pair.Value)
                {
                    if (dto == null || !dto.stageInfos.TryGetValue(sceneType, out var info) || 
                        !info.completionDict.TryGetValue(BaseWeaponIndex + weaponIndex, out var value) ||
                        !value)
                    {
                        var obj = coreManager.objectPoolManager.Get(pair.Key + "_Dummy");
                        obj.transform.SetPositionAndRotation(val.position, val.rotation);
                        if (obj.TryGetComponent(out DummyWeapon weapon)) weapon.SetInstanceId(BaseWeaponIndex + weaponIndex);
                        spawnedWeapons.Add(obj);
                    }
                    weaponIndex++;
                }
            }
            foreach (var pair in CurrentSpawnData.ItemSpawnPoints)
            {
                foreach (var val in pair.Value)
                {
                    if (dto == null || !dto.stageInfos.TryGetValue(sceneType, out var info) || 
                        !info.completionDict.TryGetValue(BaseItemIndex + itemIndex, out var value) ||
                        !value)
                    {
                        var obj = coreManager.objectPoolManager.Get(pair.Key + "_Prefab");
                        obj.transform.SetPositionAndRotation(val.position, val.rotation);
                        if (obj.TryGetComponent(out DummyItem item)) item.SetInstanceId(BaseItemIndex + itemIndex);
                        spawnedItems.Add(obj);
                    }
                    itemIndex++;
                }
            }
        }

        public void RemoveWeaponFromSpawnedList(GameObject obj)
        {
            spawnedWeapons.Remove(obj);
        }

        public void RemoveItemFromSpawnedList(GameObject obj)
        {
            spawnedItems.Remove(obj);
        }

        public void ChangeStencilLayer(bool isOn)
        {
            IsVisible = isOn;
            ChangeStencilLayerAllNpc(isOn);
            ChangeLayerOfWeaponsAndItems(isOn);
        }

        /// <summary>
        /// true일 시 스텐실레이어 활성화, false일 시 해제
        /// </summary>
        /// <param name="isOn"></param>
        private void ChangeStencilLayerAllNpc(bool isOn)
        {
            foreach (GameObject obj in spawnedEnemies)
            {
                if (obj.TryGetComponent<StencilAbleForDrone>(out var stencilAbleForDrone))
                {
                    stencilAbleForDrone.StencilLayerOnOrNot(isOn);
                }
                // 이후 다른 컴포넌트도 추가
                // 예시 : TryGetComponent<StencilAbleForGunner>
            }
        }

        private void ChangeLayerOfWeaponsAndItems(bool isTransparent)
        {
            foreach (var obj in spawnedWeapons)
            {
                if (!obj.TryGetComponent(out DummyWeapon weapon)) continue;
                weapon.ChangeLayerOfBody(isTransparent);
            }

            foreach (var obj in spawnedItems)
            {
                if (!obj.TryGetComponent(out DummyItem item)) continue;
                item.ChangeLayerOfBody(isTransparent);
            }
        }

        public void DisposeAllUniTasksFromSpawnedEnemies()
        {
            coreManager.NpcCTS?.Cancel();
            foreach (var obj in spawnedEnemies)
            {
                if (!obj.TryGetComponent(out BaseNpcStatController statController)) continue;
                statController.DisposeAllUniTasks();
            }
        }
        
        public void RemoveMeFromSpawnedEnemies(GameObject enemy)
        {
            spawnedEnemies.Remove(enemy);
        }

        public void ClearAllSpawnedEnemies()
        {
            spawnedEnemies.Clear(); 
        }

        public void ClearAllProps()
        {
            spawnedWeapons.Clear();
            spawnedItems.Clear();
        }

        public void Reset()
        {
            ChangeStencilLayer(false);
            ClearAllSpawnedEnemies();
            ClearAllProps();
            CurrentSpawnData = null;
        }
    }
}
