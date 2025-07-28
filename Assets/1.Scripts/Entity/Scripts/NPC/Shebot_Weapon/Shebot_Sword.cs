using System;
using System.Collections;
using System.Collections.Generic;
using _1.Scripts.Entity.Scripts.Npc.StatControllers.Base;
using _1.Scripts.Interfaces.Common;
using UnityEngine;

namespace _1.Scripts.Entity.Scripts.NPC.Shebot_Weapon
{
    public class Shebot_Sword : MonoBehaviour
    {
        private BaseNpcStatController statController;
        private bool canhit;

        public void EnableHit()
        {
            canhit = true;
        }

        public void DisableHit()
        {
            canhit = false;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (!canhit) return;

            if (other.TryGetComponent(out IDamagable damagable))
            {
                canhit = false;
                damagable.OnTakeDamage(statController.RuntimeStatData.BaseDamage);
            }
        }
    }
}
