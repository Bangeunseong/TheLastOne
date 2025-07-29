using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace _1.Scripts.Entity.Scripts.NPC.Data.AnimationHashData
{
    public static class DroneAnimationHashData
    {
        public static readonly int Repair = Animator.StringToHash("DroneBot_Repair");
        public static readonly int Dead3 = Animator.StringToHash("DroneBot_Dead3");
        public static readonly int Dead2 = Animator.StringToHash("DroneBot_Dead2");
        public static readonly int Dead1 = Animator.StringToHash("DroneBot_Dead1");
        public static readonly int Hit4 = Animator.StringToHash("DroneBot_Hit4");
        public static readonly int Hit3 = Animator.StringToHash("DroneBot_Hit3");
        public static readonly int Hit2 = Animator.StringToHash("DroneBot_Hit2");
        public static readonly int Hit1 = Animator.StringToHash("DroneBot_Hit1");
        public static readonly int StrafeLeft = Animator.StringToHash("DroneBot_StrafeLeft");
        public static readonly int Idle1 = Animator.StringToHash("DroneBot_Idle1");
        public static readonly int Fire = Animator.StringToHash("DroneBot_Fire");
    }

    public static class ShebotAnimationHashData
    {
        public static readonly int ShebotSword_Run = Animator.StringToHash("ShebotSword_Run");
        public static readonly int Shebot_Idle = Animator.StringToHash("Shebot_Idle");
        public static readonly int Shebot_Die = Animator.StringToHash("Shebot_Die");
        public static readonly int Shebot_Guard = Animator.StringToHash("Shebot_Guard");
        public static readonly int Shebot_Sword_Attack3 = Animator.StringToHash("Shebot_Sword_Attack3");
        public static readonly int Shebot_Sword_Attack_Full = Animator.StringToHash("Shebot_Sword_Attack_Full");
        public static readonly int Shebot_Walk = Animator.StringToHash("Shebot_Walk");
        public static readonly int Shebot_Guard_Stay = Animator.StringToHash("Shebot_Guard_Stay");
        public static readonly int Shebot_Rifle_Aim = Animator.StringToHash("Shebot_Rifle_Aim");
        public static readonly int Shebot_Rifle_fire = Animator.StringToHash("Shebot_Rifle_fire");
    }
}
