using System.Collections;
using System.Collections.Generic;
using System.Threading;
using _1.Scripts.Entity.Scripts.NPC.Data.AnimationHashData;
using _1.Scripts.Entity.Scripts.NPC.Data.ForRuntime;
using _1.Scripts.Entity.Scripts.NPC.Data.StatDataSO;
using _1.Scripts.Entity.Scripts.NPC.Shebot_Weapon;
using _1.Scripts.Entity.Scripts.NPC.StatControllers.Base;
using _1.Scripts.Manager.Core;
using _1.Scripts.Static;
using _1.Scripts.Util;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace _1.Scripts.Entity.Scripts.NPC.StatControllers.Shebots
{
    public class ShebotSwordStatController : BaseShebotStatController
    {
        private RuntimeShebotSwordStatData runtimeShebotSwordStatData;
        
        [Header("hackingFailPenalty")]
        [SerializeField] protected int hackingFailAttackIncrease = 3;
        [SerializeField] protected float hackingFailArmorIncrease = 3f;
        [SerializeField] protected float hackingFailPenaltyDuration = 10f;
        private CancellationTokenSource penaltyToken;

        [Header("Weapons")]
        private Shebot_Shield shield;
        
        protected override void Awake()
        {
            base.Awake();
            var shebotSwordStatData = CoreManager.Instance.resourceManager.GetAsset<ShebotSwordStatData>("ShebotSwordStatData"); // 자신만의 데이터 가져오기
            runtimeShebotSwordStatData = new RuntimeShebotSwordStatData(shebotSwordStatData);
            runtimeStatData = runtimeShebotSwordStatData;

            shield = GetComponentInChildren<Shebot_Shield>(true);
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            runtimeShebotSwordStatData.ResetStats();
            shield.DisableShield();
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
            base.PlayDeathAnimation();
            animator.SetTrigger(ShebotAnimationHashData.Shebot_Die);
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
        
        protected override void HackingSuccess()
        {
            base.HackingSuccess();
            behaviorTree.SetVariableValue(BehaviorNames.ShieldUsedOnce, false);
        }
    }
}
