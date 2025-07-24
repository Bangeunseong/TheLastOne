using System.Collections;
using System.Collections.Generic;
using _1.Scripts.UI.InGame;
using _1.Scripts.UI.InGame.HUD;
using _1.Scripts.Weapon.Scripts.Common;
using _1.Scripts.Weapon.Scripts.Grenade;
using _1.Scripts.Weapon.Scripts.Guns;
using _1.Scripts.Weapon.Scripts.Hack;
using UnityEngine;

namespace _1.Scripts.Util
{
    /// <summary>
    /// UI 인벤토리나 무기 슬롯, 무기 개조 등에 쓰일 슬롯 헬퍼
    /// </summary>
    /// 
    public struct WeaponStatView
    {
        public int Damage;
        public float Rpm;
        public float Recoil;
        public float Weight;
        public int MaxAmmoCountInMagazine;

        public WeaponStatView(int damage, float rpm, float recoil, float weight, int maxAmmo)
        {
            Damage = damage;
            Rpm = rpm;
            Recoil = recoil;
            Weight = weight;
            MaxAmmoCountInMagazine = maxAmmo;
        }
    }

    public static class SlotUtility
    {
        public static bool IsMatchSlot(BaseWeapon w, SlotType slot)
        {
            switch (slot)
            {
                case SlotType.Main:
                    return w is Gun g1 && g1.GunData.GunStat.Type == WeaponType.Rifle;
                case SlotType.Pistol:
                    return w is Gun g2 && g2.GunData.GunStat.Type == WeaponType.Pistol;
                case SlotType.GrenadeLauncher:
                    return w is GrenadeLauncher;
                case SlotType.HackGun:
                    return w is HackGun;
                default:
                    return false;
            }
        }
        
        public static string GetWeaponName(BaseWeapon w)
        {
            if (w is Gun g) return g.GunData.GunStat.Type.ToString();
            if (w is GrenadeLauncher gl) return gl.GrenadeData.GrenadeStat.Type.ToString();
            if (w is HackGun hg) return hg.HackData.HackStat.Type.ToString();
            return w?.GetType().Name ?? "Unknown";
        }
        
        public static (int mag, int total) GetWeaponAmmo(BaseWeapon w)
        {
            if (w is Gun g) return (g.CurrentAmmoCountInMagazine, g.CurrentAmmoCount);
            if (w is GrenadeLauncher gl) return (gl.CurrentAmmoCountInMagazine, gl.CurrentAmmoCount);
            if (w is HackGun hg) return (hg.CurrentAmmoCountInMagazine, hg.CurrentAmmoCount);
            return (0, 0);
        }
        public static WeaponStatView GetWeaponStat(BaseWeapon w)
        {
            if (w is Gun g)
            {
                var s = g.GunData.GunStat;
                return new WeaponStatView(s.Damage, s.Rpm, s.Recoil, s.WeightPenalty, g.MaxAmmoCountInMagazine);
            }
            if (w is GrenadeLauncher gl)
            {
                var s = gl.GrenadeData.GrenadeStat;
                return new WeaponStatView(s.Damage, s.Rpm, s.Recoil, s.WeightPenalty, gl.MaxAmmoCountInMagazine);
            }
            if (w is HackGun hg)
            {
                var s = hg.HackData.HackStat;
                 return new WeaponStatView(s.Damage, s.Rpm, s.Recoil, s.WeightPenalty, hg.MaxAmmoCountInMagazine);
            }
            return new WeaponStatView();
        }
        public static SlotType GetSlotTypeFromWeapon(BaseWeapon weapon)
        {
            if (weapon is Gun gun)
                return gun.GunData.GunStat.Type == WeaponType.Pistol ? SlotType.Pistol : SlotType.Main;
            if (weapon is GrenadeLauncher)
                return SlotType.GrenadeLauncher;
            if (weapon is HackGun)
                return SlotType.HackGun;
            return SlotType.Main;
        }
    }
}