using System.Collections;
using System.Collections.Generic;
using _1.Scripts.Entity.Scripts.NPC.Data.AnimationHashData;
using _1.Scripts.Entity.Scripts.NPC.Data.ForRuntime;
using _1.Scripts.Entity.Scripts.NPC.Data.StatDataSO;
using _1.Scripts.Entity.Scripts.NPC.StatControllers.Base;
using _1.Scripts.Manager.Core;
using _1.Scripts.Static;
using UnityEngine;

namespace _1.Scripts.Entity.Scripts.NPC.StatControllers.Shebots
{
    public class ShebotSwordStatController : BaseShebotStatController
    {
        private RuntimeShebotSwordStatData runtimeShebotSwordStatData;
        
        protected override void Awake()
        {
            base.Awake();
            var shebotSwordStatData = CoreManager.Instance.resourceManager.GetAsset<ShebotSwordStatData>("ShebotSwordStatData"); // 자신만의 데이터 가져오기
            runtimeShebotSwordStatData = new RuntimeShebotSwordStatData(shebotSwordStatData);
            runtimeStatData = runtimeShebotSwordStatData;
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            runtimeShebotSwordStatData.ResetStats();
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            animator.SetTrigger(ShebotAnimationHashData.Shebot_Idle);
        }
        
        protected override void PlayHitAnimation()
        {
            if (!IsStunned) behaviorTree.SetVariableValue(BehaviorNames.ShouldAlertNearBy, true);
        }

        protected override void PlayDeathAnimation()
        {
            // 사망 시 애니메이션
        }

        protected override void HackingFailurePenalty()
        {
            // 해킹패널티
        }
    }
}
