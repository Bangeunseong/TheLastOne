using System;
using System.Collections;
using System.Collections.Generic;
using _1.Scripts.Map.Console;
using _1.Scripts.Map.Door;
using UnityEngine;

namespace _1.Scripts.Map
{
    [Serializable]
    public class Stage
    {
        public string stageName;
        public List<StageDoor> doors;
        public List<StageClearCondition> clearConditions;

        public bool CheckStageCleared()
        {
            foreach (var condition in clearConditions)
                if (!condition.isConditionMet) return false;
            return true;
        }

        public void CheckAllDoors()
        {
            foreach (var door in doors)
            {
                door.TryOpenDoor();
            }
        }
    }
}
