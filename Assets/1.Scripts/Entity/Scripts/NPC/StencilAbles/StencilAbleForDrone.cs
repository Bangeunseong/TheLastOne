using System;
using System.Collections;
using System.Collections.Generic;
using _1.Scripts.Entity.Scripts.Npc.StatControllers.Base;
using _1.Scripts.Static;
using _1.Scripts.Util;
using UnityEngine;

namespace _1.Scripts.Entity.Scripts.NPC.StencilAbles
{
    public class StencilAbleForDrone : MonoBehaviour
    {
        private BaseNpcStatController statController;
        private GameObject rootRenderer;
        
        private void Awake()
        {
            statController = GetComponent<BaseNpcStatController>();
            rootRenderer = this.TryGetChildComponent<GameObject>("body");
        }

        public void StencilLayerOnOrNot(bool isOn)
        {
            if (isOn)
            {
                int layerMask = statController.RuntimeStatData.IsAlly ? LayerConstants.StencilAlly : LayerConstants.StencilEnemy;
                NpcUtil.SetLayerRecursively(rootRenderer, layerMask);
            }
            else
            {
                int layerMask = statController.RuntimeStatData.IsAlly ? LayerConstants.Ally : LayerConstants.Enemy;
                NpcUtil.SetLayerRecursively(rootRenderer, layerMask);
            }
        }
    }
    
}
