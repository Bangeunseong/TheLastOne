namespace _1.Scripts.Interfaces
{
    public interface IPatrolable
    {
        public float MinWaitingDuration { get; }
        public float MaxWaitingDuration { get; }
        public float MinWanderingDistance { get; }
        public float MaxWanderingDistance { get; }
    }
}