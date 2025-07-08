using System;
using System.Collections.Generic;
using System.Linq;
using _1.Scripts.Item.Common;
using _1.Scripts.Util;
using AYellowpaper.SerializedCollections;
using UnityEngine;

namespace _1.Scripts.Weapon.Scripts.Common
{
    [CreateAssetMenu(fileName = "New SpawnPoints", menuName = "ScriptableObjects/Common", order = 0)]
    public class SpawnData : ScriptableObject
    {
        [field: Header("Item Spawn Points")]
        [field: SerializeField] public SerializedDictionary<ItemType, List<Pair>> ItemSpawnPoints { get; private set; }
        
        [field: Header("Weapon Spawn Points")]
        [field: SerializeField] public SerializedDictionary<WeaponType, List<Pair>> WeaponSpawnPoints { get; private set; }
        
        [field: Header("Enemy Spawn Points")]
        [field: SerializeField] public SerializedDictionary<string, List<Pair>> EnemySpawnPoints { get; private set; }

        public void SetSpawnPoints(string type, Pair[] spawnPoints)
        {
            EnemySpawnPoints ??= new SerializedDictionary<string, List<Pair>>();
            if (EnemySpawnPoints.TryGetValue(type, out var list)) { list.AddRange(spawnPoints); }
            else { EnemySpawnPoints[type] = spawnPoints.ToList(); }
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