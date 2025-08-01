using System.Threading;
using _1.Scripts.Entity.Scripts.NPC.Data.AnimationHashData;
using _1.Scripts.Entity.Scripts.NPC.Data.ForRuntime;
using _1.Scripts.Entity.Scripts.NPC.Data.StatDataSO;
using _1.Scripts.Entity.Scripts.NPC.StatControllers.Base;
using _1.Scripts.Manager.Core;
using _1.Scripts.Static;
using _1.Scripts.Util;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace _1.Scripts.Entity.Scripts.NPC.StatControllers.Shebots
{
    public class ShebotSniperStatController : BaseShebotStatController
    {
        private RuntimeShebotSniperStatData runtimeShebotSniperStatData;
        private LineRenderer lineRenderer;
        
        protected override void Awake()
        {
            base.Awake();
            var shebotSwordStatData = CoreManager.Instance.resourceManager.GetAsset<ShebotSniperStatData>("ShebotSniperStatData"); // 자신만의 데이터 가져오기
            runtimeShebotSniperStatData = new RuntimeShebotSniperStatData(shebotSwordStatData);
            runtimeStatData = runtimeShebotSniperStatData;
            
            lineRenderer = GetComponent<LineRenderer>();
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            runtimeShebotSniperStatData.ResetStats();
            lineRenderer.enabled = false;
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            animator.SetTrigger(ShebotAnimationData.Shebot_Rifle_Aim);
        }

        protected override void PlayHitAnimation()
        {
            // 맞는 모션 없으므로 넘김
        }

        protected override void PlayDeathAnimation()
        {
            base.PlayDeathAnimation();
            lineRenderer.enabled = false;
        }
        
        protected override void HackingSuccess()
        {
            base.HackingSuccess();
            behaviorTree.SetVariableValue(BehaviorNames.ShootInterval, 0f);
        }
    }
}
