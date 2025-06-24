using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace _1.Scripts.Entity.Scripts.Npc.StatControllers
{
    /// <summary>
    /// Npc 스텟 공통로직 정의
    /// </summary>
    public abstract class BaseNpcStat : MonoBehaviour, IDamagable
    {
        public abstract void OnTakeDamage(int damage); // 데미지 입을 시 
    }
}