using UnityEngine;

namespace _1.Scripts.Entity.Scripts
{
    [CreateAssetMenu(fileName = "Stats", menuName = "ScriptableObjects/Entity")]
    public class EntityStatData : ScriptableObject
    {
        [Header("Entity type")] 
        public string entityName;
        public bool isPlayer;
        public bool isAlly;

        [Header("Stats")] 
        public float maxHealth;
        public float currentHealth;
        public float baseDamage;
        public float baseAttackRate;

        [Header("Movement")] 
        public float moveSpeed;
        public float runMultiplier;
        public float walkMultiplier;

        [Header("Audio")] 
        public AudioClip[] footStepSounds;
        public AudioClip[] hitSounds;
        public AudioClip[] deathSounds;
    }
}