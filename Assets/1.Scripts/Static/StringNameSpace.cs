using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace _1.Scripts.Static
{
    public static class PoolableGameObjects_Common
    {
        public static readonly HashSet<string> prefabs = new()
        {
            "bullet"
        };
    }
    
    public static class PoolableGameObjects_Stage1
    {
        public static readonly HashSet<string> prefabs = new()
        {
            "robot1"
        };
    }

    public static class PoolableGameObjects_Stage2
    {
        public static readonly HashSet<string> prefabs = new()
        {
            "robot2"
        };
    }
}
