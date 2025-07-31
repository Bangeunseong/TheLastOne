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
    public class ShebotRifleStatController : BaseShebotStatController
    {
        private RuntimeShebotRifleStatData runtimeShebotRifleStatData;
        
        [Header("hackingFailPenalty")]
        [SerializeField] protected int hackingFailAttackIncrease = 3;
        [SerializeField] protected float hackingFailArmorIncrease = 3f;
        [SerializeField] protected float hackingFailPenaltyDuration = 10f;
        private CancellationTokenSource penaltyToken;
        
        private LineRenderer lineRenderer;
        
        protected override void Awake()
        {
            base.Awake();
            var shebotSwordStatData = CoreManager.Instance.resourceManager.GetAsset<ShebotRifleStatData>("ShebotRifleStatData"); // 자신만의 데이터 가져오기
            runtimeShebotRifleStatData = new RuntimeShebotRifleStatData(shebotSwordStatData);
            runtimeStatData = runtimeShebotRifleStatData;
            
            lineRenderer = GetComponentInChildren<LineRenderer>();
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            runtimeShebotRifleStatData.ResetStats();
            lineRenderer.enabled = false;
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            animator.SetTrigger(ShebotAnimationData.Shebot_Rifle_Aim);
            lineRenderer.enabled = true;
        }

        protected override void PlayHitAnimation()
        {
            // 맞는 모션 없으므로 넘김
        }

        protected override void PlayDeathAnimation()
        {
            base.PlayDeathAnimation();
            lineRenderer.enabled = false;
            animator.SetTrigger(ShebotAnimationData.Shebot_Die);
        }

        protected override void HackingFailurePenalty()
        {
            int baseDamage = runtimeStatData.BaseDamage;
            float baseArmor = runtimeStatData.Armor;
            
            penaltyToken?.Cancel();
            penaltyToken?.Dispose();
            penaltyToken = NpcUtil.CreateLinkedNpcToken();
            
            _= DamageAndArmorIncrease(baseDamage, baseArmor, penaltyToken.Token);
            behaviorTree.SetVariableValue(BehaviorNames.ShouldAlertNearBy, true);
        }
        
        private async UniTaskVoid DamageAndArmorIncrease(int baseDamage, float baseArmor, CancellationToken token)
        {
            runtimeStatData.BaseDamage = baseDamage + hackingFailAttackIncrease;
            runtimeStatData.Armor = baseArmor + hackingFailArmorIncrease;

            await UniTask.WaitForSeconds(hackingFailPenaltyDuration, cancellationToken:token);

            runtimeStatData.BaseDamage = baseDamage;
            runtimeStatData.Armor = baseArmor;
        }
    }
}
