using _1.Scripts.Entity.Scripts.Common;
using _1.Scripts.Interfaces;
using UnityEngine;

namespace _1.Scripts.Entity.Scripts.Npc.StatControllers
{
    /// <summary>
    /// Npc 스텟 공통로직 정의
    /// </summary>
    public abstract class BaseNpcStat : MonoBehaviour, IDamagable
    {
        public EntityStatData statData;
        
        public abstract void OnTakeDamage(int damage); // 데미지 입을 시 
    }
}