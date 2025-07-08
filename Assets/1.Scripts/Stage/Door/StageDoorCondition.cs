using System;
using System.Collections;
using System.Collections.Generic;
using _1.Scripts.Map.Console;
using UnityEngine;

namespace _1.Scripts.Map.Door
{
    public enum StageDoorConditionType
    {
        ConsoleInteract,
        EnemyDefeat,
        LocationReached
    }
    
    [Serializable]
    public class StageDoorCondition
    {
        public StageDoorConditionType conditionType;
        public MapConsole targetConsole;
        public GameObject targetEnemy;
        public Transform targetLocation;

        [HideInInspector] public bool isConditionMet;
    }
}
