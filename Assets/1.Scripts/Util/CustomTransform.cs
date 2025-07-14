using System;
using UnityEngine;

namespace _1.Scripts.Util
{
    [Serializable] public struct CustomTransform
    {
        public Vector3 position;
        public Quaternion rotation;

        public CustomTransform(Vector3 position, Quaternion rotation)
        {
            this.position = position;
            this.rotation = rotation;
        }
    }
}