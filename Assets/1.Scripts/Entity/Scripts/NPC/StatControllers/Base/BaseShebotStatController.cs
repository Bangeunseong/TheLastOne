using System;
using System.Collections;
using System.Collections.Generic;
using _1.Scripts.Entity.Scripts.NPC.DamageConvert;
using _1.Scripts.Entity.Scripts.Npc.StatControllers.Base;
using UnityEngine;

namespace _1.Scripts.Entity.Scripts.NPC.StatControllers.Base
{
    public abstract class BaseShebotStatController : BaseNpcStatController
    {
        [Header("DamageConvert")]
        [SerializeField] private DamageConvertForNpc[] damageConvertForNpc;
        
        private void Reset()
        {
            damageConvertForNpc = GetComponentsInChildren<DamageConvertForNpc>();
        }

        protected override void Awake()
        {
            base.Awake();
            foreach (DamageConvertForNpc convert in damageConvertForNpc)
            {
                convert.Initialize(this);
            }

            rootRenderer = this.transform;
        }
    }
}