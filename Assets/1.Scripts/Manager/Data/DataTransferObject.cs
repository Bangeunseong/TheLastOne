using System;
using _1.Scripts.Manager.Subs;
using UnityEngine;

namespace _1.Scripts.Manager.Data
{
    [Serializable]
    public class SerializableVector3
    {
        public float x, y, z;
        
        public SerializableVector3(){}
        public SerializableVector3(Vector3 v){ x = v.x; y = v.y; z = v.z; }

        public Vector3 ToVector3() { return new Vector3(x, y, z); }
    }

    [Serializable]
    public class SerializableQuaternion
    {
        public float x, y, z, w;
        
        public SerializableQuaternion(){}
        public SerializableQuaternion(Quaternion q){ x = q.x; y = q.y; z = q.z; w = q.w; }
        
        public Quaternion ToQuaternion() { return new Quaternion(x, y, z, w); }
    }
    
    [Serializable] public class DataTransferObject
    {
        [Header("Character Stat.")]
        [SerializeField] public int maxHealth;
        [SerializeField] public int health;
        [SerializeField] public int damage;
        [SerializeField] public float attackRate;

        [Header("Stage Info.")] 
        [SerializeField] public SceneType CurrentSceneId;
        [SerializeField] public SerializableVector3 CurrentCharacterPosition;
        [SerializeField] public  SerializableQuaternion CurrentCharacterRotation;

        public override string ToString()
        {
            return
                $"Character Stat.\n{maxHealth}, {health}\n{damage}, {attackRate}\n" +
                $"Stage Info.\n{CurrentCharacterPosition.ToVector3()}, {CurrentCharacterRotation.ToQuaternion()}";
        }
    }
}