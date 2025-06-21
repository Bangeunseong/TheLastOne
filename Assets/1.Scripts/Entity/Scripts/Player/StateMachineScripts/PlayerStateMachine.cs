using System;
using UnityEngine;

namespace _1.Scripts.Entity.Scripts.Player.StateMachineScripts
{
    using StateMachine = _1.Scripts.Entity.Scripts.Common.StateMachine;
    using Player = _1.Scripts.Entity.Scripts.Player.Core.Player;
    
    [Serializable] public class PlayerStateMachine : StateMachine
    {
        public Player Player { get; }
        public Vector2 MovementDirection { get; set; }
        public float MovementSpeed { get; private set; }
        public float RotationDamping { get; private set; }
        public float JumpForce { get; set; }
        public Transform MainCameraTransform { get; set; }

        public PlayerStateMachine(Player player)
        {
            Player = player;
        }
    }
}