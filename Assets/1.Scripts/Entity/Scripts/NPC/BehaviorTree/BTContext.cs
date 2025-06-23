using System.Collections;
using System.Collections.Generic;
using _1.Scripts.Entity.Scripts.NPC.AIControllers;
using UnityEngine;

public class BTContext
{
    public BaseNpcAI controller;   // 현재 몬스터의 컨트롤러
    public Transform myPosition;   // 현재 내 위치
    public Transform target;       // 플레이어 또는 목표
    // 필요한 거 계속 추가
}