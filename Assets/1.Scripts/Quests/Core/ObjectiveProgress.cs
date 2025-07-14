using System;
using _1.Scripts.Manager.Core;
using _1.Scripts.Quests.Data;

namespace _1.Scripts.Quests.Core
{
    [Serializable] public class ObjectiveProgress : IGameEventListener
    {
        public ObjectiveData data;
        public int currentAmount;
        public bool IsActivated;
        public bool IsCompleted => currentAmount >= data.requiredAmount;
        
        public void Activate()
        {
            IsActivated = true;
            GameEventSystem.Instance.RegisterListener(this);
        }

        public void Deactivate()
        {
            IsActivated = false;
            GameEventSystem.Instance.UnregisterListener(this);
        }

        public void OnEventRaised(int eventID)
        {
            if (IsCompleted) return;

            if (data.targetID == eventID)
            {
                currentAmount++;
                CoreManager.Instance.gameManager.Player.PlayerCondition.UpdateLastSavedTransform();
                CoreManager.Instance.SaveData_QueuedAsync();
                Service.Log($"[Objective] {data.description} 진행도: {currentAmount}/{data.requiredAmount}");
            }
        }
    }
}