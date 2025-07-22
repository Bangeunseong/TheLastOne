using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace _1.Scripts.Static
{
    public static class PoolableGameObjects_Common
    {
        public static HashSet<string> prefabs = new()
        {
            "Bullet",
            "BulletHole_Wall",
            "BulletHole_Ground",
            "EmpGrenadeExplosion",
            "Grenade",
            "SoundPlayer",
            "Pistol_Dummy",
            "Rifle_Dummy",
            "GrenadeLauncher_Dummy",
            "HackGun_Dummy",
            "Medkit_Prefab",
            "NanoAmple_Prefab",
            "Shield_Prefab",
            "EnergyBar_Prefab",
        };
    }

    public static class PoolableGameObjects_Stage1
    {
        public static HashSet<string> prefabs = new()
        {
            "ReconDroneNotHackable",
            "BattleRoomReconDrone",
            "SuicideDroneNotHackable",
            "HackingProgressUI",
        };
    }

    public static class PoolableGameObjects_Stage2
    {
        public static HashSet<string> prefabs = new()
        {
            "robot2"
        };
    }
}