using System;
using UnityEngine;

namespace _1.Scripts.Util
{
    [Serializable] public struct Pair
    {
        public Vector3 position;
        public Quaternion rotation;

        public Pair(Vector3 position, Quaternion rotation)
        {
            this.position = position;
            this.rotation = rotation;
        }
    }
}