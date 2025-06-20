using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace _1.Scripts.Entity.Scripts.Enemy.StatControllers
{
    /// <summary>
    /// 적 스텟 공통로직 정의
    /// </summary>
    public abstract class BaseEnemyStat : MonoBehaviour, IDamagable
    {
        public abstract void TakeDamage(float damage); // 데미지 입을 시 
    }
}
