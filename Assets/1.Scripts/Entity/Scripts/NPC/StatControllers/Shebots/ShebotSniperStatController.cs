using System.Threading;
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
        
        [Header("hackingFailPenalty")]
        [SerializeField] protected int hackingFailAttackIncrease = 3;
        [SerializeField] protected float hackingFailArmorIncrease = 3f;
        [SerializeField] protected float hackingFailPenaltyDuration = 10f;
        private CancellationTokenSource penaltyToken;
        
        protected override void Awake()
        {
            base.Awake();
            var shebotSwordStatData = CoreManager.Instance.resourceManager.GetAsset<ShebotSniperStatData>("ShebotSniperStatData"); // 자신만의 데이터 가져오기
            runtimeShebotSniperStatData = new RuntimeShebotSniperStatData(shebotSwordStatData);
            runtimeStatData = runtimeShebotSniperStatData;
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            runtimeShebotSniperStatData.ResetStats();
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            // 애니메이션 초기상태 설정
        }

        protected override void PlayHitAnimation()
        {
            base.PlayHitAnimation();
            // 맞는 모션 설정
        }

        protected override void PlayDeathAnimation()
        {
            base.PlayDeathAnimation();
            // 죽는 모션 설정
        }

        protected override void HackingSuccess()
        {
            base.HackingSuccess();
            // 할게있다면 건들기
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
