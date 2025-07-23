using System.Collections;
using System.Collections.Generic;
using _1.Scripts.Entity.Scripts.NPC.StatControllers.Base;
using UnityEngine;

namespace _1.Scripts.Entity.Scripts.NPC.StatControllers.Shebots
{
    public class ShebotSwordStatController : BaseShebotStatController
    {
        protected override void Awake()
        {
            base.Awake();
            // 스탯 받아서 넣기
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            // 스탯 리셋
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            // 애니메이션 초기상태로 되돌리기
        }
        
        protected override void PlayHitAnimation()
        {
            // 맞았을때 (소드는 해당X)
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
