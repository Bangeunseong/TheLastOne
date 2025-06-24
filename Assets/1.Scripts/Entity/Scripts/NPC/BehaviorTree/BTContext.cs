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
        // 필요한 거 계속 추가
    }
}
