using System.Collections;
using System.Collections.Generic;
using _1.Scripts.Entity.Scripts.Enemy.BehaviorTree;
using UnityEngine;

namespace _1.Scripts.Entity.Scripts.Enemy
{
    /// <summary>
    /// Enemy 공통 동작 정의
    /// </summary>
    public abstract class BaseEnemy : MonoBehaviour
    {
        protected SelectorNode rootNode; // 최상위 셀렉터 노드
        // 적 스탯 정보 SO 필요
        
        protected virtual void Awake()
        {
            rootNode = new SelectorNode();
        }

        protected virtual void Update()
        {
            rootNode.Evaluate();
        }

        // 각 몬스터가 자신의 행동 트리를 정의하도록 강제
        protected abstract void BuildTree();
    }
}
    
