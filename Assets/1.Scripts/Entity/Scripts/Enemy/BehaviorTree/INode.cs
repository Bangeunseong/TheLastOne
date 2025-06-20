namespace _1.Scripts.Entity.Scripts.Enemy.BehaviorTree
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

        public INode.State Evaluate(); // 판단하여 상태 리턴
    }
}