using _1.Scripts.Entity.Scripts.NPC.Data.AnimationHashData;
using _1.Scripts.Entity.Scripts.NPC.Data.ForRuntime;
using _1.Scripts.Entity.Scripts.NPC.Data.StatDataSO;
using _1.Scripts.Entity.Scripts.Npc.StatControllers.Base;
using _1.Scripts.Entity.Scripts.NPC.StatControllers.Base;
using _1.Scripts.Manager.Core;
using _1.Scripts.Static;

namespace _1.Scripts.Entity.Scripts.NPC.StatControllers.Dog
{
    public class DogStatController : BaseNpcStatController
    {
        private RuntimeDogStatData runtimeDogStatData;
        
        protected override void Awake()
        {
            base.Awake();
            var dogStatData = CoreManager.Instance.resourceManager.GetAsset<DogStatData>("DogStatData"); // 자신만의 데이터 가져오기
            runtimeDogStatData = new RuntimeDogStatData(dogStatData);
            runtimeStatData = runtimeDogStatData;
        }
        // 이후 runtimeSuicideDroneStatData 수정 시 베이스에 반영
        
        protected override void OnDisable()
        {
            base.OnDisable();
            runtimeDogStatData.ResetStats();
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            animator.SetTrigger(DroneAnimationData.Idle1);
        }

        protected override void PlayHitAnimation()
        {
            if (!IsStunned) behaviorTree.SetVariableValue(BehaviorNames.ShouldAlertNearBy, true);
        }

        protected override void HackingFailurePenalty()
        {
            
        }

        public override void DisposeAllUniTasks()
        {
            base.DisposeAllUniTasks();
        }
    }
}