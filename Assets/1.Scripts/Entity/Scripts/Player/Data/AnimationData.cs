using System;
using Unity.Collections;
using UnityEngine;

namespace _1.Scripts.Entity.Scripts.Player.Data
{
    [Serializable] public class AnimationData
    {
        [Header("Animation Parameters on ground")]
        [SerializeField, ReadOnly] private string groundParameterName = "@Ground";
        [SerializeField, ReadOnly] private string speedParameterName = "Speed";
        [SerializeField, ReadOnly] private string crouchParameterName = "Crouch";
        [SerializeField, ReadOnly] private string crouchWalkParameterName = "CrouchWalk";
        
        [Header("Animation Parameters on air")]
        [SerializeField, ReadOnly] private string airParameterName = "@Air";
        [SerializeField, ReadOnly] private string jumpParameterName = "Jump";
        [SerializeField, ReadOnly] private string fallParameterName = "Fall";
        
        [Header("Animation Parameter on death")]
        [SerializeField, ReadOnly] private string deathParameterName = "Dead";
        [SerializeField, ReadOnly] private string hitParameterName = "Hit";
        
        // Properties of parameter hash
        public int GroundParameterHash { get; private set; }
        public int SpeedParameterHash { get; private set; }
        public int CrouchParameterHash { get; private set; }
        public int CrouchWalkParameterHash { get; private set; }
        public int AirParameterHash { get; private set; }
        public int JumpParameterHash { get; private set; }
        public int FallParameterHash { get; private set; }
        public int DeathParameterHash { get; private set; }
        public int HitParameterHash { get; private set; }
        
        public void Initialize()
        {
            GroundParameterHash = Animator.StringToHash(groundParameterName);
            SpeedParameterHash = Animator.StringToHash(speedParameterName);
            CrouchParameterHash = Animator.StringToHash(crouchParameterName);
            CrouchWalkParameterHash = Animator.StringToHash(crouchWalkParameterName);

            AirParameterHash = Animator.StringToHash(airParameterName);
            JumpParameterHash = Animator.StringToHash(jumpParameterName);
            FallParameterHash = Animator.StringToHash(fallParameterName);
            
            DeathParameterHash = Animator.StringToHash(deathParameterName);
            HitParameterHash = Animator.StringToHash(deathParameterName);
        }
    }
}