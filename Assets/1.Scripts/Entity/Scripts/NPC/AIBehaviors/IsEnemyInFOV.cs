using System.Collections;
using System.Collections.Generic;
using _1.Scripts.Entity.Scripts.Common;
using _1.Scripts.Entity.Scripts.NPC.AIControllers.Base;
using _1.Scripts.Entity.Scripts.NPC.BehaviorTree;
using _1.Scripts.Entity.Scripts.NPC.Data;
using _1.Scripts.Interfaces;
using _1.Scripts.Manager.Core;
using _1.Scripts.Util;
using UnityEngine;

namespace _1.Scripts.Entity.Scripts.NPC.AIBehaviors
{
    /// <summary>
    /// 육안으로 보이는 곳에 있는지 검사
    /// </summary>
    public class IsEnemyInFOV : INode 
    {
        public INode.State Evaluate(BaseNpcAI controller)
        {
            if (NpcUtil.IsTargetVisible(controller.MyPos, 
                    controller.targetPos, 100f, 
                    controller.statController.RuntimeStatData.isAlly))
            {
                return INode.State.SUCCESS;
            }
            return INode.State.FAILED;
        }
    }   
}