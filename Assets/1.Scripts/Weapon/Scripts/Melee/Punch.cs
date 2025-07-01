using _1.Scripts.Weapon.Scripts.Common;
using UnityEngine;

namespace _1.Scripts.Weapon.Scripts.Melee
{
    public class Punch : BaseWeapon
    {
        public override void Initialize(GameObject ownerObj)
        {
            
        }

        public override bool OnShoot()
        {
            return false;
        }

        public override bool OnRefillAmmo(int ammo) { return false; }
    }
}