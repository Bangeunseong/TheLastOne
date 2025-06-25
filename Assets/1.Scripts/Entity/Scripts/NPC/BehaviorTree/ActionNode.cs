using System;
using _1.Scripts.Entity.Scripts.NPC.AIControllers;

namespace _1.Scripts.Entity.Scripts.NPC.BehaviorTree
{ 
    /// <summary>
    /// 최하위 노드인 액션 노드, 조건이나 동작 정의
    /// </summary>
    public class ActionNode : INode
    {
        public Func<BaseNpcAI, INode.State> action; // 반환형이 INode.State 인 대리자

        public ActionNode(Func<BaseNpcAI, INode.State> action) // 노드를 생성할 때 매개변수로 대리자를 받음(지정자)
        {
            this.action = action;
        }

        public INode.State Evaluate(BaseNpcAI controller) 
        {
            // 대리자가 null 이 아닐 때 호출, null 인 경우 Failed 반환
            return action?.Invoke(controller) ?? INode.State.FAILED;
        }
    }
}
