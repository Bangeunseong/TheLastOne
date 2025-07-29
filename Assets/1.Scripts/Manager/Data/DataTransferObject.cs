using System;
using System.Collections.Generic;
using _1.Scripts.Manager.Subs;
using _1.Scripts.Weapon.Scripts.Common;
using _1.Scripts.Weapon.Scripts.WeaponDetails;
using AYellowpaper.SerializedCollections;
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

    [Serializable]
    public class CharacterInfo
    {
        public int maxHealth;
        public int health;
        public float maxStamina;
        public float stamina;
        public int maxShield;
        public int shield;
        public float damage;
        public float attackRate;
        public float focusGauge;
        public float instinctGauge;
    }

    [Serializable] public class WeaponInfo
    {
        public int currentAmmoCount;
        public int currentAmmoCountInMagazine;
        public Dictionary<PartType, int> equippedParts;
        public Dictionary<int, bool> equipableParts;
    }

    [Serializable] public class StageInfo
    {
        
    }

    [Serializable] public class QuestInfo
    {
        public int currentObjectiveIndex;
        public Dictionary<int, int> progresses;
        public Dictionary<int, bool> completionList;
    }
    
    [Serializable] public class DataTransferObject
    {
        [Header("Character Stat.")]
        [SerializeField] public CharacterInfo characterInfo;

        [Header("Character Weapons")] 
        [SerializeField] public SerializedDictionary<WeaponType, WeaponInfo> weapons;
        [SerializeField] public SerializedDictionary<WeaponType, bool> availableWeapons;

        [field: Header("Character Items")]
        [field: SerializeField] public int[] Items { get; set; }
        
        [field: Header("Quests")]
        [field: SerializeField] public SerializedDictionary<int, QuestInfo> Quests { get; private set; } = new();
        
        [Header("Stage Info.")] 
        [SerializeField] public SceneType currentSceneId;
        [SerializeField] public SerializableVector3 currentCharacterPosition;
        [SerializeField] public SerializableQuaternion currentCharacterRotation;
        
        public override string ToString()
        {
            return
                $"Character Stat.\n{characterInfo.maxHealth}, " +
                $"{characterInfo.health}\n{characterInfo.damage}, " +
                $"{characterInfo.attackRate}\n" +
                "Weapon Info.\n" +
                $"{weapons}\n" +
                $"{availableWeapons}\n" +

                $"Stage Info.\n{currentCharacterPosition.ToVector3()}, " +
                $"{currentCharacterRotation.ToQuaternion()}" +
                $"Quest Info.\n{Quests.Values}";
        }
    }
}