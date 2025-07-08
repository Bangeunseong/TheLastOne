using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace _1.Scripts.Map.Door
{
    [Serializable]
    public class StageDoor
    {
        public DoorController door;
        public List<StageDoorCondition> conditions;

        public bool CheckConditions()
        {
            foreach (var condition in conditions)
            {
                if (!condition.isConditionMet) return false;
            }
            return true;
        }
        public void TryOpenDoor()
        {
            if (CheckConditions()) door.OpenDoor();
        }
    }
}