using _1.Scripts.Entity.Scripts.Player.Core;
using _1.Scripts.Manager.Core;
using _1.Scripts.Quests.Core;
using UnityEngine;

namespace _1.Scripts.Map.GameEvents
{
    public class SavePoint : MonoBehaviour, IGameEventListener
    {
        public const int BaseSavePointIndex = 100;

        [field: Header("Save Point Id")]
        [field: Tooltip("It should be same with corresponding Spawn Trigger Id")]
        [field: Range(1, 50)] [field: SerializeField] public int Id { get; private set; }

        private void OnEnable()
        {
            GameEventSystem.Instance.RegisterListener(this);
            Service.Log($"Registered Save Point: { BaseSavePointIndex + Id }");
        }

        private void Start()
        {
            var save = CoreManager.Instance.gameManager.SaveData;
            if (save == null || !save.stageInfos.TryGetValue(CoreManager.Instance.sceneLoadManager.CurrentScene, out var info)) return;
            if (info.completionDict.TryGetValue(BaseSavePointIndex + Id, out var value) && value) enabled = false;
        }

        private void OnDisable()
        {
            GameEventSystem.Instance.UnregisterListener(this);
            Service.Log($"Unregistered Save Point: {BaseSavePointIndex + Id}");
        }

        private void OnTriggerEnter(Collider other)
        {
            if (!other.CompareTag("Player")) return;
            if (!other.TryGetComponent(out Player player)) return;
            
            player.PlayerCondition.LastSavedPosition = player.transform.position;
            player.PlayerCondition.LastSavedRotation = player.transform.rotation;
            
            GameEventSystem.Instance.RaiseEvent(BaseSavePointIndex + Id);
        }

        public void OnEventRaised(int eventID)
        {
            if (eventID != BaseSavePointIndex + Id) return;
            
            var save = CoreManager.Instance.gameManager.SaveData;
            if (save == null ||
                !save.stageInfos.TryGetValue(CoreManager.Instance.sceneLoadManager.CurrentScene, out var info))
            {
                throw new MissingReferenceException("Save file not found!");
            }

            if (!info.completionDict.TryAdd(BaseSavePointIndex + Id, true))
                info.completionDict[BaseSavePointIndex + Id] = true;
            
            CoreManager.Instance.SaveData_QueuedAsync();
            enabled = false;
        }
    }
}
