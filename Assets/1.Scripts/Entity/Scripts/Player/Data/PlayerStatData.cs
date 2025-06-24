using _1.Scripts.Entity.Scripts.Common;
using UnityEngine;

namespace _1.Scripts.Entity.Scripts.Player.Data
{
    [CreateAssetMenu(fileName = "New PlayerData", menuName = "ScriptableObjects/Player", order = 0)]
    public class PlayerStatData : EntityStatData
    {
        [Header("Extra Stats")]
        public float maxStamina;
        
        [Header("Extra Movement")] 
        public float jumpHeight; 
        public float crouchMultiplier;
        
        [Header("Update Rates And Intervals")] 
        public float consumeRateOfStamina;
        public float recoverRateOfStamina_Idle;
        public float recoverRateOfStamina_Walk;
        public float interval;
    }
}