using System;
using System.Collections;
using System.Collections.Generic;
using _1.Scripts.Map.Console;
using _1.Scripts.Map.Door;
using UnityEngine;

namespace _1.Scripts.Map
{
    [Serializable]
    public class StageClearCondition
    {
        public StageDoorConditionType conditionType;
        public MapConsole targetConsole;
        public GameObject TargetEnemy;
        public Transform targetLocation;
        
        [HideInInspector] public bool isConditionMet;
    }
}