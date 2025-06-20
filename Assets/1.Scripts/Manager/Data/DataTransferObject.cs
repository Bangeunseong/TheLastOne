using System;
using UnityEngine;

namespace _1.Scripts.Manager.Data
{
    [Serializable] public class DataTransferObject
    {
        [Header("Character Stat.")]
        [SerializeField] public int MaxHealth;
        [SerializeField] public int Health;

        [Header("Stage Info.")] 
        [SerializeField] public int CurrentStageId;
        [SerializeField] public Vector3 CurrentCharacterPosition;
        [SerializeField] public Quaternion CurrentCharacterRotation;
    }
}