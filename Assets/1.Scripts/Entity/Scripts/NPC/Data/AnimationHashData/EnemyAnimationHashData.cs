using System;
using UnityEngine;

namespace _1.Scripts.Entity.Scripts.NPC.Data.AnimationHashData
{
    public static class DroneAnimationData
    {
        public const string RepairStr = "DroneBot_Repair";
        public const string Dead3Str = "DroneBot_Dead3";
        public const string Dead2Str = "DroneBot_Dead2";
        public const string Dead1Str = "DroneBot_Dead1";
        public const string Hit4Str = "DroneBot_Hit4";
        public const string Hit3Str = "DroneBot_Hit3";
        public const string Hit2Str = "DroneBot_Hit2";
        public const string Hit1Str = "DroneBot_Hit1";
        public const string StrafeLeftStr = "DroneBot_StrafeLeft";
        public const string Idle1Str = "DroneBot_Idle1";
        public const string FireStr = "DroneBot_Fire";

        public static readonly int Repair = Animator.StringToHash(RepairStr);
        public static readonly int Dead3 = Animator.StringToHash(Dead3Str);
        public static readonly int Dead2 = Animator.StringToHash(Dead2Str);
        public static readonly int Dead1 = Animator.StringToHash(Dead1Str);
        public static readonly int Hit4 = Animator.StringToHash(Hit4Str);
        public static readonly int Hit3 = Animator.StringToHash(Hit3Str);
        public static readonly int Hit2 = Animator.StringToHash(Hit2Str);
        public static readonly int Hit1 = Animator.StringToHash(Hit1Str);
        public static readonly int StrafeLeft = Animator.StringToHash(StrafeLeftStr);
        public static readonly int Idle1 = Animator.StringToHash(Idle1Str);
        public static readonly int Fire = Animator.StringToHash(FireStr);
    }

    public static class ShebotAnimationData
    {
        public const string Shebot_Sword_RunStr = "ShebotSword_Run";
        public const string Shebot_Sword_Run_AnimationNameStr = "anim_f2_run";
        public const string Shebot_IdleStr = "Shebot_Idle";
        public const string Shebot_DieStr = "Shebot_Die";
        public const string Shebot_GuardStr = "Shebot_Guard";
        public const string Shebot_Sword_Attack3Str = "Shebot_Sword_Attack3";
        public const string Shebot_Sword_Attack_FullStr = "Shebot_Sword_Attack_Full";
        public const string Shebot_WalkStr = "Shebot_Walk";
        public const string Shebot_Guard_StayStr = "Shebot_Guard_Stay";
        public const string Shebot_Rifle_AimStr = "Shebot_Rifle_Aim";
        public const string Shebot_Rifle_fireStr = "Shebot_Rifle_fire";

        public static readonly int ShebotSword_Run = Animator.StringToHash(Shebot_Sword_RunStr);
        public static readonly int Shebot_Idle = Animator.StringToHash(Shebot_IdleStr);
        public static readonly int Shebot_Die = Animator.StringToHash(Shebot_DieStr);
        public static readonly int Shebot_Guard = Animator.StringToHash(Shebot_GuardStr);
        public static readonly int Shebot_Sword_Attack3 = Animator.StringToHash(Shebot_Sword_Attack3Str);
        public static readonly int Shebot_Sword_Attack_Full = Animator.StringToHash(Shebot_Sword_Attack_FullStr);
        public static readonly int Shebot_Walk = Animator.StringToHash(Shebot_WalkStr);
        public static readonly int Shebot_Guard_Stay = Animator.StringToHash(Shebot_Guard_StayStr);
        public static readonly int Shebot_Rifle_Aim = Animator.StringToHash(Shebot_Rifle_AimStr);
        public static readonly int Shebot_Rifle_fire = Animator.StringToHash(Shebot_Rifle_fireStr);
    }
}