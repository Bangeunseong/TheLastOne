using System;

namespace _1.Scripts.Quests.Data
{
    public enum ObjectiveType
    {
        Console,
        KillDrones,
    }
    
    [Serializable] public class ObjectiveData
    {
        public int targetID;
        public string description;
        public ObjectiveType type;
        public int requiredAmount;
    }
}