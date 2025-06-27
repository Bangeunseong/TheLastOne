using _1.Scripts.Entity.Scripts.Common;
using _1.Scripts.Entity.Scripts.NPC.Data.ForRuntime;
using _1.Scripts.Interfaces;
using _1.Scripts.Interfaces.Common;
using UnityEngine;

namespace _1.Scripts.Entity.Scripts.Npc.StatControllers
{
    /// <summary>
    /// Npc 스텟 공통로직 정의
    /// </summary>
    public abstract class BaseNpcStatController : MonoBehaviour, IDamagable
    {
        /// <summary>
        /// 자식마다 들고있는 런타임 스탯을 부모가 가지고 있도록 함
        /// </summary>
        public abstract RuntimeEntityStatData RuntimeStatData { get; }
        protected Animator animator;
        
        public abstract void OnTakeDamage(int damage); // 데미지 입을 시 

        
        /// <summary>
        /// 자식마다 들고있는 런타임 스탯에 특정 인터페이스가 있는지 검사 후, 그 인터페이스를 반환
        /// </summary>
        /// <param name="result"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public bool TryGetRuntimeStatInterface<T>(out T result) where T : class
        {
            result = null;

            result = RuntimeStatData as T;
            if (result == null)
            {
                Debug.LogWarning($"{GetType().Name}의 RuntimeStatData에 {typeof(T).Name} 인터페이스가 없음");
                return false;
            }

            return true;
        }
        
        // 스탯 관련 효과를 주고 싶을 시엔,
        // 기본 스탯 RuntimeEntityStatData을 건드리는 거라면 virtual 메소드 선언한 뒤 자식에서 재정의 (또는 IDamagable처럼 인터페이스 선언) -> 베이스에 있는건 읽기전용 프로퍼티뿐, 자식에서 직접 수정해야함
        // 만약 자식에서 정의한 스탯을 건드리는 거라면 인터페이스 기반으로 호출해야함 (베이스에서 불가능)
        // 예 : IDetectable 스탯인 detectRange 수정하고자 한다면, 위의 인터페이스 가져가는 메소드 쓴다음 detectable.ReduceRange() 호출해야함
        public virtual void ModifySpeed(float percent)
        {
            
        }

        public virtual void Running(bool isRunning)
        {
            
        }
    }
}