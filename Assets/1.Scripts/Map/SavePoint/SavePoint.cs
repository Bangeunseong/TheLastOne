using System;
using _1.Scripts.Entity.Scripts.Player.Core;
using _1.Scripts.Manager.Core;
using _1.Scripts.Quests.Core;
using UnityEngine;

namespace _1.Scripts.Map.SavePoint
{
    public class SavePoint : MonoBehaviour, IGameEventListener
    {
        [field: Header("Save Point Id")]
        [field: SerializeField] public int Id { get; private set; }

        private void OnEnable()
        {
            GameEventSystem.Instance.RegisterListener(this);
        }

        private void Start()
        {
            var save = CoreManager.Instance.gameManager.SaveData;
            if (save == null || !save.stageInfos.TryGetValue(CoreManager.Instance.sceneLoadManager.CurrentScene, out var info)) return;
            if (info.completionDict.TryGetValue(Id, out var value) && value) enabled = false;
        }

        private void OnDisable()
        {
            GameEventSystem.Instance.UnregisterListener(this);
        }

        private void OnTriggerEnter(Collider other)
        {
            if (!other.CompareTag("Player")) return;
            if (!other.TryGetComponent(out Player player)) return;
            
            player.PlayerCondition.LastSavedPosition = player.transform.position;
            player.PlayerCondition.LastSavedRotation = player.transform.rotation;
            
            GameEventSystem.Instance.RaiseEvent(Id);
        }

        public void OnEventRaised(int eventID)
        {
            var save = CoreManager.Instance.gameManager.SaveData;
            if (save == null ||
                !save.stageInfos.TryGetValue(CoreManager.Instance.sceneLoadManager.CurrentScene, out var info))
            {
                throw new MissingReferenceException("Save file not found!");
            }

            if (!info.completionDict.TryAdd(Id, true))
                info.completionDict[Id] = true;
            
            CoreManager.Instance.SaveData_QueuedAsync();
            enabled = false;
        }
    }
}
