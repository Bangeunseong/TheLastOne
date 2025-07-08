using System;
using UnityEngine;

namespace _1.Scripts.Util
{
    [Serializable] public struct Pair
    {
        public Vector3 position;
        public Quaternion rotation;
        public bool isRepeating;
        public int repeatCount;

        public Pair(Vector3 position, Quaternion rotation)
        {
            this.position = position;
            this.rotation = rotation;
            isRepeating = false;
            repeatCount = 0;
        }
    }
}