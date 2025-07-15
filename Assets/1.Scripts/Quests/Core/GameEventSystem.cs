using System.Collections.Generic;
using UnityEngine;

namespace _1.Scripts.Quests.Core
{
    public class GameEventSystem : MonoBehaviour
    {
        private static GameEventSystem instance;
        public static GameEventSystem Instance => instance ??= FindObjectOfType<GameEventSystem>();

        private List<IGameEventListener> listeners = new();

        public void RegisterListener(IGameEventListener listener)
        {
            if (!listeners.Contains(listener))
                listeners.Add(listener);
        }

        public void UnregisterListener(IGameEventListener listener)
        {
            if (listeners.Contains(listener))
                listeners.Remove(listener);
        }

        public void RaiseEvent(int eventID)
        {
            foreach (var listener in listeners)
                listener.OnEventRaised(eventID);
        }
    }
}