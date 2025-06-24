using System.Collections;
using System.Collections.Generic;
using _1.Scripts.Entity.Scripts.NPC.AIControllers;
using UnityEngine;

namespace _1.Scripts.Entity.Scripts.NPC.BehaviorTree
{
    /// <summary>
    /// 자식 노드들에게 필요한 정보를 제공해주는 자료 모음 클래스
    /// </summary>
    public class BTContext
    {
        public BaseNpcAI controller;   // 현재 NPC의 컨트롤러
        public Transform myPosition;   // 현재 내 위치
        public Transform target;       // 플레이어 또는 목표
        // 필요한 거 계속 추가
    }
}
