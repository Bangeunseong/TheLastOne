using System;
using System.Collections.Generic;
using System.Linq;
using _1.Scripts.Entity.Scripts.Player.Core;
using _1.Scripts.Entity.Scripts.Player.Data;
using _1.Scripts.Item.Common;
using _1.Scripts.Util;
using _1.Scripts.Weapon.Scripts.Common;
using UnityEditor;
using UnityEngine;

namespace _6.Debug
{
    public class CustomDebugWindow : EditorWindow
    {
        private GameObject playerObj;

        [MenuItem("Window/Custom Debug Window")]
        public static void ShowWindow()
        {
            GetWindow<CustomDebugWindow>("Debug Window");
        }

        private void OnGUI()
        {
            GUILayout.Label("Custom Debug Tool", EditorStyles.boldLabel);
            
            if (GUILayout.Button("Recover Focus Gauge"))
            {
                playerObj = GameObject.FindWithTag("Player");
                if (!playerObj.TryGetComponent(out Player playerComponent)) return;
                playerComponent.PlayerCondition.OnRecoverFocusGauge(FocusGainType.Debug);
            }

            if (GUILayout.Button("Recover Instinct Gauge"))
            {
                playerObj = GameObject.FindWithTag("Player");
                if (!playerObj.TryGetComponent(out Player playerComponent)) return;
                playerComponent.PlayerCondition.OnRecoverInstinctGauge(InstinctGainType.Debug);
            }

            if (GUILayout.Button("Find Spawn Points & Create S.O."))
            {
#if UNITY_EDITOR
                var enemySpawnObjects = GameObject.FindGameObjectsWithTag("EnemySpawnPoint");
                var droneSpawnPoints = enemySpawnObjects.Where(obj => obj.name.Contains("_Drone", StringComparison.OrdinalIgnoreCase)).Select(obj => new Pair(obj.transform.position, obj.transform.rotation));
                var suicideDroneSpawnPoints = enemySpawnObjects.Where(obj => obj.name.Contains("_SuicideDrone", StringComparison.OrdinalIgnoreCase)).Select(obj => new Pair(obj.transform.position, obj.transform.rotation));
                
                var itemSpawnObjects = GameObject.FindGameObjectsWithTag("ItemSpawnPoint");
                var medkitSpawnPoints = itemSpawnObjects.Where(obj => obj.name.Contains("_Medkit", StringComparison.OrdinalIgnoreCase)).Select(obj => new Pair(obj.transform.position, obj.transform.rotation));
                var nanoAmpleSpawnPoints = itemSpawnObjects.Where(obj => obj.name.Contains("_NanoAmple", StringComparison.OrdinalIgnoreCase)).Select(obj => new Pair(obj.transform.position, obj.transform.rotation));
                var staminaPillSpawnPoints = itemSpawnObjects.Where(obj =>  obj.name.Contains("_EnergyBar", StringComparison.OrdinalIgnoreCase)).Select(obj => new Pair(obj.transform.position, obj.transform.rotation));
                var shieldSpawnPoints = itemSpawnObjects.Where(obj => obj.name.Contains("_Shield", StringComparison.OrdinalIgnoreCase)).Select(obj => new Pair(obj.transform.position, obj.transform.rotation));
                
                var weaponSpawnObjects = GameObject.FindGameObjectsWithTag("WeaponSpawnPoint");
                var pistolSpawnPoints = weaponSpawnObjects.Where(obj => obj.name.Contains("_Pistol", StringComparison.OrdinalIgnoreCase)).Select(obj => new Pair(obj.transform.position, obj.transform.rotation));
                var rifleSpawnPoints = weaponSpawnObjects.Where(obj => obj.name.Contains("_Rifle", StringComparison.OrdinalIgnoreCase)).Select(obj => new Pair(obj.transform.position, obj.transform.rotation));
                var glSpawnPoints = weaponSpawnObjects.Where(obj => obj.name.Contains("_GL", StringComparison.OrdinalIgnoreCase)).Select(obj => new Pair(obj.transform.position, obj.transform.rotation));

                var data = CreateInstance<SpawnData>();
                data.SetSpawnPoints("Drone", droneSpawnPoints.ToArray());
                data.SetSpawnPoints("SuicideDrone", suicideDroneSpawnPoints.ToArray());
                
                data.SetSpawnPoints(ItemType.Medkit, medkitSpawnPoints.ToArray());
                data.SetSpawnPoints(ItemType.NanoAmple, nanoAmpleSpawnPoints.ToArray());
                data.SetSpawnPoints(ItemType.EnergyBar, staminaPillSpawnPoints.ToArray());
                data.SetSpawnPoints(ItemType.Shield, shieldSpawnPoints.ToArray());
                
                data.SetSpawnPoints(WeaponType.Pistol, pistolSpawnPoints.ToArray());
                data.SetSpawnPoints(WeaponType.Rifle, rifleSpawnPoints.ToArray());
                data.SetSpawnPoints(WeaponType.GrenadeLauncher, glSpawnPoints.ToArray());
                
                AssetDatabase.CreateAsset(data, "Assets/8.ScriptableObjects/SpawnPoint/SpawnPoints.asset");
                AssetDatabase.SaveAssets();
#endif
            }
        }
    }
}