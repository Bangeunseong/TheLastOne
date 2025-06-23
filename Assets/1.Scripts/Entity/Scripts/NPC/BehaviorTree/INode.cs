namespace _1.Scripts.Entity.Scripts.NPC.BehaviorTree
{
    /// <summary>
    /// 노드 인터페이스
    /// </summary>
    public interface INode
    {
        public enum State
        {
            RUN,
            SUCCESS,
            FAILED
        }

        public INode.State Evaluate(BTContext context); // 판단하여 상태 리턴
    }
}