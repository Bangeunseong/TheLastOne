using System.Collections.Generic;
using System.Linq;
using _1.Scripts.Entity.Scripts.Npc.StatControllers.Base;
using _1.Scripts.Item.Common;
using _1.Scripts.Weapon.Scripts.Common;
using AYellowpaper.SerializedCollections;
using UnityEngine;

namespace _1.Scripts.Util
{
    [CreateAssetMenu(fileName = "New SpawnPoints", menuName = "ScriptableObjects/Common/Create New SpawnPoints", order = 0)]
    public class SpawnData : ScriptableObject
    {
        [field: Header("Item Spawn Points")]
        [field: SerializeField] public SerializedDictionary<ItemType, List<Pair>> ItemSpawnPoints { get; private set; }
        
        [field: Header("Weapon Spawn Points")]
        [field: SerializeField] public SerializedDictionary<WeaponType, List<Pair>> WeaponSpawnPoints { get; private set; }
        
        [field: Header("Enemy Spawn Points")]
        [field: SerializeField] public SerializedDictionary<int, SerializedDictionary<EnemyType, List<Pair>>> EnemySpawnPoints { get; private set; }
        
        public void SetSpawnPoints(int index, EnemyType enemyType, Pair[] spawnPoints)
        {
            EnemySpawnPoints ??= new SerializedDictionary<int, SerializedDictionary<EnemyType, List<Pair>>>();

            if (!EnemySpawnPoints.TryGetValue(index, out var spawnInfo))
            {
                spawnInfo = new SerializedDictionary<EnemyType, List<Pair>>();
                EnemySpawnPoints[index] = spawnInfo;
            }

            if (spawnInfo.TryGetValue(enemyType, out var list))
            {
                list.AddRange(spawnPoints);
            }
            else
            {
                spawnInfo[enemyType] = spawnPoints.ToList();
            }
        }
        
        public void SetSpawnPoints(ItemType type, Pair[] spawnPoints)
        {
            ItemSpawnPoints ??= new SerializedDictionary<ItemType, List<Pair>>();
            if (ItemSpawnPoints.TryGetValue(type, out var list)) { list.AddRange(spawnPoints); }
            else { ItemSpawnPoints[type] = spawnPoints.ToList(); }
        }
        
        public void SetSpawnPoints(WeaponType type, Pair[] spawnPoints)
        {
            WeaponSpawnPoints ??= new SerializedDictionary<WeaponType, List<Pair>>();
            if (WeaponSpawnPoints.TryGetValue(type, out var list)) { list.AddRange(spawnPoints); }
            else { WeaponSpawnPoints[type] = spawnPoints.ToList(); }
        }
    }
}