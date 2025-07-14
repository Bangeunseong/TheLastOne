using System;
using UnityEngine;

namespace _1.Scripts.Quests.Data
{
    public enum ObjectiveType
    {
        Console,
        KillDrones,
        CollectItem,
    }
    
    [Serializable] public class ObjectiveData
    {
        public int targetID;
        public string description;
        public ObjectiveType type;
        public int requiredAmount;
        public GameObject target;
    }
}