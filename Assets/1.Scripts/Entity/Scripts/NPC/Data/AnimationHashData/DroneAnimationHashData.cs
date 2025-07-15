using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace _1.Scripts.Entity.Scripts.NPC.Data.AnimationHashData
{
    [Serializable]
    public static class DroneAnimationHashData
    {
        [SerializeField] public static readonly int Repair = Animator.StringToHash("DroneBot_Repair");
        [SerializeField] public static readonly int Dead3 = Animator.StringToHash("DroneBot_Dead3");
        [SerializeField] public static readonly int Dead2 = Animator.StringToHash("DroneBot_Dead2");
        [SerializeField] public static readonly int Dead1 = Animator.StringToHash("DroneBot_Dead1");
        [SerializeField] public static readonly int Hit4 = Animator.StringToHash("DroneBot_Hit4");
        [SerializeField] public static readonly int Hit3 = Animator.StringToHash("DroneBot_Hit3");
        [SerializeField] public static readonly int Hit2 = Animator.StringToHash("DroneBot_Hit2");
        [SerializeField] public static readonly int Hit1 = Animator.StringToHash("DroneBot_Hit1");
        [SerializeField] public static readonly int StrafeRight = Animator.StringToHash("DroneBot_StrafeRight");
        [SerializeField] public static readonly int StrafeLeft = Animator.StringToHash("DroneBot_StrafeLeft");
        [SerializeField] public static readonly int MoveForward = Animator.StringToHash("DroneBot_Move");
        [SerializeField] public static readonly int Idle2 = Animator.StringToHash("DroneBot_Idle2");
        [SerializeField] public static readonly int Idle1 = Animator.StringToHash("DroneBot_Idle1");
        [SerializeField] public static readonly int Fire = Animator.StringToHash("DroneBot_Fire");
    }
}
