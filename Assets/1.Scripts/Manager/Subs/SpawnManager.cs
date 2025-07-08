using System;
using _1.Scripts.Manager.Core;
using _1.Scripts.Util;
using UnityEngine;

namespace _1.Scripts.Manager.Subs
{
    [Serializable] public class SpawnManager
    {
        private SpawnData currentSpawnData;

        public void ChangeSpawnData(SpawnData spawnData)
        {
            currentSpawnData = spawnData;
        }

        public void SpawnWeaponAndItemBySpawnData(SpawnData spawnData, SceneType sceneType)
        {
            ChangeSpawnData(spawnData);
            //
            // switch (sceneType)
            // {
            //     case SceneType.Stage1:
            //         foreach (var weapons in currentSpawnData.)
            // }
            //
            // foreach (var VARIABLE in currentSpawnData.WeaponSpawnPoints)
            // {
            // }

        }
    }
}
